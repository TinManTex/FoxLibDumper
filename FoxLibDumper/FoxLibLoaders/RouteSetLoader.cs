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
        private static RouteNode CreateDummyRouteNode()
        {
            //var position = new Vector3(1667f, 360.0f, -282.0f);
            var position = new Vector3(-1578.3302f, 354.714447f, -285.961426f);

            var event0 = new RouteEvent((uint)Hashing.StrCode("RelaxedWalk"), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1234");
            var event1 = new RouteEvent(3297759236u, 0, 0, 0, 0, 0, 0, 728838112u, 0, 0, 0, "1234");
            var event2 = new RouteEvent((uint)Hashing.StrCode("VehicleIdle"), 16777985u, 2379809247u, 4106517606u, (uint)Hashing.StrCode(""), (uint)Hashing.StrCode(""), (uint)Hashing.StrCode(""), 0, 0, 0, 0, "1234");

            var events = new List<RouteEvent>() { event0, event1, event2 };
            var node = new RouteNode(position, event0, events);

            return node;
        }

        private static Route CreateDummyRoute()
        {
            var node0 = CreateDummyRouteNode();
            //var node1 = CreateRouteNode();
            //var node2 = CreateRouteNode();

            var routeId = (uint)Hashing.StrCode("rt_quest_d_dummy");
            var nodes = new List<RouteNode>() { node0 };
            return new Route(routeId, nodes);
        }


        /// <summary>
        /// Create a route node.
        /// </summary>
        /// <returns>The created route node.</returns>
        private static RouteNode CreateRouteNode(float x, float y, float z, string snippet)
        {
            var position = new Vector3(x, y, z);

            var event0 = new RouteEvent((uint)Hashing.StrCode("RelaxedWalk"), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1234");

            //var event0 = new RouteEvent((uint)Hashing.StrCode("CautionSquatFire"), 16777473, 1161101791, 1152746673, 1135115795, 1152909810, 0, 0, 0, 0, 0, snippet);
            //var event1 = new RouteEvent((uint)Hashing.StrCode("CautionSquatFire"), 16777473, 1161101791, 1152746673, 1135115795, 1152909810, 0, 0, 0, 0, 0, snippet);
            //var event1 = new RouteEvent(3297759236u, 0, 0, 0, 0, 0, 0, 728838112u, 0, 0, 0, "4160");
            //var event2 = new RouteEvent((uint)Hashing.HashFileNameLegacy("VehicleIdle"), 16777985u, 2379809247u, 4106517606u, (uint)Hashing.HashFileNameLegacy(""), (uint)Hashing.HashFileNameLegacy(""), (uint)Hashing.HashFileNameLegacy(""), 0, 0, 0, 0, ",\"\"]");

            var events = new List<RouteEvent>() { event0 };//,event1, event2 };
            var node = new RouteNode(position, event0, events);

            return node;
        }

        /// <summary>
        /// Create a route.
        /// </summary>
        /// <returns>The created route.</returns>
        private static Route CreateRoute()
        {
            var routeId = (uint)Hashing.StrCode("rt_quest_d_0000_test2");


            var node0 = CreateRouteNode(-1612.843f, 355.072f, -292.099f, "1");
            var node1 = CreateRouteNode(-1637.756f, 355.072f, -295.725f, "2");
            var node2 = CreateRouteNode(-1641.882f, 355.072f, -277.100f, "3");
            var node3 = CreateRouteNode(-1616.792f, 355.072f, -272.854f, "4");

            //var node0 = CreateRouteNode(-1578.3302f, 354.714447f, -285.961426f);
            //var node1 = CreateRouteNode(1667f, 360.0f, -282.0f);
            //var node2 = CreateRouteNode();

            var nodes = new List<RouteNode>() { node0, node1, node2, node3 };
            return new Route(routeId, nodes);
        }

        /// <summary>
        /// Create a routeset.
        /// </summary>
        /// <returns>The created routeset.</returns>
        public static RouteSet CreateRouteSet()
        {
            
            var route0 = CreateRoute(); //CreateDummyRoute();
            //var route1 = CreateRoute();
            //var route2 = CreateRoute();

            var routes = new List<Route>() { route0 };
            var routeset = new RouteSet(routes);
            return routeset;
        }

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
    }
}