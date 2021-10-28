using EMVCapkProcessor.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EMVCapkProcessor.Processor
{
    public static class CAPKProcessor
    {
        public static async Task ProcessCapk(Enums.EMVFile target)
        {
            string fileName = FindTargetFile(target);
            if (!string.IsNullOrEmpty(fileName))
            {
                List<CapkFileSchema> capKFileSchema = new List<CapkFileSchema>();

                foreach (string line in File.ReadLines(fileName))
                {
                    if (!line.Contains("CA_KEYS"))
                    {
                        CapkFileSchema schema = null;
                        ParseSchema(line, out schema);
                        if (schema is { })
                        {
                            capKFileSchema.Add(schema);
                        }
                    }
                }

                await ProduceFileOutput(SetupCapkOutputFile(), capKFileSchema);
            }
        }

        private static async Task ProduceFileOutput(string fileName, List<CapkFileSchema> capkFileSchema)
        {
            Console.Write("Creating output file...");

            using (StreamWriter fs = new(fileName))
            {
                string currentRID = string.Empty;
                foreach (CapkFileSchema schema in capkFileSchema)
                {
                    if (schema is { })
                    {
                        if (!currentRID.Equals(schema.RegisterApplicationProviderIdentifier))
                        {
                            await fs.WriteLineAsync($"#--- {RetrieveRIDName(schema.RegisterApplicationProviderIdentifier)} ---#\n[RID]\n{schema.RegisterApplicationProviderIdentifier}\n");
                            currentRID = schema.RegisterApplicationProviderIdentifier;
                        }
                        Console.WriteLine($"# {schema.CAPublicKeyIndex} {schema.PublicKeyCheckSum}");
                        await fs.WriteLineAsync($"# {schema.CAPublicKeyIndex} {schema.PublicKeyCheckSum}");
                        await fs.WriteLineAsync($"[Modulus]");
                        await fs.WriteLineAsync($"{CapkFileSchema.StartDate}-{schema.Expiration.Substring(4, 4) + schema.Expiration.Substring(0, 4)}");

                        string[] pkModulus = SplitByLength(schema.PublicKeyModulus, CapkFileSchema.CheckSumSplitLength);
                        foreach (string value in pkModulus)
                        {
                            await fs.WriteLineAsync(value);
                        }

                        await fs.WriteLineAsync($"\n[Exponent]\n{schema.PublicKeyExponent}\n");
                    }
                }
            }

            Console.WriteLine("DONE!");
        }

        private static string RetrieveRIDName(string ridValue)
        {
            byte[] value = ConversionHelper.HexToByteArray(ridValue);
            byte[] targetArray = new byte[8];
            Array.Copy(value, 0, targetArray, 3, 5);
            Array.Reverse(targetArray);
           return StringValueAttribute.GetStringValue((AppProviderIdentifiers)BitConverter.ToInt64(targetArray, 0));
        }

        private static string[] SplitByLength(this string text, int length) =>
           text.EnumerateByLength(length).ToArray();

        private static bool ParseSchema(string line, out CapkFileSchema schema)
        {
            schema = null;

            string[] split = line.Split(CapkFileSchema.SplitChar);

            if (split.Length == CapkFileSchema.SchemaLength)
            {
                try
                {
                    schema = new CapkFileSchema()
                    {
                        Expiration = split[(int)CapkSchemaIndex.Expiration],
                        HashAlgorithmIndicator = split[(int)CapkSchemaIndex.HashAlgorithmIndicator],
                        PublicKeyAlgorithmIndicator = split[(int)CapkSchemaIndex.PublicKeyAlgorithmIndicator],
                        RegisterApplicationProviderIdentifier = split[(int)CapkSchemaIndex.RegisterApplicationProviderIdentifier],
                        CAPublicKeyIndex = split[(int)CapkSchemaIndex.CAPublicKeyIndex],
                        PublicKeyModulus = split[(int)CapkSchemaIndex.PublicKeyModulus],
                        PublicKeyExponent = split[(int)CapkSchemaIndex.PublicKeyExponent],
                        PublicKeyCheckSum = split[(int)CapkSchemaIndex.PublicKeyCheckSum]
                    };
                }
                catch
                {
                    throw new FormatException($"Invalid CAPK Schema Length");
                }
            }

            return false;
        }

        private static string FindTargetFile(Enums.EMVFile target)
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Assets");
            if (Directory.Exists(filePath))
            {
                string fileName = Path.Combine(filePath, StringValueAttribute.GetStringValue(target));

                if (File.Exists(fileName))
                {
                    return fileName;
                }
            }

            return null;
        }

        private static string SetupCapkOutputFile()
        {
            // Setup output file
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "output");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            return Path.Combine(filePath, CapkFileSchema.CapkOutputFile);
        }
    }
}
