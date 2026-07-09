using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace ScadaGUI.Localization
{

    public class LocalizationManager : INotifyPropertyChanged
    {
        private static LocalizationManager instance;
        public static LocalizationManager Instance => instance ?? (instance = new LocalizationManager());

        public event PropertyChangedEventHandler PropertyChanged;

        private string currentLanguage = "sr";

        public string CurrentLanguage
        {
            get => currentLanguage;
            set
            {
                if (currentLanguage == value) return;
                currentLanguage = value;
                CultureInfo.CurrentUICulture = new CultureInfo(value == "sr" ? "sr-Latn-RS" : "en-US");

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        public string this[string key]
        {
            get
            {
                if (translations.TryGetValue(currentLanguage, out var dict) && dict.TryGetValue(key, out var value))
                {
                    return value;
                }
                return key;
            }
        }

        private readonly Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["sr"] = new Dictionary<string, string>
            {
                ["MainWindowTitle"] = "SCADA Aplikacija",
                ["AddTagButton"] = "Dodaj",
                ["AddTagButtonTooltip"] = "Dodaje novi tag ili alarm",
                ["RemoveTagButton"] = "Ukloni",
                ["RemoveTagButtonTooltip"] = "Uklanja selektovani tag",
                ["ReportButton"] = "Izveštaj",
                ["ReportButtonTooltip"] = "Generiše .txt izveštaj sa vrednostima analognih ulaza u opsegu (High+Low)/2 ± 5",
                ["DetailsButton"] = "Detalji",
                ["DetailsButtonTooltip"] = "Prikazuje alarme vezane za ovaj analogni ulaz",
                ["WriteButton"] = "Upiši",
                ["WriteButtonTooltip"] = "Upisuje vrednost u izlaznu veličinu",
                ["ScanColumnHeader"] = "Sken",
                ["ScanCheckboxTooltip"] = "Uključuje/isključuje skeniranje ulaznog taga",
                ["NameColumn"] = "Naziv",
                ["DescriptionColumn"] = "Opis",
                ["TypeColumn"] = "Tip",
                ["AddressColumn"] = "I/O adresa",
                ["ValueColumn"] = "Vrednost",
                ["LanguageLabel"] = "Jezik:",
                ["LanguageLabelTooltip"] = "Bira jezik korisničkog interfejsa",
                ["TimeZoneLabel"] = "Vremenska zona:",
                ["TimeZoneLabelTooltip"] = "Bira vremensku zonu za prikaz vremena alarma",
                ["DateFormatLabel"] = "Format datuma:",
                ["DateFormatLabelTooltip"] = "Bira format prikaza datuma",
                ["AddWindowTitle"] = "Dodaj tag / alarm",
                ["TypeLabel"] = "Tip:",
                ["TypeLabelTooltip"] = "Bira se tip objekta koji se dodaje: AI, AO, DI, DO ili Alarm",
                ["TagNameLabel"] = "Naziv (ID):",
                ["TagNameLabelTooltip"] = "Jedinstveni naziv taga",
                ["IOAddressLabel"] = "I/O adresa:",
                ["IOAddressLabelTooltip"] = "Adresa u PLC simulatoru, npr. ADDR001",
                ["ScanTimeLabel"] = "Scan time (ms):",
                ["ScanTimeLabelTooltip"] = "Interval skeniranja u milisekundama - samo za ulazne tagove",
                ["OnScanLabel"] = "Uključi skeniranje:",
                ["OnScanLabelTooltip"] = "Da li je skeniranje uključeno odmah po kreiranju",
                ["LowLimitLabel"] = "Donja granica:",
                ["LowLimitLabelTooltip"] = "Donja granica opsega - samo za analogne tagove",
                ["HighLimitLabel"] = "Gornja granica:",
                ["HighLimitLabelTooltip"] = "Gornja granica opsega - samo za analogne tagove",
                ["UnitsLabel"] = "Jedinica mere:",
                ["UnitsLabelTooltip"] = "Jedinica mere - samo za analogne tagove",
                ["DeadbandLabel"] = "Deadband:",
                ["DeadbandLabelTooltip"] = "Promena vrednosti na koju se reaguje - samo AI",
                ["HysteresisLabel"] = "Histerezis:",
                ["HysteresisLabelTooltip"] = "Histerezis za paljenje/gašenje alarma - samo AI",
                ["InitialValueLabel"] = "Početna vrednost:",
                ["InitialValueLabelTooltip"] = "Početna vrednost - samo za izlazne tagove",
                ["AnalogInputLabel"] = "Analogni ulaz:",
                ["AnalogInputLabelTooltip"] = "Bira AI na koji se alarm vezuje",
                ["LimitValueLabel"] = "Granica alarma:",
                ["LimitValueLabelTooltip"] = "Vrednost granice pri kojoj se alarm aktivira",
                ["DirectionLabel"] = "Aktivacija:",
                ["DirectionLabelTooltip"] = "Da li se alarm aktivira iznad ili ispod granice",
                ["AboveOption"] = "Iznad granice",
                ["BelowOption"] = "Ispod granice",
                ["MessageLabel"] = "Poruka:",
                ["MessageLabelTooltip"] = "Tekst poruke koja se prikazuje kada se alarm aktivira",
                ["SaveButton"] = "Sačuvaj",
                ["SaveButtonTooltip"] = "Čuva unete podatke",
                ["CancelButton"] = "Otkaži",
                ["CancelButtonTooltip"] = "Zatvara prozor bez čuvanja",
                ["AlarmDetailsTitle"] = "Alarmi",
                ["AcknowledgeButton"] = "Acknowledge",
                ["AcknowledgeButtonTooltip"] = "Potvrđuje da je korisnik video aktivni alarm",
                ["StateColumn"] = "Stanje",
                ["ValidationError"] = "Greška u unosu",
                ["FieldRequired"] = "Sva obavezna polja moraju biti popunjena",
                ["LoginFailed"] = "Neispravno korisničko ime ili lozinka.",
                ["LoginFailedTitle"] = "Greška prijave",
                ["EditButton"] = "Izmeni",
                ["EditButtonTooltip"] = "Izmeni selektovani tag ili njegove alarme",
                ["EditPromptMessage"] = "Izmeniti tag? Da = Izmeni tag, Ne = Upravljaj alarmima",
                ["EditPromptTitle"] = "Izmeni",
                ["SelectTagToEditMessage"] = "Izaberite tag za izmenu.",
                ["AlarmNameLabel"] = "Naziv alarma:",
                ["AlarmNameLabelTooltip"] = "Unesite jedinstveni naziv alarma",
            },
            ["en"] = new Dictionary<string, string>
            {
                ["MainWindowTitle"] = "SCADA Application",
                ["AddTagButton"] = "Add",
                ["AddTagButtonTooltip"] = "Adds a new tag or alarm",
                ["RemoveTagButton"] = "Remove",
                ["RemoveTagButtonTooltip"] = "Removes the selected tag",
                ["ReportButton"] = "Report",
                ["ReportButtonTooltip"] = "Generates a .txt report with analog input values in range (High+Low)/2 ± 5",
                ["DetailsButton"] = "Details",
                ["DetailsButtonTooltip"] = "Shows alarms attached to this analog input",
                ["WriteButton"] = "Write",
                ["WriteButtonTooltip"] = "Writes a value to the output tag",
                ["ScanColumnHeader"] = "Scan",
                ["ScanCheckboxTooltip"] = "Turns scanning of the input tag on/off",
                ["NameColumn"] = "Name",
                ["DescriptionColumn"] = "Description",
                ["TypeColumn"] = "Type",
                ["AddressColumn"] = "I/O address",
                ["ValueColumn"] = "Value",
                ["LanguageLabel"] = "Language:",
                ["LanguageLabelTooltip"] = "Selects the UI language",
                ["TimeZoneLabel"] = "Time zone:",
                ["TimeZoneLabelTooltip"] = "Selects the time zone used to display alarm times",
                ["DateFormatLabel"] = "Date format:",
                ["DateFormatLabelTooltip"] = "Selects the date display format",
                ["AddWindowTitle"] = "Add tag / alarm",
                ["TypeLabel"] = "Type:",
                ["TypeLabelTooltip"] = "Selects the type of object being added: AI, AO, DI, DO or Alarm",
                ["TagNameLabel"] = "Name (ID):",
                ["TagNameLabelTooltip"] = "Unique tag name",
                ["IOAddressLabel"] = "I/O address:",
                ["IOAddressLabelTooltip"] = "Address in the PLC simulator, e.g. ADDR001",
                ["ScanTimeLabel"] = "Scan time (ms):",
                ["ScanTimeLabelTooltip"] = "Scan interval in milliseconds - input tags only",
                ["OnScanLabel"] = "Enable scan:",
                ["OnScanLabelTooltip"] = "Whether scanning starts immediately after creation",
                ["LowLimitLabel"] = "Low limit:",
                ["LowLimitLabelTooltip"] = "Lower range limit - analog tags only",
                ["HighLimitLabel"] = "High limit:",
                ["HighLimitLabelTooltip"] = "Upper range limit - analog tags only",
                ["UnitsLabel"] = "Units:",
                ["UnitsLabelTooltip"] = "Unit of measure - analog tags only",
                ["DeadbandLabel"] = "Deadband:",
                ["DeadbandLabelTooltip"] = "Value change threshold to react to - AI only",
                ["HysteresisLabel"] = "Hysteresis:",
                ["HysteresisLabelTooltip"] = "Hysteresis for turning the alarm on/off - AI only",
                ["InitialValueLabel"] = "Initial value:",
                ["InitialValueLabelTooltip"] = "Initial value - output tags only",
                ["AnalogInputLabel"] = "Analog input:",
                ["AnalogInputLabelTooltip"] = "Selects the AI the alarm is attached to",
                ["LimitValueLabel"] = "Alarm limit:",
                ["LimitValueLabelTooltip"] = "Threshold value that triggers the alarm",
                ["DirectionLabel"] = "Activation:",
                ["DirectionLabelTooltip"] = "Whether the alarm triggers above or below the limit",
                ["AboveOption"] = "Above limit",
                ["BelowOption"] = "Below limit",
                ["MessageLabel"] = "Message:",
                ["MessageLabelTooltip"] = "Message text shown when the alarm triggers",
                ["SaveButton"] = "Save",
                ["SaveButtonTooltip"] = "Saves the entered data",
                ["CancelButton"] = "Cancel",
                ["CancelButtonTooltip"] = "Closes the window without saving",
                ["AlarmDetailsTitle"] = "Alarms",
                ["AcknowledgeButton"] = "Acknowledge",
                ["AcknowledgeButtonTooltip"] = "Confirms the user has seen the active alarm",
                ["StateColumn"] = "State",
                ["ValidationError"] = "Input error",
                ["FieldRequired"] = "All required fields must be filled in",
                ["LoginFailed"] = "Incorrect username or password.",
                ["LoginFailedTitle"] = "Login error",
                ["EditButton"] = "Edit",
                ["EditButtonTooltip"] = "Edit selected tag or its alarms",
                ["EditPromptMessage"] = "Edit tag? Yes = Edit tag, No = Manage alarms",
                ["EditPromptTitle"] = "Edit",
                ["SelectTagToEditMessage"] = "Select a tag to edit.",
                ["AlarmNameLabel"] = "Alarm name:",
                ["AlarmNameLabelTooltip"] = "Enter a unique alarm name",
            }
        };
    }
}
