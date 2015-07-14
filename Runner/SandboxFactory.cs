// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Gauge.CSharp.Runner.Communication;

namespace Gauge.CSharp.Runner
{
    public class SandboxFactory
    {
        public static ISandbox Create(AppDomainSetup setup = null)
        {
            var sandboxAppDomainSetup = setup ?? new AppDomainSetup { ApplicationBase = Utils.GetGaugeBinDir() };

            var permSet = new PermissionSet(PermissionState.Unrestricted);

            var sandboxDomain = AppDomain.CreateDomain("Sandbox", null, sandboxAppDomainSetup, permSet);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var sandbox = (Sandbox)sandboxDomain.CreateInstanceFromAndUnwrap(
                typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName,
                typeof(Sandbox).FullName);
            
            sandbox.LoadAssemblyFiles();
            return sandbox;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var shortAssemblyName = args.Name.Substring(0, args.Name.IndexOf(','));
            var fileName = Path.Combine(Utils.GetGaugeBinDir(), shortAssemblyName + ".dll");
            if (File.Exists(fileName))
            {
                return Assembly.LoadFrom(fileName);
            }
            return Assembly.GetExecutingAssembly().FullName == args.Name ? Assembly.GetExecutingAssembly() : null;
        }
    }
}