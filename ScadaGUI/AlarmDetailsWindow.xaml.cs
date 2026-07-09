using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DataConcentrator;

namespace ScadaGUI
{
    public partial class AlarmDetailsWindow : Window
    {
        private readonly AnalogInput analogInput;
        private ObservableCollection<Alarm> alarms;

        public AlarmDetailsWindow(AnalogInput analogInput)
        {
            InitializeComponent();
            this.analogInput = analogInput;
            Title = Title + " - " + analogInput.Name;
            Refresh();

            DataConcentratorManager.Instance.AlarmsChanged += OnAlarmsChanged;
        }

        private void Refresh()
        {
            alarms = new ObservableCollection<Alarm>(DataConcentratorManager.Instance.GetAlarmsForTag(analogInput.Name));
            AlarmsGrid.ItemsSource = alarms;
        }

        private void AcknowledgeButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlarmsGrid.SelectedItem is Alarm alarm)
            {
                DataConcentratorManager.Instance.AcknowledgeAlarm(alarm);
                Refresh();
            }
        }

        private void RemoveAlarmButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlarmsGrid.SelectedItem is Alarm alarm)
            {
                DataConcentratorManager.Instance.RemoveAlarm(alarm);
                Refresh();
            }
        }

        private void EditAlarmButton_Click(object sender, RoutedEventArgs e)
        {
            if (AlarmsGrid.SelectedItem is Alarm alarm)
            {
                var win = new AddWindow(alarm) { Owner = this };
                if (win.ShowDialog() == true)
                {
                    Refresh();
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DataConcentratorManager.Instance.AlarmsChanged -= OnAlarmsChanged;
            Close();
        }

        private void OnAlarmsChanged(string aiName)
        {
            if (aiName == analogInput.Name)
            {
                Dispatcher.Invoke(() => Refresh());
            }
        }
    }
}
