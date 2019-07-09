// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Gauge.CSharp.Runner
{
    internal class Logger
    {
        public static string SerializeLogInfo(LogInfo logInfo)
        {
            var serializer = new DataContractJsonSerializer(typeof(LogInfo));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, logInfo);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
        private static void print(String level, String messsage, Boolean isError = false)
        {
            var l = new LogInfo();
            l.logLevel = level;
            l.message = messsage;
            var data = SerializeLogInfo(l);
            if (isError)
            {
                Console.Error.WriteLine(data);
            }
            else
            {
                Console.Out.WriteLine(data);
            }
        }

        internal static void Info(String message)
        {
            print("info", message);
        }
        internal static void Debug(String message)
        {
            print("debug", message);
        }
        internal static void Warning(String message)
        {
            print("warning", message);
        }
        internal static void Error(String message)
        {
            print("error", message, true);
        }
        internal static void Fatal(String message)
        {
            print("fatal", message, true);
            Environment.Exit(1);
        }
    }

    [DataContract]
    internal class LogInfo
    {
        [DataMember]
        public String logLevel { get; set; }

        [DataMember]
        public String message { get; set; }

    }
}