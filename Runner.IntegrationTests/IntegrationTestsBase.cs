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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    public class IntegrationTestsBase
    {
        private readonly string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);
        }

        public static void AssertRunnerDomainDidNotLoadUsersAssembly()
        {
            Assert.AreNotEqual("0.6.999",
                FileVersionInfo.GetVersionInfo(typeof(AfterScenario).Assembly.Location).ProductVersion,
                "Runner's test domain should not load the Gauge.CSharp.Lib assembly with 0.6.999 version");
            // 0.6.999 version should be only loaded in sandbox. 
            // Runner should have its own version, the one we just built in this project
        }

        public static string SerializeTable(Table table)
        {
            var serializer = new DataContractJsonSerializer(typeof(Table));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, table);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
            AssertRunnerDomainDidNotLoadUsersAssembly();
        }
    }
}