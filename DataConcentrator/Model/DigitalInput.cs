using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    public class DigitalInput : InputTag
    {
        [NotMapped]
        public override TagType Type => TagType.DI;
    }
}
