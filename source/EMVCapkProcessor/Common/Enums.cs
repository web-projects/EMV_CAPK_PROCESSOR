namespace EMVCapkProcessor.Common
{
    public class Enums
    {
        public enum EMVFile
        {
            [StringValue("Attended_emv.dat")]
            Attended = 0,
            [StringValue("Unttended_emv.dat")] 
            Unattended = 1
        }
    }
}
