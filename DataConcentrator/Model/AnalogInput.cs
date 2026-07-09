using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    public class AnalogInput : InputTag
    {
        private double lowLimit;
        private double highLimit;
        private string units;
        private double deadband;
        private double hysteresis;

        // Donja granica opsega - samo za analogne tagove.
        public double LowLimit
        {
            get { return lowLimit; }
            set { lowLimit = value; OnPropertyChanged("LowLimit"); }
        }

        // Gornja granica opsega - samo za analogne tagove.
        public double HighLimit
        {
            get { return highLimit; }
            set { highLimit = value; OnPropertyChanged("HighLimit"); }
        }

        // Jedinica mere - samo za analogne tagove.
        public string Units
        {
            get { return units; }
            set { units = value; OnPropertyChanged("Units"); }
        }

        // Na koliku promenu vrednosti se reaguje (samo AI).
        public double Deadband
        {
            get { return deadband; }
            set { deadband = value; OnPropertyChanged("Deadband"); }
        }

        // Histerezis za paljenje alarma (samo AI).
        public double Hysteresis
        {
            get { return hysteresis; }
            set { hysteresis = value; OnPropertyChanged("Hysteresis"); }
        }

        // Alarmi se ne unose prilikom kreiranja taga, vec se naknadno vezuju za AI.
        public virtual ICollection<Alarm> Alarms { get; set; } = new List<Alarm>();

        [NotMapped]
        public override TagType Type => TagType.AI;

        private AlarmState signalState = AlarmState.Inactive;

        // Agregirano stanje svih alarma nad ovim AI - koristi se za signalizaciju
        // bojom u GUI-ju (crveno = Active/neacknowledge-ovan, zuto = Acknowledged).
        [NotMapped]
        public AlarmState SignalState
        {
            get { return signalState; }
            set { signalState = value; OnPropertyChanged("SignalState"); }
        }
    }
}
