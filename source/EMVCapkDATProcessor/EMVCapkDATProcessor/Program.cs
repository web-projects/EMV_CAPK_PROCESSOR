using EMVCapkProcessor.Processor;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;
using static EMVCapkProcessor.Common.Enums;

namespace EMVCapkProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"\r\n==========================================================================================");
            Console.WriteLine($"{Assembly.GetEntryAssembly().GetName().Name} - Version {Assembly.GetEntryAssembly().GetName().Version}");
            Console.WriteLine($"==========================================================================================\r\n");

            // Get appsettings.json config - AddEnvironmentVariables() requires package: Microsoft.Extensions.Configuration.EnvironmentVariables
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            await CAPKProcessor.ProcessCapk(GetApplicationExecutionMode(configuration));
        }

        static EMVFile GetApplicationExecutionMode(IConfiguration configuration)
        {
            return GetExecutionMode(configuration.GetValue<string>("Application:CAPKFile"));
        }

        static EMVFile GetExecutionMode(string mode) => mode switch
        {
            "Attended_emv.dat" => EMVFile.Attended,
            "Unattended_emv.dat" => EMVFile.Unattended,
            _ => EMVFile.Undefined
        };
    }
}
