using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    // Alarm nad ulaznom analognom velicinom (AI). Alarmi se ne unose prilikom
    // pravljenja taga, vec se naknadno vezuju za odredjeni AI.
    public class Alarm : INotifyPropertyChanged
    {
        private double limitValue;
        private bool activateAbove;
        private string message;
        private AlarmState state = AlarmState.Inactive;
        private string name;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Naziv AI velicine nad kojom je alarm definisan (FK).
        [ForeignKey("AnalogInput")]
        public string AnalogInputName { get; set; }

        public virtual AnalogInput AnalogInput { get; set; }

        // Vrednost granice velicine.
        public double LimitValue
        {
            get { return limitValue; }
            set { limitValue = value; OnPropertyChanged("LimitValue"); }
        }

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }

        // true = alarm se aktivira kada vrednost predje IZNAD granice
        // false = alarm se aktivira kada vrednost padne ISPOD granice
        public bool ActivateAbove
        {
            get { return activateAbove; }
            set { activateAbove = value; OnPropertyChanged("ActivateAbove"); }
        }

        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        // Trenutno stanje alarma: Active, Acknowledged ili Inactive.
        public AlarmState State
        {
            get { return state; }
            set { state = value; OnPropertyChanged("State"); }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            UISync.Post(_ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property)));
        }

        #endregion
    }
}
