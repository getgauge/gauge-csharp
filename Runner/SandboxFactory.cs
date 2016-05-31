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
				/*HACK - This is evil! Runner doesn't need to load users assemblies.
				 * But we cannot avoid this on Windows because ISandbox is declaring MethodInfo
				 * which is passed over .NET remoting, which is forcing users assemblies to be loaded.
				 * If we don't do this then Gauge.CSharp.Runner.ISandbox.GetHookRegistry() will throw up
				 * when called from runners domain.
				 * Mono is less restrictive and ignores that MethodInfo refers/contains foreign assemblies.
				 */
				AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolveForCurrentDomain;

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

		static Assembly HandleAssemblyResolveForCurrentDomain (object sender, ResolveEventArgs args)
		{
			var shortAssemblyName = args.Name.Substring(0, args.Name.IndexOf(','));
			var gaugeBinPath = Path.Combine(Utils.GetGaugeBinDir(), shortAssemblyName + ".dll");
			if (File.Exists (gaugeBinPath)) {
				return Assembly.LoadFrom (gaugeBinPath);
			}
			return Assembly.GetExecutingAssembly().FullName == args.Name ? Assembly.GetExecutingAssembly() : null;
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