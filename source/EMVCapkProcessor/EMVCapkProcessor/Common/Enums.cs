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
           [StringValue("Prod_Attended_EMV.xml")]
            Attended_XML = 2,
            [StringValue("Unattended_emv.dat")] 
            Unattended = 3,
            [StringValue("Prod_Unattended_EMV.xml")] 
            Unattended_XML = 4 
        }
    }
}
