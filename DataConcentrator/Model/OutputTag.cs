namespace DataConcentrator
{
    // Zajednicka baza za izlazne tagove (AO, DO) - imaju pocetnu (initial) vrednost.
    public abstract class OutputTag : Tag
    {
        private double initialValue;

        // Pocetna vrednost - unosi se samo za izlazne tagove.
        public double InitialValue
        {
            get { return initialValue; }
            set { initialValue = value; OnPropertyChanged("InitialValue"); }
        }
    }
}
