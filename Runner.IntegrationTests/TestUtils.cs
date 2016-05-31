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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
	class TestUtils
	{
		public static string GetIntegrationTestSampleDirectory ()
		{
			/* Rather than assuming what directory integration tests are executing from
			 * We will discover 'up' the IntegrationTestSample project
			*/
			var dir = new DirectoryInfo(Directory.GetCurrentDirectory ());
			return FindIntegrationTestDirectory (dir).FullName;
		}

		static DirectoryInfo FindIntegrationTestDirectory (DirectoryInfo dir)
		{
			var found = dir.GetDirectories ().FirstOrDefault (d => d.Name.Equals ("IntegrationTestSample"));
			if (found != null)
				return found;
			else if (dir.Parent != null)// not on system boundry
				return FindIntegrationTestDirectory(dir.Parent);
			else
				throw new DirectoryNotFoundException ("Failed to find IntegrationTestSample directory");
		}

	}

}
