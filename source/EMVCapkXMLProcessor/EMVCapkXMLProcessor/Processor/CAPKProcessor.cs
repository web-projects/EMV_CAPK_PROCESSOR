using EMVCapkProcessor.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace EMVCapkProcessor.Processor
{
    public static class CAPKProcessor
    {
        public static async Task ProcessCapk(Enums.EMVFile target)
        {
            string fileName = FindTargetFile(target);

            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(fileName);

                    string strData = xmlDoc.InnerXml;
                    XElement xElement = XElement.Parse(strData);

                    // remove all comments in XML file
                    xElement.DescendantNodes().OfType<XComment>().Remove();

                    // note: In original XML file, remove all attributes in CAPKTable for children to load properly
                    List<CapkXMLSchema> prodCAPKList = xElement.Elements("CAPKRow").Select(d => new CapkXMLSchema
                    {
                        CAPKId = d.Element("CAPKId").Value,
                        RID = d.Element("RID").Value,
                        CAPKIndex = d.Element("CAPKIndex").Value,
                        CAPKModulus = d.Element("CAPKModulus").Value,
                        CAPKExponent = d.Element("CAPKExponent").Value,
                        CAPKExpDate = d.Element("CAPKExpDate").Value,
                        CAPKChecksum = d.Element("CAPKChecksum").Value

                    }).GroupBy(x => new
                    {
                        x.RID,
                        x.CAPKIndex
                    }).Select(x => x.First()).ToList();

                    List<CapkFileSchema> capKFileSchema = new List<CapkFileSchema>();

                    foreach (CapkXMLSchema capk in prodCAPKList)
                    {
                        CapkFileSchema schema = null;
                        ParseSchemaXML(capk, out schema);
                        if (schema is { })
                        {
                            capKFileSchema.Add(schema);
                        }
                    }

                    // write individual CAPK files
                    foreach (CapkFileSchema capk in capKFileSchema)
                    {
                        string filename = string.Concat(capk.RegisterApplicationProviderIdentifier, ".", capk.CAPublicKeyIndex);
                        await ProduceFileCAPKOutput(SetupCapkOutputFile(filename), capk);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"EXCEPTION PROCESSING XML FILE:{ex.Message}");
                    Console.WriteLine($"EXCEPTION PROCESSING XML FILE:{ex.Message}");
                }

                // delete working file
                File.Delete(fileName);
            }
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

        private static string FindTargetFile(Enums.EMVFile target)
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "output");

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (Directory.Exists(filePath))
            {
                string fileName = StringValueAttribute.GetStringValue(target);
                string targetFile = Path.Combine(Constants.TargetDirectory, fileName);

                if (FindEmbeddedResourceByName(StringValueAttribute.GetStringValue(target), targetFile))
                {
                    if (File.Exists(targetFile))
                    {
                        return targetFile;
                    }
                }
            }

            return null;
        }

        private static bool FindEmbeddedResourceByName(string fileName, string fileTarget)
        {
            bool result = false;

            // Main Assembly contains embedded resources
            Assembly mainAssembly = Assembly.GetEntryAssembly();

            foreach (string name in mainAssembly.GetManifestResourceNames())
            {
                if (name.EndsWith(fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    using (Stream stream = mainAssembly.GetManifestResourceStream(name))
                    {
                        BinaryReader br = new BinaryReader(stream);
                        // always create working file
                        FileStream fs = File.Open(fileTarget, FileMode.Create);
                        BinaryWriter bw = new BinaryWriter(fs);
                        byte[] ba = new byte[stream.Length];
                        stream.Read(ba, 0, ba.Length);
                        bw.Write(ba);
                        br.Close();
                        bw.Close();
                        stream.Close();
                        result = true;
                    }
                    break;

                }
            }
            return result;
        }

        #region    --- BUNDLED CAPK FILE OUTPUT ---
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

        #endregion --- BUNDLED CAPK FILE OUTPUT ---

        #region    --- CAPK INDIVIDUAL FILES ---
        private static bool ParseSchemaXML(CapkXMLSchema capk, out CapkFileSchema schema)
        {
            schema = null;

            try
            {
                schema = new CapkFileSchema()
                {
                    // FILENAME COMPOSITION
                    RegisterApplicationProviderIdentifier = capk.RID,
                    CAPublicKeyIndex = capk.CAPKIndex,

                    // FILE CONTENTS
                    PublicKeyModulus = capk.CAPKModulus,
                    PublicKeyExponent = capk.CAPKExponent,
                    Expiration = capk.CAPKExpDate,
                    PublicKeyCheckSum = capk.CAPKChecksum
                    //HashAlgorithmIndicator = split[(int)CapkSchemaIndex.HashAlgorithmIndicator],
                    //PublicKeyAlgorithmIndicator = split[(int)CapkSchemaIndex.PublicKeyAlgorithmIndicator],
                };
            }
            catch
            {
                throw new FormatException($"Invalid CAPK Schema Length");
            }


            return false;
        }

        private static string SetupCapkOutputFile(string filename)
        {
            // Setup output file
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "output");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            return Path.Combine(filePath, filename);
        }

        /// <summary>
        /// FILE FORMAT:
        /// 
        /// 1. 3-byte ASCII decimal length of public key modulus 
        /// 2. ASCII hex representation of public key modulus
        /// 3. 2-byte ASCII decimal length of public key exponent
        /// 4. ASCII hex representation of public key exponent
        /// 5. CAPK Checksum
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="capkFileSchema"></param>
        /// <returns></returns>
        private static async Task ProduceFileCAPKOutput(string fileName, CapkFileSchema capkFileSchema)
        {
            Console.Write($"Creating output file: {fileName}");

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("{0,3}", capkFileSchema.PublicKeyModulus.Length.ToString("D3")));
            sb.Append(capkFileSchema.PublicKeyModulus);
            sb.Append(string.Format("{0,2}", capkFileSchema.PublicKeyExponent.Length.ToString("D2")));
            sb.Append(capkFileSchema.PublicKeyExponent);
            sb.Append(capkFileSchema.PublicKeyCheckSum);

            using (StreamWriter fs = new(fileName))
            {
                await fs.WriteAsync(sb.ToString());
            }

            Console.WriteLine("DONE!");
        }

        #endregion --- CAPK INDIVIDUAL FILES ---
    }
}
