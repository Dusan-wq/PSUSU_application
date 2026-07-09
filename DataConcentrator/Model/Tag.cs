using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    // Bazna klasa za sve tagove (AnalogInput, AnalogOutput, DigitalInput, DigitalOutput).
    // Koristi se EF6 Table-Per-Hierarchy (TPH) nasledjivanje - sve cetiri klase se cuvaju
    // u istoj "Tags" tabeli, sa diskriminator kolonom koju EF automatski dodaje.
    public abstract class Tag : INotifyPropertyChanged
    {
        private string name;
        private string description;
        private string ioAddress;
        private double currentValue;

        [Key]
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; OnPropertyChanged("Description"); }
        }

        [Column("IOAddress")]
        public string IOAddress
        {
            get { return ioAddress; }
            set { ioAddress = value; OnPropertyChanged("IOAddress"); }
        }

        // Tekuca vrednost velicine - cuva se u bazu da bi bila dostupna posle restarta aplikacije.
        public double CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; OnPropertyChanged("CurrentValue"); }
        }

        // Vraca TagType enum na osnovu konkretnog tipa (koristi se u GUI-ju za prikaz/filtriranje).
        [NotMapped]
        public abstract TagType Type { get; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            UISync.Post(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)));
        }

        #endregion
    }
}
