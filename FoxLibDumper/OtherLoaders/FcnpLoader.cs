using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxLibDumper.OtherLoaders
{
    class FcnpLoader
    {
        public static void ReadHashes(string filePath, ref Dictionary<string, HashSet<string>> hashes, ref List<string> failed)
        {
            using (FileStream readStream = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    FcnpTool.Fcnp fcnp = new FcnpTool.Fcnp();
                    fcnp.Read(readStream);

                    //DumpToJson(filePath, fcnp);

                    hashes["HeaderUnknown"].Add(fcnp.headerEntry.unkStrCode32.ToString());

                    if (fcnp.entries != null && fcnp.entries.Count() != 0)
                    {
                        foreach (var entry in fcnp.entries)
                        {
                            hashes["EntryUnknown"].Add(entry.unkStrCode32.ToString());
                            var parentInfo = entry.parentInfo;
                            hashes["ParentUnknown0"].Add(parentInfo.unkStrCode32_0.ToString());
                            hashes["ParentUnknown1"].Add(parentInfo.unkStrCode32_1.ToString());
                        }
                    }
                } catch (Exception e)
                {
                    readStream.Close();
                    Console.WriteLine($"Could not read {filePath}");
                    failed.Add(filePath);
                }
            }//using
        }//ReadFcnpAndWriteHashes
    }
}
