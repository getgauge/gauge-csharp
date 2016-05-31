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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Gauge.CSharp.Core;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class SandboxFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ISandbox Create(AppDomainSetup setup)
        {
			if (setup == null)
				throw new ArgumentNullException ("setup");
			
            var sandboxAppDomainSetup = setup ?? new AppDomainSetup { ApplicationBase = Utils.GetGaugeBinDir() };
            Logger.Info("Creating a Sandbox in: {0}", sandboxAppDomainSetup.ApplicationBase);
            try
            {
                var permSet = new PermissionSet(PermissionState.Unrestricted);

				string runnersApplicationBase = setup.ApplicationBase;
				var resolver = new InjectableAssemblyResolver(runnersApplicationBase,Utils.GetGaugeBinDir());

				var sandboxDomain = AppDomain.CreateDomain("Sandbox", AppDomain.CurrentDomain.Evidence, sandboxAppDomainSetup, permSet);
				sandboxDomain.AssemblyResolve += resolver.HandleAssemblyResolveForSandboxDomain;

                var sandbox = (Sandbox)sandboxDomain.CreateInstanceFromAndUnwrap(
                    typeof(Sandbox).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(Sandbox).FullName);

                return sandbox;
            }
            catch (Exception e)
            {
                Logger.Info("Unable to create Sandbox in {0}", sandboxAppDomainSetup.ApplicationBase);
                Logger.Fatal(e.ToString);
                throw;
            }
        }
    }

	[Serializable]
	public class InjectableAssemblyResolver
	{
		string runnersApplicationBase;
		string gaugeBin;

		public InjectableAssemblyResolver(string runnersApplicationBase,string gaugeBin)
		{
			if (gaugeBin == null)
				throw new ArgumentNullException ("gaugeBin");
			if (runnersApplicationBase == null)
				throw new ArgumentNullException ("runnersApplicationBase");
			this.gaugeBin = gaugeBin;
			this.runnersApplicationBase = runnersApplicationBase;
		}

		public Assembly HandleAssemblyResolveForSandboxDomain(object sender, ResolveEventArgs args)
		{
			var shortAssemblyName = args.Name.Substring(0, args.Name.IndexOf(','));
			var gaugeBinPath = Path.Combine(gaugeBin, shortAssemblyName + ".dll");
			// first preference is to load assemblies from gauge-bin/ as User has provided
			if (File.Exists (gaugeBinPath)) {
				return Assembly.LoadFrom (gaugeBinPath);
			} else {
				// but when assembly is missing there, then we should fallback on the runner's libraries
				string runnersPath = Path.Combine(runnersApplicationBase, shortAssemblyName + ".dll");
				if(File.Exists(runnersPath))
					return Assembly.LoadFrom (runnersPath);
				// this will also take care of loading runner's assembly
				runnersPath = Path.Combine(runnersApplicationBase, shortAssemblyName + ".exe");
				if(File.Exists(runnersPath))
					return Assembly.LoadFrom (runnersPath);
			}
			return Assembly.GetExecutingAssembly().FullName == args.Name ? Assembly.GetExecutingAssembly() : null;
		}
	}
}