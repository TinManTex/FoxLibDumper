using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using FoxLibLoaders;
using FoxLibDumper.OtherLoaders;
using System.Text.RegularExpressions;

namespace FoxLibDumper
{
    class Program
    {
        static List<string> fileTypes = new List<string> {
            "fcnp",
            "fmdl",
            "frld",
            "frt",
            "fv2",//tex foxlib implementation can't handle all fv2s, have yet to decipher implementation in fv2twool 
            "lba",
        };

        class RunSettings
        {
            public bool outputToFilePath = false; //tex for <somepath><some file> write hashes to <somepath><somefile>_<hashType>Hashes.txt
            public string outputToHashTypeFoldersPath = @"D:\GitHub\mgsv-lookup-strings";

            public string hashTypeDefinitionsPath = @"D:\GitHub\mgsv-lookup-strings";
            public string filesToDumpPath = @"E:\GameData\TPP\assetpath";

            public string gameId = "TPP";
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //ShowUsageInfo();

                RunSettings defaultConfig = new RunSettings();
                JsonSerializerSettings serializeSettings = new JsonSerializerSettings();
                serializeSettings.Formatting = Formatting.Indented;
                string jsonStringOut = JsonConvert.SerializeObject(defaultConfig, serializeSettings);
                string jsonOutPath = Directory.GetCurrentDirectory() + "/default-config.json";
                jsonOutPath = Regex.Replace(jsonOutPath, @"\\", "/");
                File.WriteAllText(jsonOutPath, jsonStringOut);
                Console.WriteLine();
                Console.WriteLine($"Writing default run config to {jsonOutPath}");
                return;
            }


            var runSettings = new RunSettings();


            string configPath = GetPath(args[0]);
            if (configPath == null)
            {
                Console.WriteLine("ERROR: invalid path " + args[0]);
                return;
            }
            if (configPath.Contains(".json"))
            {
                Console.WriteLine("Using run settings " + configPath);
                string jsonString = File.ReadAllText(configPath);
                runSettings = JsonConvert.DeserializeObject<RunSettings>(jsonString);
            }


            if (!Directory.Exists(runSettings.hashTypeDefinitionsPath))
            {
                Console.WriteLine("ERROR: Could not find path " + runSettings.hashTypeDefinitionsPath);
                return;
            }

            if (runSettings.outputToHashTypeFoldersPath != null && !Directory.Exists(runSettings.outputToHashTypeFoldersPath))
            {
                Console.WriteLine("ERROR: Could not find path " + runSettings.outputToHashTypeFoldersPath);
                return;
            }

            // REF: example hash type json
            /*{
                "BoneName": "StrCode64",
                "TexturePath": "PathCode64",
                "ShaderName": "StrCode64",
                "MaterialName": "StrCode64",
                "TextureName": "StrCode64",
                "ParameterName": "StrCode64",
                "MeshGroup": "StrCode64",
                "MeshName": "StrCode64",
            }*/
            Dictionary<string, Dictionary<string, string>> fileTypeHashTypes = new Dictionary<string, Dictionary<string, string>>();
            foreach (string fileType in fileTypes)
            {
                string jsonPath = $"{runSettings.hashTypeDefinitionsPath}\\{fileType}\\{fileType}_hash_types.json";
                if (!File.Exists(jsonPath))
                {
                    Console.WriteLine("WARNING: could not find " + jsonPath);
                } else
                {
                    string jsonString = File.ReadAllText(jsonPath);
                    var hashTypes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                    fileTypeHashTypes.Add(fileType, hashTypes);
                }
            }

            List<string> files = BuildFileList(runSettings.filesToDumpPath);

            if (files.Count == 0)
            {
                Console.WriteLine("No files found");
                return;
            }

            var failed = new List<string>();

            foreach (var filePath in files)
            {
                string fileType = Path.GetExtension(filePath).ToLower();
                fileType = fileType.TrimStart('.');

                bool canDumpType = false;
                foreach (string checkFileType in fileTypes)
                {
                    if (checkFileType == fileType)
                    {
                        canDumpType = true;
                        break;
                    }
                }

                if (!canDumpType)
                {
                    continue;
                }

                Console.WriteLine(filePath);

                var hashTypes = fileTypeHashTypes[fileType];
                var hashes = new Dictionary<string, HashSet<string>>();
                foreach (var entry in hashTypes)
                {
                    hashes.Add(entry.Key, new HashSet<string>());
                }

                switch (fileType)
                {
                    case "lba":
                        GimmickLocatorSetLoader.ReadHashes(filePath, ref hashes, ref failed);
                        break;
                    case "fcnp":
                        FcnpLoader.ReadHashes(filePath, ref hashes, ref failed);
                        break;
                    case "fmdl":
                        FmdlLoader.ReadHashes(filePath, ref hashes, ref failed);
                        break;
                    case "frld":
                        FrldLoader.ReadHashes(filePath, ref hashes, ref failed);
                        break;
                    case "frt":
                        RouteSetLoader.ReadHashes(filePath, ref hashes, ref failed);
                        break;
                    case "fv2":
                        FormVariationLoader.ReadHashes(filePath, ref hashes, ref failed);
                        break;
                    default:
                        break;
                }

                //tex write the categorized hashes we collectted from the file
                foreach (var entry in hashes)
                {
                    var hashName = entry.Key;
                    var hashList = entry.Value.ToList();
                    if (hashList.Count() != 0)
                    {
                        hashList.Sort();

                        string fileName = Path.GetFileName(filePath);
                        string fileDirectory = Path.GetDirectoryName(filePath);

                        if (runSettings.outputToFilePath)
                        {
                            string outputPath = Path.Combine(fileDirectory, $"{fileName}_{hashName}Hashes.txt");
                            File.WriteAllLines(outputPath, hashList);
                        }

                        if (runSettings.outputToHashTypeFoldersPath != null)
                        {
                            string assetsPath = fileName;
                            int isAssetsPath = filePath.IndexOf("Assets");
                            if (isAssetsPath != -1)
                            {
                                assetsPath = filePath.Substring(isAssetsPath);
                            }
                            string gameId = "";
                            if (runSettings.gameId != null && runSettings.gameId != "")
                            {
                                gameId = runSettings.gameId + "\\";
                            }
                            string outputPath = $"{runSettings.outputToHashTypeFoldersPath}\\{fileType}\\Hashes\\{gameId}{hashName}\\{assetsPath}_{hashName}Hashes.txt";
                            if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                            }
                            File.WriteAllLines(outputPath, hashList);
                        }
                    }//if hashList.Count
                }//foreach hashes
            }//foreach files

            if (failed.Count > 0)
            {
                Console.WriteLine(failed.Count + " files failed to read.");
                string outPath = Path.Combine(Path.GetDirectoryName(runSettings.filesToDumpPath), "DumpFailedFiles.txt");
                Console.WriteLine("Writing " + outPath);
                File.WriteAllLines(outPath, failed);
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }//Main

        private static List<string> BuildFileList(string[] paths)
        {
            List<string> files = new List<string>();
            foreach (var filePath in paths)
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
        }//BuildFileList

        private static List<string> BuildFileList(string filePath)
        {
            List<string> files = new List<string>();
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

            return files;
        }//BuildFileList

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

        private static string GetPath(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                if (!Path.IsPathRooted(path))
                {
                    path = Path.GetFullPath(path);
                }
            } else
            {
                path = null;
            }

            return path;
        }//GetPath
    }//Program
}
