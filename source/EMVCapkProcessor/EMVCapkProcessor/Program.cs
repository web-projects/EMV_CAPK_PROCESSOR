using EMVCapkProcessor.Processor;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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

            List<string> capkFiles = GetApplicationCapkFiles(configuration);
            foreach(string capk in capkFiles)
            { 
                await CAPKProcessor.ProcessCapk(GetApplicationExecutionMode(capk));
            }
        }

        static List<string> GetApplicationCapkFiles(IConfiguration configuration)
        {
            return configuration.GetSection("Application:CAPKFiles")?.GetChildren()?.Select(x => x.Value)?.ToList();
        }

        static EMVFile GetApplicationExecutionMode(string capkFile)
        {
            return GetExecutionMode(capkFile);
        }

        static EMVFile GetExecutionMode(string mode) => mode switch
        {
            "Attended_emv.dat" => EMVFile.Attended,
            "Prod_Attended_EMV.xml" => EMVFile.Attended_XML,
            "Unattended_emv.dat" => EMVFile.Unattended,
            "Prod_Unttended_EMV.xml" => EMVFile.Unattended_XML,
            _ => EMVFile.Undefined
        };
    }
}
