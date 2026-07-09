using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using DataConcentrator;
using ScadaGUI.Localization;

namespace ScadaGUI
{
    public partial class AddWindow : Window
    {
        private Tag editingTag;
        private Alarm editingAlarm;
        public AddWindow()
        {
            InitializeComponent();

            TypeCombo.ItemsSource = new[] { "AI", "AO", "DI", "DO", "Alarm" };
            TypeCombo.SelectedIndex = 0;

            AnalogInputCombo.ItemsSource = DataConcentratorManager.Instance.Tags
                .OfType<AnalogInput>()
                .Select(t => t.Name)
                .ToList();
        }

        public AddWindow(Tag tagToEdit) : this()
        {
            editingTag = tagToEdit;
            if (editingTag != null)
            {
                NameBox.Text = editingTag.Name;
                NameBox.IsEnabled = false;
                TypeCombo.SelectedItem = editingTag is AnalogInput ? "AI" :
                    editingTag is AnalogOutput ? "AO" :
                    editingTag is DigitalInput ? "DI" : "DO";
                TypeCombo.IsEnabled = false;

                DescriptionBox.Text = editingTag.Description;
                IOAddressBox.Text = editingTag.IOAddress;

                if (editingTag is InputTag input)
                {
                    ScanTimeBox.Text = input.ScanTime.ToString();
                    OnScanCheck.IsChecked = input.OnScan;
                }

                if (editingTag is AnalogInput ai)
                {
                    LowLimitBox.Text = ai.LowLimit.ToString(CultureInfo.InvariantCulture);
                    HighLimitBox.Text = ai.HighLimit.ToString(CultureInfo.InvariantCulture);
                    UnitsBox.Text = ai.Units;
                    DeadbandBox.Text = ai.Deadband.ToString(CultureInfo.InvariantCulture);
                    HysteresisBox.Text = ai.Hysteresis.ToString(CultureInfo.InvariantCulture);
                }

                if (editingTag is OutputTag output)
                {
                    InitialValueBox.Text = output.InitialValue.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        public AddWindow(Alarm alarmToEdit) : this()
        {
            editingAlarm = alarmToEdit;
            if (editingAlarm != null)
            {
                TypeCombo.SelectedItem = "Alarm";
                TypeCombo.IsEnabled = false;
                CommonTagPanel.Visibility = Visibility.Collapsed;
                AlarmPanel.Visibility = Visibility.Visible;

                AnalogInputCombo.ItemsSource = DataConcentratorManager.Instance.Tags
                    .OfType<AnalogInput>()
                    .Select(t => t.Name)
                    .ToList();

                AnalogInputCombo.SelectedItem = editingAlarm.AnalogInputName;
                LimitValueBox.Text = editingAlarm.LimitValue.ToString(CultureInfo.InvariantCulture);
                AboveRadio.IsChecked = editingAlarm.ActivateAbove;
                AlarmMessageBox.Text = editingAlarm.Message;
                AlarmNameBox.Text = editingAlarm.Name;
            }
        }

        private void TypeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CommonTagPanel == null) return;

            string selected = TypeCombo.SelectedItem as string;
            bool isAlarm = selected == "Alarm";
            bool isInput = selected == "AI" || selected == "DI";
            bool isOutput = selected == "AO" || selected == "DO";
            bool isAnalogInput = selected == "AI";

            CommonTagPanel.Visibility = isAlarm ? Visibility.Collapsed : Visibility.Visible;
            InputPanel.Visibility = isInput ? Visibility.Visible : Visibility.Collapsed;
            AnalogInputPanel.Visibility = isAnalogInput ? Visibility.Visible : Visibility.Collapsed;
            OutputPanel.Visibility = isOutput ? Visibility.Visible : Visibility.Collapsed;
            AlarmPanel.Visibility = isAlarm ? Visibility.Visible : Visibility.Collapsed;

            ErrorText.Text = "";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string selected = TypeCombo.SelectedItem as string;

            try
            {
                if (selected == "Alarm")
                {
                    SaveAlarm();
                }
                else
                {
                    SaveTag(selected);
                }

                DialogResult = true;
                Close();
            }
            catch (FormatException)
            {
                ErrorText.Text = LocalizationManager.Instance["FieldRequired"];
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
            }
        }

        private void SaveTag(string type)
        {
            string name = NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(IOAddressBox.Text))
            {
                ErrorText.Text = LocalizationManager.Instance["FieldRequired"];
                return;
            }

            if (editingTag != null)
            {

                editingTag.Description = DescriptionBox.Text;
                editingTag.IOAddress = IOAddressBox.Text.Trim();

                if (editingTag is InputTag existingInput)
                {
                    int newScan = ParseInt(ScanTimeBox.Text);
                    bool newOnScan = OnScanCheck.IsChecked == true;
                    existingInput.ScanTime = newScan;
                    if (existingInput.OnScan != newOnScan)
                    {
                        if (newOnScan)
                            DataConcentratorManager.Instance.StartScan(existingInput);
                        else
                            DataConcentratorManager.Instance.StopScan(existingInput);
                        existingInput.OnScan = newOnScan;
                    }
                }

                if (editingTag is AnalogInput existingAi)
                {
                    existingAi.LowLimit = ParseDouble(LowLimitBox.Text);
                    existingAi.HighLimit = ParseDouble(HighLimitBox.Text);
                    existingAi.Units = UnitsBox.Text;
                    existingAi.Deadband = ParseDouble(DeadbandBox.Text);
                    existingAi.Hysteresis = ParseDouble(HysteresisBox.Text);
                    if (existingAi.LowLimit > existingAi.HighLimit)
                    {
                        ErrorText.Text = LocalizationManager.Instance.CurrentLanguage == "sr"
                            ? "Gornja granica ne može biti manja od donje."
                            : "High limit cannot be less than Low limit.";
                        return;
                    }
                }

                if (editingTag is OutputTag existingOut)
                {
                    existingOut.InitialValue = ParseDouble(InitialValueBox.Text);
                    existingOut.CurrentValue = existingOut.InitialValue;
                    if (existingOut is DigitalOutput dout)
                    {
                        if (dout.InitialValue != 0 && dout.InitialValue != 1)
                        {
                            ErrorText.Text = LocalizationManager.Instance.CurrentLanguage == "sr"
                                ? "Početna vrednost za digitalni izlaz mora biti 0 ili 1."
                                : "Initial value for digital output must be 0 or 1.";
                            return;
                        }
                    }
                }

                DataConcentratorManager.Instance.UpdateTag(editingTag);
                return;
            }


            Tag tag;

            switch (type)
            {
                case "AI":
                    tag = new AnalogInput
                    {
                        LowLimit = ParseDouble(LowLimitBox.Text),
                        HighLimit = ParseDouble(HighLimitBox.Text),
                        Units = UnitsBox.Text,
                        Deadband = ParseDouble(DeadbandBox.Text),
                        Hysteresis = ParseDouble(HysteresisBox.Text),
                        ScanTime = ParseInt(ScanTimeBox.Text),
                        OnScan = OnScanCheck.IsChecked == true
                    };

                    if (tag is AnalogInput aiTag && aiTag.LowLimit > aiTag.HighLimit)
                    {
                        ErrorText.Text = LocalizationManager.Instance.CurrentLanguage == "sr"
                            ? "Gornja granica ne može biti manja od donje."
                            : "High limit cannot be less than Low limit.";
                        return;
                    }
                    break;
                case "DI":
                    tag = new DigitalInput
                    {
                        ScanTime = ParseInt(ScanTimeBox.Text),
                        OnScan = OnScanCheck.IsChecked == true
                    };
                    break;
                case "AO":
                    tag = new AnalogOutput
                    {
                        InitialValue = ParseDouble(InitialValueBox.Text)
                    };
                    break;
                case "DO":
                    tag = new DigitalOutput
                    {
                        InitialValue = ParseDouble(InitialValueBox.Text)
                    };

                    if (tag is DigitalOutput dout)
                    {
                        if (dout.InitialValue != 0 && dout.InitialValue != 1)
                        {
                            ErrorText.Text = LocalizationManager.Instance.CurrentLanguage == "sr"
                                ? "Početna vrednost za digitalni izlaz mora biti 0 ili 1."
                                : "Initial value for digital output must be 0 or 1.";
                            return;
                        }
                    }
                    break;
                default:
                    return;
            }

            if (DataConcentratorManager.Instance.Tags.Any(t => t.Name == name))
            {
                ErrorText.Text = LocalizationManager.Instance["ValidationError"] + ": tag '" + name + "' already exists.";
                return;
            }

            tag.Name = name;
            tag.Description = DescriptionBox.Text;
            tag.IOAddress = IOAddressBox.Text.Trim();
            tag.CurrentValue = tag is OutputTag output ? output.InitialValue : 0;

            DataConcentratorManager.Instance.AddTag(tag);

            if (tag is InputTag input && input.OnScan)
            {
                DataConcentratorManager.Instance.StartScan(input);
            }
        }

        private void SaveAlarm()
        {
            string aiName = AnalogInputCombo.SelectedItem as string;
            if (string.IsNullOrEmpty(aiName) || string.IsNullOrWhiteSpace(LimitValueBox.Text))
            {
                ErrorText.Text = LocalizationManager.Instance["FieldRequired"];
                return;
            }

            var limit = ParseDouble(LimitValueBox.Text);


            var ai = DataConcentratorManager.Instance.Tags.OfType<AnalogInput>().FirstOrDefault(t => t.Name == aiName);
            if (ai != null)
            {
                if (limit < ai.LowLimit || limit > ai.HighLimit)
                {
                    ErrorText.Text = LocalizationManager.Instance.CurrentLanguage == "sr"
                        ? "Vrednost granice alarma mora biti unutar opsega taga."
                        : "Alarm limit must be within the tag's range.";
                    return;
                }
            }

            if (editingAlarm != null)
            {
                editingAlarm.AnalogInputName = aiName;
                editingAlarm.LimitValue = limit;
                editingAlarm.ActivateAbove = AboveRadio.IsChecked == true;
                editingAlarm.Message = AlarmMessageBox.Text;
                editingAlarm.Name = AlarmNameBox.Text?.Trim();
                DataConcentratorManager.Instance.UpdateAlarm(editingAlarm);
                return;
            }

            var alarm = new Alarm
            {
                AnalogInputName = aiName,
                LimitValue = limit,
                ActivateAbove = AboveRadio.IsChecked == true,
                Message = AlarmMessageBox.Text,
                Name = AlarmNameBox.Text?.Trim(),
                State = AlarmState.Inactive
            };

            DataConcentratorManager.Instance.AddAlarm(alarm);
        }

        private static double ParseDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return double.Parse(text, CultureInfo.InvariantCulture);
        }

        private static int ParseInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 1000;
            return int.Parse(text, CultureInfo.InvariantCulture);
        }
    }
}
