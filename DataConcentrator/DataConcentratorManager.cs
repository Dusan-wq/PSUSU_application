using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using PLCSimulator;

namespace DataConcentrator
{
    // Data Concentrator - sadrzi sve trenutne vrednosti velicina i sve informacije
    // o velicinama i alarmima. Na svaku promenu vrednosti velicine ispituje da li
    // je velicina u alarmnoj zoni.
    public class DataConcentratorManager
    {
        private static DataConcentratorManager instance;
        public static DataConcentratorManager Instance => instance ?? (instance = new DataConcentratorManager());

        public ObservableCollection<Tag> Tags { get; } = new ObservableCollection<Tag>();

        // istorija vrednosti AI tagova u toku rada aplikacije - koristi se za Report dugme
        private readonly Dictionary<string, List<Tuple<DateTime, double>>> valueHistory =
            new Dictionary<string, List<Tuple<DateTime, double>>>();

        private readonly Dictionary<string, Thread> scanThreads = new Dictionary<string, Thread>();
        private readonly Dictionary<string, volatileFlag> scanFlags = new Dictionary<string, volatileFlag>();
        private readonly object dbLocker = new object();

        public event Action<Tag> TagValueChanged;
        public event Action<Alarm> AlarmActivated;
        // Raised when alarms for an analog input change (add/remove/acknowledge)
        public event Action<string> AlarmsChanged;

        // pomocna klasa jer C# nema "volatile" lokalne/dictionary vrednosti tipa bool
        private class volatileFlag { public volatile bool Value; }

        private DataConcentratorManager()
        {
        }

        #region Ucitavanje / cuvanje tagova

        public void LoadTagsFromDatabase()
        {
            Tags.Clear();
            List<Tag> loaded;
            lock (dbLocker)
            {
                loaded = ContextClass.Instance.Tags.ToList();
            }

            foreach (var tag in loaded)
            {
                Tags.Add(tag);
                if (tag is AnalogInput ai)
                {
                    valueHistory[ai.Name] = new List<Tuple<DateTime, double>>();
                }
                if (tag is InputTag input && input.OnScan)
                {
                    StartScan(input);
                }
            }
        }

        public void AddTag(Tag tag)
        {
            lock (dbLocker)
            {
                ContextClass.Instance.Tags.Add(tag);
                ContextClass.Instance.SaveChanges();
            }
            Tags.Add(tag);
            if (tag is AnalogInput)
            {
                valueHistory[tag.Name] = new List<Tuple<DateTime, double>>();
            }
            Logger.Log("Add tag", $"{tag.Type} {tag.Name}");
        }

        public void UpdateTag(Tag tag)
        {
            lock (dbLocker)
            {
                ContextClass.Instance.SaveChanges();
            }
            Logger.Log("Update tag", tag.Name);
        }

        public void RemoveTag(Tag tag)
        {
            if (tag is InputTag input)
            {
                StopScan(input);
            }

            lock (dbLocker)
            {
                if (tag is AnalogInput ai)
                {
                    // brisanje alarma koji su vezani za ovaj AI (cascade)
                    var alarmsToRemove = ContextClass.Instance.Alarms.Where(a => a.AnalogInputName == ai.Name).ToList();
                    foreach (var alarm in alarmsToRemove)
                    {
                        ContextClass.Instance.Alarms.Remove(alarm);
                    }
                    valueHistory.Remove(tag.Name);
                }

                ContextClass.Instance.Tags.Remove(tag);
                ContextClass.Instance.SaveChanges();
            }
            Tags.Remove(tag);
            Logger.Log("Remove tag", $"{tag.Type} {tag.Name}");
        }

        #endregion

        #region Alarmi

        public void AddAlarm(Alarm alarm)
        {
            lock (dbLocker)
            {
                ContextClass.Instance.Alarms.Add(alarm);
                ContextClass.Instance.SaveChanges();
            }
            Logger.Log("Add alarm", $"{alarm.AnalogInputName} limit={alarm.LimitValue}");
            try
            {
                AlarmsChanged?.Invoke(alarm.AnalogInputName);
            }
            catch { }
        }

        public void UpdateAlarm(Alarm alarm)
        {
            lock (dbLocker)
            {
                ContextClass.Instance.SaveChanges();
            }
            Logger.Log("Update alarm", $"id={alarm.Id}");
            try
            {
                AlarmsChanged?.Invoke(alarm.AnalogInputName);
            }
            catch { }
        }

        public void RemoveAlarm(Alarm alarm)
        {
            lock (dbLocker)
            {
                ContextClass.Instance.Alarms.Remove(alarm);
                ContextClass.Instance.SaveChanges();
            }
            Logger.Log("Remove alarm", $"id={alarm.Id}");
            try
            {
                AlarmsChanged?.Invoke(alarm.AnalogInputName);
            }
            catch { }
        }

        public IEnumerable<Alarm> GetAlarmsForTag(string analogInputName)
        {
            lock (dbLocker)
            {
                return ContextClass.Instance.Alarms.Where(a => a.AnalogInputName == analogInputName).ToList();
            }
        }

        public void AcknowledgeAlarm(Alarm alarm)
        {
            if (alarm.State == AlarmState.Active)
            {
                lock (dbLocker)
                {
                    alarm.State = AlarmState.Acknowledged;
                    ContextClass.Instance.SaveChanges();
                }
                Logger.Log("Acknowledge alarm", $"id={alarm.Id} tag={alarm.AnalogInputName}");

                var ai = Tags.OfType<AnalogInput>().FirstOrDefault(t => t.Name == alarm.AnalogInputName);
                if (ai != null)
                {
                    var related = GetAlarmsForTag(ai.Name).ToList();
                    ai.SignalState = related.Any(a => a.State == AlarmState.Active)
                        ? AlarmState.Active
                        : related.Any(a => a.State == AlarmState.Acknowledged)
                            ? AlarmState.Acknowledged
                            : AlarmState.Inactive;
                }
                try
                {
                    AlarmsChanged?.Invoke(alarm.AnalogInputName);
                }
                catch { }
            }
        }

        private void CheckAlarms(AnalogInput ai)
        {
            List<Alarm> alarms;

            lock (dbLocker)
            {
                alarms = ContextClass.Instance.Alarms.Local
                    .Where(a => a.AnalogInputName == ai.Name)
                    .ToList();

                // ako Local jos nije popunjen (lazy loading), povuci iz baze
                if (!alarms.Any())
                {
                    alarms = ContextClass.Instance.Alarms.Where(a => a.AnalogInputName == ai.Name).ToList();
                }

                foreach (var alarm in alarms)
                {
                    bool inAlarmZone = alarm.ActivateAbove
                        ? ai.CurrentValue > alarm.LimitValue
                        : ai.CurrentValue < alarm.LimitValue;

                    bool backToNormal = alarm.ActivateAbove
                        ? ai.CurrentValue < alarm.LimitValue - ai.Hysteresis
                        : ai.CurrentValue > alarm.LimitValue + ai.Hysteresis;

                    if (inAlarmZone && alarm.State == AlarmState.Inactive)
                    {
                        alarm.State = AlarmState.Active;

                        var activated = new ActivatedAlarm
                        {
                            AlarmId = alarm.Id,
                            TagName = ai.Name,
                            Message = alarm.Message,
                            Timestamp = DateTime.UtcNow
                        };

                        ContextClass.Instance.ActivatedAlarms.Add(activated);
                        ContextClass.Instance.SaveChanges();

                        Logger.Log("Alarm activated", $"{ai.Name}: {alarm.Message}");
                        AlarmActivated?.Invoke(alarm);
                    }
                    else if (!inAlarmZone && backToNormal && alarm.State != AlarmState.Inactive)
                    {
                        alarm.State = AlarmState.Inactive;
                        ContextClass.Instance.SaveChanges();
                    }
                }
            }

            // agregirano stanje za signalizaciju bojom na glavnom ekranu:
            // crveno ako postoji bar jedan Active (nepotvrdjen) alarm, zuto ako je bar jedan Acknowledged
            if (alarms.Any(a => a.State == AlarmState.Active))
            {
                ai.SignalState = AlarmState.Active;
            }
            else if (alarms.Any(a => a.State == AlarmState.Acknowledged))
            {
                ai.SignalState = AlarmState.Acknowledged;
            }
            else
            {
                ai.SignalState = AlarmState.Inactive;
            }
        }

        #endregion

        #region Skeniranje ulaznih tagova

        public void StartScan(InputTag tag)
        {
            if (scanThreads.ContainsKey(tag.Name))
            {
                return; // vec skenira
            }

            tag.OnScan = true;
            var flag = new volatileFlag { Value = true };
            scanFlags[tag.Name] = flag;

            var thread = new Thread(() => ScanLoop(tag, flag)) { IsBackground = true };
            scanThreads[tag.Name] = thread;
            thread.Start();
        }

        public void StopScan(InputTag tag)
        {
            tag.OnScan = false;
            if (scanFlags.TryGetValue(tag.Name, out var flag))
            {
                flag.Value = false;
            }
            scanThreads.Remove(tag.Name);
            scanFlags.Remove(tag.Name);
        }

        private void ScanLoop(InputTag tag, volatileFlag flag)
        {
            while (flag.Value)
            {
                try
                {
                    double value = PLC.Instance.GetAnalogValue(tag.IOAddress);
                    tag.CurrentValue = value;

                    if (tag is AnalogInput ai)
                    {
                        RecordHistory(ai, value);
                        CheckAlarms(ai);
                    }

                    TagValueChanged?.Invoke(tag);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Scan error on {tag.Name}", ex);
                }

                Thread.Sleep(Math.Max(50, tag.ScanTime));
            }
        }

        private void RecordHistory(AnalogInput ai, double value)
        {
            if (!valueHistory.TryGetValue(ai.Name, out var list))
            {
                list = new List<Tuple<DateTime, double>>();
                valueHistory[ai.Name] = list;
            }

            // Deadband: belezi novu vrednost samo ako se dovoljno promenila u odnosu na poslednju
            if (list.Count == 0 || Math.Abs(list[list.Count - 1].Item2 - value) >= ai.Deadband)
            {
                lock (list)
                {
                    list.Add(Tuple.Create(DateTime.Now, value));
                    if (list.Count > 5000)
                    {
                        list.RemoveAt(0);
                    }
                }
            }
        }

        #endregion

        #region Upis u izlazne tagove

        public void WriteToOutput(OutputTag tag, double value)
        {
            tag.CurrentValue = value;

            if (tag is AnalogOutput)
            {
                PLC.Instance.SetAnalogValue(tag.IOAddress, value);
            }
            else if (tag is DigitalOutput)
            {
                PLC.Instance.SetDigitalValue(tag.IOAddress, value);
            }

            TagValueChanged?.Invoke(tag);
            Logger.Log("Write to tag", $"{tag.Name} = {value}");
        }

        #endregion

        #region Report

        // Generise .txt fajl sa vrednostima analognih ulaza koje su bile u opsegu
        // (HighLimit + LowLimit) / 2 ± 5.
        public void GenerateReport(string filePath)
        {
            using (var writer = new StreamWriter(filePath, false))
            {
                foreach (var tag in Tags.OfType<AnalogInput>())
                {
                    double center = (tag.HighLimit + tag.LowLimit) / 2.0;
                    double lower = center - 5;
                    double upper = center + 5;

                    if (!valueHistory.TryGetValue(tag.Name, out var samples))
                    {
                        continue;
                    }

                    List<Tuple<DateTime, double>> matches;
                    lock (samples)
                    {
                        matches = samples.Where(s => s.Item2 >= lower && s.Item2 <= upper).ToList();
                    }

                    foreach (var sample in matches)
                    {
                        writer.WriteLine($"{tag.Name}\t{sample.Item1:yyyy-MM-dd HH:mm:ss}\t{sample.Item2:F2} {tag.Units}");
                    }
                }
            }

            Logger.Log("Generate report", filePath);
        }

        #endregion
    }
}
