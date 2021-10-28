using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMVCapkProcessor.Processor
{
    public class CapkFileSchema
    {
        public const char SplitChar = ',';
        public const int SchemaLength = 8;
        public const int CheckSumSplitLength = 32;
        public const string StartDate = "20210101";
        public const string CapkOutputFile = "capk.txt";

        public string Expiration { get; set; }
        public string HashAlgorithmIndicator { get; set; }
        public string PublicKeyAlgorithmIndicator { get; set; }
        public string RegisterApplicationProviderIdentifier { get; set; }

        public string CAPublicKeyIndex { get; set; }
        public string PublicKeyModulus { get; set; }
        public string PublicKeyExponent { get; set; }
        public string PublicKeyCheckSum { get; set; }
    }
}
