using EMVCapkProcessor.Processor;
using System.Threading.Tasks;
using static EMVCapkProcessor.Common.Enums;

namespace EMVCapkProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CAPKProcessor.ProcessCapk(EMVFile.Attended);
        }
    }
}
