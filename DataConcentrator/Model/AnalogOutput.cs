using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    public class AnalogOutput : OutputTag
    {
        [NotMapped]
        public override TagType Type => TagType.AO;
    }
}
