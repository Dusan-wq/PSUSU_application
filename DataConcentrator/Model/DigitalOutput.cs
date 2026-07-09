using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    public class DigitalOutput : OutputTag
    {
        [NotMapped]
        public override TagType Type => TagType.DO;
    }
}
