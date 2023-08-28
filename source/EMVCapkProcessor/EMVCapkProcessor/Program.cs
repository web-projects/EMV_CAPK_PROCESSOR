using EMVCapkProcessor.Common;
using EMVCapkProcessor.Processor;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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

            (DirectoryInfo di, IConfiguration configuration) = SetupEnvironment();

            List<string> capkFiles = GetApplicationCapkFiles(configuration);
            foreach(string capk in capkFiles)
            { 
                await CAPKProcessor.ProcessCapk(GetApplicationExecutionMode(capk));
            }

            // delete working directory
            DeleteWorkingDirectory(di);
        }

        private static (DirectoryInfo di, IConfiguration configuration) SetupEnvironment()
        {
            DirectoryInfo di = null;

            // create working directory
            if (!Directory.Exists(Constants.TargetDirectory))
            {
                di = Directory.CreateDirectory(Constants.TargetDirectory);
            }

            // Get appsettings.json config - AddEnvironmentVariables() requires package: Microsoft.Extensions.Configuration.EnvironmentVariables
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return (di, configuration);
        }

        private static void DeleteWorkingDirectory(DirectoryInfo di)
        {
            if (di == null)
            {
                di = new DirectoryInfo(Constants.TargetDirectory);
            }

            if (di != null)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                di.Delete();
            }
            else if (Directory.Exists(Constants.TargetDirectory))
            {
                di = new DirectoryInfo(Constants.TargetDirectory);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                Directory.Delete(Constants.TargetDirectory);
            }
        }

        private static List<string> GetApplicationCapkFiles(IConfiguration configuration)
        {
            return configuration.GetSection("Application:CAPKFiles")?.GetChildren()?.Select(x => x.Value)?.ToList();
        }

        private static EMVFile GetApplicationExecutionMode(string capkFile)
        {
            return GetExecutionMode(capkFile);
        }

        private static EMVFile GetExecutionMode(string mode) => mode switch
        {
            "Attended_emv.dat" => EMVFile.Attended,
            "Prod_Attended_EMV.xml" => EMVFile.Attended_XML,
            "Unattended_emv.dat" => EMVFile.Unattended,
            "Prod_Unttended_EMV.xml" => EMVFile.Unattended_XML,
            _ => EMVFile.Undefined
        };
    }
}
