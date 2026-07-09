using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    // Istorijski zapis o tome da se alarm sa datim ID-em desio (aktivirao).
    public class ActivatedAlarm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Id alarma koji se aktivirao.
        public int AlarmId { get; set; }

        [ForeignKey("AlarmId")]
        public virtual Alarm Alarm { get; set; }

        // Naziv velicine nad kojom se desio alarm (denormalizovano radi lakseg izvestaja).
        public string TagName { get; set; }

        // Poruka o alarmu.
        public string Message { get; set; }

        // Vreme kada se alarm desio (UTC - konverzija u odabranu vremensku zonu radi se u GUI-ju).
        public DateTime Timestamp { get; set; }
    }
}
