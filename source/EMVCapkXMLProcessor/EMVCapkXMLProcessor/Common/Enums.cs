namespace EMVCapkProcessor.Common
{
    public class Enums
    {
        public enum EMVFile
        {
            [StringValue("UNDEFINED")]
            Undefined = 0,
            [StringValue("Prod_Attended_EMV.xml")]
            Attended = 1,
            [StringValue("Prod_Unattended_EMV.xml")] 
            Unattended = 2
        }
    }
}
