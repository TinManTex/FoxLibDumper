using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using static FoxLib.Core;
using static FoxLib.Tpp.RouteSet;

namespace FoxLibLoaders
{
    public static class RouteSetLoader
    {
        /// <summary>
        /// Writes a RouteSet to a .frt file.
        /// </summary>
        /// <param name="routeset">RouteSet to write.</param>
        /// <param name="outputPath">Path of the file to write.</param>
        public static void WriteRouteSet(RouteSet routeset, string outputPath)
        {
            using (var writer = new BinaryWriter(new FileStream(outputPath, FileMode.Create), getEncoding()))
            {
                Action<int> writeEmptyBytes = numberOfBytes => WriteEmptyBytes(writer, numberOfBytes);
                var writeFunctions = new WriteFunctions(writer.Write, writer.Write, writer.Write, writer.Write, writer.Write, writeEmptyBytes);
                Write(writeFunctions, routeset);
            }
        }

        /// <summary>
        /// Reads a .frt file and parses it into a RouteSet.
        /// </summary>
        /// <param name="inputPath">File to read.</param>
        /// <returns>The parsed routeset.</returns>
        public static RouteSet ReadRouteSet(string inputPath)
        {
            using (var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open), getEncoding()))
            {
                Action<int> skipBytes = numberOfBytes => SkipBytes(reader, numberOfBytes);
                var readFunctions = new ReadFunctions(reader.ReadSingle, reader.ReadUInt16, reader.ReadUInt32, reader.ReadInt32, reader.ReadBytes, skipBytes);
                return Read(readFunctions);
            }
        }

        /// <summary>
        /// Writes a number of empty bytes.
        /// </summary>
        /// <param name="writer">The BinaryWriter to use.</param>
        /// <param name="numberOfBytes">The number of empty bytes to write.</param>
        private static void WriteEmptyBytes(BinaryWriter writer, int numberOfBytes)
        {
            writer.Write(new byte[numberOfBytes]);
        }

        /// <summary>
        /// Skip reading a number of bytes.
        /// </summary>
        /// <param name="reader">The BinaryReader to use.</param>
        /// <param name="numberOfBytes">The number of bytes to skip.</param>
        private static void SkipBytes(BinaryReader reader, int numberOfBytes)
        {
            reader.BaseStream.Position += numberOfBytes;
        }

        public static List<uint> GetParams(RouteEvent routeEvent)
        {
            return new List<uint>() { routeEvent.Param1, routeEvent.Param2, routeEvent.Param3, routeEvent.Param4, routeEvent.Param5, routeEvent.Param6, routeEvent.Param7, routeEvent.Param8, routeEvent.Param9, routeEvent.Param10 };
        }

        public static void ReadHashes(string filePath, ref Dictionary<string, HashSet<string>> hashes, ref List<string> failed)
        {
            string fileName = Path.GetFileName(filePath);

            var routeSet = RouteSetLoader.ReadRouteSet(filePath);
            if (routeSet == null)
            {
                Console.WriteLine($"Could not read {filePath}");
                failed.Add(filePath);
                return;
            }

            //DumpToJson(filePath, routeSet);

            //tex for parameter we just have a big ole bag of em with no context to try and see if any are hashes (like SendMessage 1st param).
            //ideally we'll figure out what parameters are actually hashes for a node type, then just dump for <node type>/parameter<n>
            var snippetList = new HashSet<string>();

            foreach (var route in routeSet.Routes)
            {

                hashes["RouteName"].Add(route.Name.ToString());
                foreach (var node in route.Nodes)
                {
                    var eventTypeHash = node.EdgeEvent.EventType;
                    hashes["EdgeEventType"].Add(eventTypeHash.ToString());

                    snippetList.Add(node.EdgeEvent.Snippet.ToString());

                    var parametersEdge = RouteSetLoader.GetParams(node.EdgeEvent);
                    foreach (var param in parametersEdge)
                    {
                        hashes["EdgeParameters"].Add(param.ToString());
                    }

                    foreach (var nodeEvent in node.Events)
                    {
                        eventTypeHash = nodeEvent.EventType;
                        hashes["NodeEventType"].Add(eventTypeHash.ToString());

                        snippetList.Add(nodeEvent.Snippet.ToString());

                        var parametersEv = RouteSetLoader.GetParams(nodeEvent);
                        foreach (var param in parametersEv)
                        {
                            hashes["NodeParameters"].Add(param.ToString());
                        }
                    }
                }
            }

            //Program.WriteHashes(filePath, snippetList, "snippet");
        }//ReadHashes
    }
}