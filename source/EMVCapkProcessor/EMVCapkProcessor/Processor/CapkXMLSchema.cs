namespace EMVCapkProcessor.Processor
{
    public class CapkXMLSchema
    {
        public string CAPKId { get; set; }
        // FILENAME COMPOSITION
        public string RID { get; set; }
        public string CAPKIndex { get; set; }
        // FILE CONTENTS
        public string CAPKModulus { get; set; }
        public string CAPKExponent { get; set; }
        public string CAPKExpDate { get; set; }
        public string CAPKChecksum { get; set; }
    }
}
