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
using System.Security;
using System.Security.Permissions;
using Gauge.CSharp.Core;
using NLog;

namespace Gauge.CSharp.Runner
{
    [Serializable]
    public class SandboxFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static ISandbox Create(AppDomainSetup setup = null)
        {
            var sandboxAppDomainSetup = setup ?? new AppDomainSetup { ApplicationBase = Utils.GetGaugeBinDir() };
            Logger.Info("Creating a Sandbox in: {0}", sandboxAppDomainSetup.ApplicationBase);
            try
            {
                var permSet = new PermissionSet(PermissionState.Unrestricted);

				var sandboxDomain = AppDomain.CreateDomain("Sandbox", AppDomain.CurrentDomain.Evidence, sandboxAppDomainSetup, permSet);
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
}