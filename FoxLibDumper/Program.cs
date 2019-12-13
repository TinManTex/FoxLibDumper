using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PowerCutAreaGimmickLocatorSet = FoxLib.Tpp.GimmickLocatorSet.GimmickLocatorSet.PowerCutAreaGimmickLocatorSet;
using NamedGimmickLocatorSet = FoxLib.Tpp.GimmickLocatorSet.GimmickLocatorSet.NamedGimmickLocatorSet;
using ScaledGimmickLocatorSet = FoxLib.Tpp.GimmickLocatorSet.GimmickLocatorSet.ScaledGimmickLocatorSet;
using Vector4 = FoxLib.Core.Vector4;
using FoxLibLoaders;
using Newtonsoft.Json;
using FoxLib;
using FoxLibDumper.Uigb;
using FoxLibDumper.Uilb;
using FoxLibDumper.Uif;

namespace FoxLibDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> files = BuildFileList(args);

            if (files.Count == 0)
            {
                Console.WriteLine("No files found");
                return;
            }

            var failed = new List<string>();

            foreach (var filePath in files)
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();
                switch (fileExtension)
                {
                    case ".lba":
                        ReadLbaAndWriteHashes(filePath, ref failed);
                        break;
                    case ".frt":
                        ReadFrtAndWriteHashes(filePath, ref failed);
                        break;
                    case ".frld":
                        ReadFrldAndWriteHashes(filePath, ref failed);
                        break;
                    case ".fv2":
                        ReadFv2AndWriteHashes(filePath, ref failed);
                        break;
                    case ".uigb":
                        ReadUigbAndWriteHashes(filePath, ref failed);
                        break;
                    case ".uilb":
                        ReadUilbAndWriteHashes(filePath, ref failed);
                        break;
                    case ".uif":
                        ReadUifAndWriteHashes(filePath, ref failed);
                        break;
                    default:
                        break;
                }
            }

            if (failed.Count > 0)
            {
                string outPath = Path.Combine(Path.GetDirectoryName(args[0]), "DumpFailedFiles.txt");
                Console.WriteLine("Writing " + outPath);
                File.WriteAllLines(outPath, failed);
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static List<string> BuildFileList(string[] args)
        {
            List<string> files = new List<string>();
            foreach (var filePath in args)
            {
                if (File.Exists(filePath))
                {
                    files.Add(filePath);
                } else
                {
                    if (Directory.Exists(filePath))
                    {
                        var dirFiles = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);
                        foreach (var file in dirFiles)
                        {
                            files.Add(file);
                        }
                    }
                }
            }

            return files;
        }

        private static void ReadFrldAndWriteHashes(string filePath, ref List<string> failed)
        {
            Console.WriteLine(filePath);
            uint[] ids = FrldLoader.Read(filePath);
            if (ids == null)
            {
                Console.WriteLine($"Could not read {filePath}");
                failed.Add(filePath);
                return;
            }

            DumpToJson(filePath, ids);

            var hashes = new List<string>();
            foreach (uint hash in ids)
            {
                hashes.Add(hash.ToString());
            }
            if (hashes.Count > 0)
            {
                hashes.Sort();
                File.WriteAllLines(filePath + "_idHashes.txt", hashes);
            }
        }


        private static void ReadFv2AndWriteHashes(string filePath, ref List<string> failed)
        {
            Console.WriteLine(filePath);
            FormVariation.FormVariation formVariation  = FormVariationLoader.Read(filePath);
            if (formVariation == null)
            {
                Console.WriteLine($"Could not read {filePath}");
                failed.Add(filePath);
                return;
            }

            DumpToJson(filePath, formVariation);

            var s32Hashes = new HashSet<string>();
            var p64Hashes = new HashSet<string>();
            foreach (var hash in formVariation.HiddenMeshGroups)
            {
                s32Hashes.Add(hash.ToString());
            }
            foreach (var hash in formVariation.ShownMeshGroups)
            {
                s32Hashes.Add(hash.ToString());
            }
            foreach (var textureSwap in formVariation.TextureSwaps)
            {
                s32Hashes.Add(textureSwap.MaterialInstanceHash.ToString());
                s32Hashes.Add(textureSwap.TextureTypeHash.ToString());
                p64Hashes.Add(textureSwap.TextureFileHash.ToString());
            }
            foreach (var boneAttachment in formVariation.BoneAttachments)
            {
                p64Hashes.Add(boneAttachment.ModelFileHash.ToString());
                p64Hashes.Add(boneAttachment.FrdvFileHash.ToString());
                p64Hashes.Add(boneAttachment.SimFileHash.ToString());
            }
            foreach (var cnpAttachment in formVariation.CNPAttachments)
            {
                p64Hashes.Add(cnpAttachment.ModelFileHash.ToString());
                p64Hashes.Add(cnpAttachment.FrdvFileHash.ToString());
                p64Hashes.Add(cnpAttachment.SimFileHash.ToString());
            }
            if (s32Hashes.Count > 0)
            {
                var list = s32Hashes.ToList();
                list.Sort();
                File.WriteAllLines(filePath + "_StrCode32Hashes.txt", list);
            }
            if (p64Hashes.Count > 0)
            {
                var list = p64Hashes.ToList();
                list.Sort();
                File.WriteAllLines(filePath + "_PathFileNameCode64Hashes.txt", list);
            }
        }

        /// <summary>
        /// Read a .lba file and write the hashes of its locators to files.
        /// </summary>
        /// <param name="filePath">Path of the file to read.</param>
        private static void ReadLbaAndWriteHashes(string filePath, ref List<string> failed)
        {
            Console.WriteLine(filePath);

            var locatorSet = GimmickLocatorSetLoader.ReadLocatorSet(filePath);
            if (locatorSet == null)
            {
                Console.WriteLine($"Could not read {filePath}");
                failed.Add(filePath);
                return;
            }

            DumpToJson(filePath, locatorSet);

            IEnumerable<Vector4> positions = null;

            var dataSets = new HashSet<string>();
            var locatorNames = new HashSet<string>();

            // The locatorSet could be one of three different types.
            // Determine the type and cast it to get access to type-specific fields.
            if (locatorSet.IsPowerCutAreaGimmickLocatorSet)
            {
                var pca = locatorSet as PowerCutAreaGimmickLocatorSet;
                positions = from locator in pca.Locators
                            select locator.Position;
            } else if (locatorSet.IsNamedGimmickLocatorSet)
            {
                var named = locatorSet as NamedGimmickLocatorSet;
                positions = from locator in named.Locators
                            select locator.Position;

                foreach (var locator in named.Locators)
                {
                    dataSets.Add(locator.DataSetName.ToString());
                    locatorNames.Add(locator.LocatorName.ToString());
                }

            } else if (locatorSet.IsScaledGimmickLocatorSet)
            {
                var named = locatorSet as ScaledGimmickLocatorSet;
                positions = from locator in named.Locators
                            select locator.Position;

                foreach (var locator in named.Locators)
                {
                    dataSets.Add(locator.DataSetName.ToString());
                    locatorNames.Add(locator.LocatorName.ToString());
                }
            }
            foreach (var position in positions)
            {
                //tex off Console.WriteLine(position);
            }

            WriteHashes(filePath, locatorNames, "locatorName");
            WriteHashes(filePath, dataSets, "dataSet");
        }

        public static void ReadFrtAndWriteHashes(string filePath, ref List<string> failed)
        {
            Console.WriteLine(filePath);

            string fileName = Path.GetFileName(filePath);
            string outPath;

            var routeSet = RouteSetLoader.ReadRouteSet(filePath);
            if (routeSet == null)
            {
                Console.WriteLine($"Could not read {filePath}");
                failed.Add(filePath);
                return;
            }

            DumpToJson(filePath, routeSet);

            var routeHashes = new List<string>();
            var edgeEventTypeHashes = new HashSet<string>();
            var nodeEventTypeHashes = new HashSet<string>();
            //tex just a big ole bag of paramters with no context to try and see if any are hashes (like SendMessage 1st param).
            var parameterHashes = new HashSet<string>();
            var snippetList = new HashSet<string>();


            foreach (var route in routeSet.Routes)
            {
   
                routeHashes.Add(route.Name.ToString());
                foreach (var node in route.Nodes)
                {
                    var eventTypeHash = node.EdgeEvent.EventType;
                    edgeEventTypeHashes.Add(eventTypeHash.ToString());

                    snippetList.Add(node.EdgeEvent.Snippet.ToString());

                    var parametersEdge = RouteSetLoader.GetParams(node.EdgeEvent);
                    foreach (var param in parametersEdge)
                    {
                        parameterHashes.Add(param.ToString());
                    }

                    foreach (var nodeEvent in node.Events)
                    {
                        eventTypeHash = nodeEvent.EventType;
                        nodeEventTypeHashes.Add(eventTypeHash.ToString());

                        snippetList.Add(nodeEvent.Snippet.ToString());

                        var parametersEv = RouteSetLoader.GetParams(nodeEvent);
                        foreach (var param in parametersEv)
                        {
                            parameterHashes.Add(param.ToString());
                        }
                    }
                }
            }

            Program.WriteList(filePath, routeHashes, "routeName");
            Program.WriteHashes(filePath, edgeEventTypeHashes, "edgeEventType");
            Program.WriteHashes(filePath, nodeEventTypeHashes, "nodeEventType");
            Program.WriteHashes(filePath, parameterHashes, "parameter");
            Program.WriteHashes(filePath, snippetList, "snippet");
        }

        public static void ReadUigbAndWriteHashes(string filePath, ref List<string> failed)
        {
            Console.WriteLine(filePath);

            string fileName = Path.GetFileName(filePath);

            var uigb = UiGraphLoader.ReadUiGraph(filePath);
            DumpToJson(filePath, uigb);

            var strCode32Hashes = new HashSet<string>();
            var nodeTypeHashes = new HashSet<string>();
            var nodeNameHashes = new HashSet<string>();

            foreach(var node in uigb.Nodes)
            {
                if (!nodeTypeHashes.Contains(node.TypeHash.ToString()))
                {
                    nodeTypeHashes.Add(node.TypeHash.ToString());
                }

                if (!nodeNameHashes.Contains(node.NameHash.ToString()))
                {
                    nodeNameHashes.Add(node.NameHash.ToString());
                }
            }

            foreach(var hash in uigb.StrCode32Hashes)
            {
                var hashStr = hash.ToString();
                if (nodeTypeHashes.Contains(hashStr.ToString()))
                {
                    continue;
                }

                if (nodeNameHashes.Contains(hashStr.ToString()))
                {
                    continue;
                }

                strCode32Hashes.Add(hashStr);
            }

            var pathFileNameCode64Hashes = new HashSet<string>();
            foreach(var hash in uigb.PathFileNameCode64Hashes)
            {
                pathFileNameCode64Hashes.Add(hash.ToString());
            }

            Program.WriteHashes(filePath, nodeTypeHashes, "nodeType");
            Program.WriteHashes(filePath, nodeNameHashes, "nodeName");
            Program.WriteHashes(filePath, strCode32Hashes, "StrCode32");
            Program.WriteHashes(filePath, pathFileNameCode64Hashes, "PathFileNameCode64");
        }

        public static void ReadUilbAndWriteHashes(string filePath, ref List<string> failed)
        {
            Console.WriteLine(filePath);

            string fileName = Path.GetFileName(filePath);

            var uilb = UiLayoutLoader.ReadUiLayout(filePath);
            DumpToJson(filePath, uilb);

            var strCode32Hashes = new HashSet<string>();
            foreach (var hash in uilb.StrCode32Hashes)
            {
                strCode32Hashes.Add(hash.ToString());
            }

            var pathFileNameCode64Hashes = new HashSet<string>();
            foreach (var hash in uilb.PathFileNameCode64Hashes)
            {
                pathFileNameCode64Hashes.Add(hash.ToString());
            }
            
            Program.WriteHashes(filePath, strCode32Hashes, "StrCode32");
            Program.WriteHashes(filePath, pathFileNameCode64Hashes, "PathFileNameCode64");
        }

        public static void ReadUifAndWriteHashes(string filePath, ref List<string> failed)
        {
            Console.WriteLine(filePath);

            string fileName = Path.GetFileName(filePath);

            var uif = UiModelLoader.ReadUiModel(filePath);
            DumpToJson(filePath, uif);

            var strCode32Hashes = new HashSet<string>();
            foreach (var hash in uif.StrCode32Hashes)
            {
                strCode32Hashes.Add(hash.ToString());
            }

            var pathFileNameCode64Hashes = new HashSet<string>();
            foreach (var hash in uif.PathFileNameCode64Hashes)
            {
                pathFileNameCode64Hashes.Add(hash.ToString());
            }

            Program.WriteHashes(filePath, strCode32Hashes, "StrCode32");
            Program.WriteHashes(filePath, pathFileNameCode64Hashes, "PathFileNameCode64");
        }

        private static void DumpToJson(string filePath, object dumpObject)
        {
            JsonSerializerSettings serializeSettings = new JsonSerializerSettings();
            serializeSettings.Formatting = Formatting.Indented;
            string jsonString = JsonConvert.SerializeObject(dumpObject, serializeSettings);
            string outPath = filePath + ".json";
            File.WriteAllText(outPath, jsonString);
        }

        public static void WriteList(string filePath, List<string> list, string fileSuffix)
        {
            if (list.Count > 0)
            {
                string fileDirectory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                if (list.Count > 0)
                {
                    string outputPath = Path.Combine(fileDirectory, $"{fileName}_{fileSuffix}Hashes.txt");
                    File.WriteAllLines(outputPath, list.ToArray());
                }
            }
        }

        public static void WriteHashes(string filePath, HashSet<string> hashSet, string fileSuffix)
        {
            if (hashSet.Count > 0)
            {
                string fileDirectory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                List<string> hashes = hashSet.ToList<string>();
                if (hashes.Count > 0)
                {
                    hashes.Sort();
                    string outputPath = Path.Combine(fileDirectory, $"{fileName}_{fileSuffix}Hashes.txt");
                    File.WriteAllLines(outputPath, hashes.ToArray());
                }
            }
        }
    }
}
