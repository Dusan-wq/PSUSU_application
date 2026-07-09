using System.ComponentModel;

namespace DataConcentrator
{
    // Zajednicka baza za ulazne tagove (AI, DI) - imaju scan time i on/off scan.
    public abstract class InputTag : Tag
    {
        private int scanTime = 1000;
        private bool onScan;

        // Vreme skeniranja u milisekundama - unosi se samo za ulazne tagove.
        public int ScanTime
        {
            get { return scanTime; }
            set { scanTime = value; OnPropertyChanged("ScanTime"); }
        }

        // Da li je skeniranje ukljuceno za ovaj tag.
        public bool OnScan
        {
            get { return onScan; }
            set { onScan = value; OnPropertyChanged("OnScan"); }
        }
    }
}
