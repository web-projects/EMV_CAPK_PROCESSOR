namespace EMVCapkProcessor.Common
{
    public class Enums
    {
        public enum EMVFile
        {
            [StringValue("UNDEFINED")]
            Undefined = 0,
            [StringValue("Attended_emv.dat")]
            Attended = 1,
            [StringValue("Unattended_emv.dat")] 
            Unattended = 2
        }
    }
}
