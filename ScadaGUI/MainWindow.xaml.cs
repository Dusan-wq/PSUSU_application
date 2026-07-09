using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataConcentrator;
using ScadaGUI.Localization;

namespace ScadaGUI
{
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer settingsTimer;

        public MainWindow()
        {
            InitializeComponent();

            TagsGrid.ItemsSource = DataConcentratorManager.Instance.Tags;

            SetupSettingsControls();

            DataConcentratorManager.Instance.AlarmActivated += OnAlarmActivated;


            settingsTimer = new System.Windows.Threading.DispatcherTimer();
            settingsTimer.Interval = TimeSpan.FromSeconds(1);
            settingsTimer.Tick += (s, e) => UpdateSettingsPreview();
            settingsTimer.Start();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (TagsGrid.SelectedItem is AnalogInput ai)
            {
                string prompt = LocalizationManager.Instance["EditPromptMessage"];
                string title = LocalizationManager.Instance["EditPromptTitle"];
                var res = MessageBox.Show(this, prompt, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    var win = new AddWindow(ai) { Owner = this };
                    win.ShowDialog();
                }
                else if (res == MessageBoxResult.No)
                {
                    var details = new AlarmDetailsWindow(ai) { Owner = this };
                    details.ShowDialog();
                }
                return;
            }

            if (TagsGrid.SelectedItem is Tag tag)
            {
                var win = new AddWindow(tag) { Owner = this };
                win.ShowDialog();
                return;
            }

            MessageBox.Show(this, LocalizationManager.Instance["SelectTagToEditMessage"], LocalizationManager.Instance["EditPromptTitle"], MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetupSettingsControls()
        {
            LanguageCombo.ItemsSource = new[] { "sr", "en" };
            LanguageCombo.SelectedItem = LocalizationManager.Instance.CurrentLanguage;

            TimeZoneCombo.ItemsSource = TimeZoneInfo.GetSystemTimeZones();
            TimeZoneCombo.DisplayMemberPath = "DisplayName";
            TimeZoneCombo.SelectedItem = TimeZoneInfo.GetSystemTimeZones()
                .FirstOrDefault(tz => tz.Id == TimeZoneInfo.Local.Id);

            DateFormatCombo.ItemsSource = new[]
            {
                "dd.MM.yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss"
            };
            DateFormatCombo.SelectedIndex = 0;
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageCombo.SelectedItem is string lang)
            {
                LocalizationManager.Instance.CurrentLanguage = lang;
            }
        }

        private void TimeZoneCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeZoneCombo.SelectedItem is TimeZoneInfo tz)
            {
                AppSettings.Instance.TimeZone = tz;
            }
        }

        private void DateFormatCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateFormatCombo.SelectedItem is string format)
            {
                AppSettings.Instance.DateFormat = format;
            }
        }

        private void UpdateSettingsPreview()
        {
            try
            {
                var tz = AppSettings.Instance.TimeZone ?? TimeZoneInfo.Local;
                var nowUtc = DateTime.UtcNow;
                var tzNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
                TimeZoneNowText.Text = tzNow.ToString("HH:mm:ss");

                var dateFormat = AppSettings.Instance.DateFormat ?? "dd.MM.yyyy HH:mm:ss";
                DateFormatNowText.Text = tzNow.ToString(dateFormat);
            }
            catch
            {

            }
        }

        private void DateFormatNowText_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            DateFormatCombo.Focus();
            DateFormatCombo.IsDropDownOpen = true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddWindow { Owner = this };
            addWindow.ShowDialog();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TagsGrid.SelectedItem is Tag tag)
            {
                DataConcentratorManager.Instance.RemoveTag(tag);
            }
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Report.txt",
                Filter = "Text files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                DataConcentratorManager.Instance.GenerateReport(dialog.FileName);
                MessageBox.Show(this, dialog.FileName, "Report", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is AnalogInput ai)
            {
                var detailsWindow = new AlarmDetailsWindow(ai) { Owner = this };
                detailsWindow.ShowDialog();
            }
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is OutputTag outputTag)
            {
                string text = button.CommandParameter as string;
                if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                {
                    DataConcentratorManager.Instance.WriteToOutput(outputTag, value);
                }
                else
                {
                    MessageBox.Show(this, LocalizationManager.Instance["ValidationError"]);
                }
            }
        }

        private void ScanCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox)?.DataContext is InputTag input)
            {
                DataConcentratorManager.Instance.StartScan(input);
            }
        }

        private void ScanCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox)?.DataContext is InputTag input)
            {
                DataConcentratorManager.Instance.StopScan(input);
            }
        }

        private void OnAlarmActivated(Alarm alarm)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var tag in DataConcentratorManager.Instance.Tags.OfType<InputTag>().ToList())
            {
                DataConcentratorManager.Instance.StopScan(tag);
            }

            PLC.StopSimulator();

            ContextClass.Instance.SaveChanges();
        }

        private void WriteValueBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

            var tb = sender as TextBox;
            if (tb == null) return;


            var row = FindParent<DataGridRow>(tb);
            if (row?.Item is OutputTag output)
            {
                if (output is DigitalOutput)
                {

                    var selectionStart = tb.SelectionStart;
                    var selectionLength = tb.SelectionLength;
                    string current = tb.Text ?? string.Empty;
                    string newText = current.Remove(selectionStart, selectionLength).Insert(selectionStart, e.Text);


                    var trimmed = newText.Trim();
                    e.Handled = !(trimmed == "0" || trimmed == "1");
                    return;
                }
            }


            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',' && c != '-')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void WriteValueBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!(e.Source is TextBox tb)) return;

            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            var pasted = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;

            var row = FindParent<DataGridRow>(tb);
            if (row?.Item is OutputTag output && output is DigitalOutput)
            {

                var selectionStart = tb.SelectionStart;
                var selectionLength = tb.SelectionLength;
                string current = tb.Text ?? string.Empty;
                string newText = current.Remove(selectionStart, selectionLength).Insert(selectionStart, pasted);
                var trimmed = newText.Trim();
                if (trimmed == "0" || trimmed == "1")
                {
                    return;
                }
                e.CancelCommand();
                return;
            }


            foreach (char c in pasted)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',' && c != '-' && !char.IsWhiteSpace(c))
                {
                    e.CancelCommand();
                    return;
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}
