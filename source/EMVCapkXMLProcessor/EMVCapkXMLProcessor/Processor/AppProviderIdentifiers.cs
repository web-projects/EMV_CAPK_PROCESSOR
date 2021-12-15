using EMVCapkProcessor.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMVCapkProcessor.Processor
{
    public enum AppProviderIdentifiers : long
    {
        [StringValue("AMEX")]
        Amex = 0xA000000025,
        [StringValue("DISCOVER")]
        Discover = 0xA000000152, 
        [StringValue("JCB")]
        JCB = 0xA000000065, 
        [StringValue("MASTERCARD")]
        MasterCard = 0xA000000004, 
        [StringValue("UnionPay")]
        UnionPay = 0xA000000333, 
        [StringValue("VISA")]
        Visa = 0xA000000003,
        [StringValue("Interac")]
        Interac = 0xA000000277,
        [StringValue("UNKNOWN-1")] 
        Unknown1 = 0xA000000768,
        [StringValue("UNKNOWN-2")]
        Unknown2 = 0xA000000780
    }
}
