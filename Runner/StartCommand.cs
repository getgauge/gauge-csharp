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
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Exceptions;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class StartCommand : IGaugeCommand
    {
        private readonly Func<IGaugeListener> _gaugeListener;
        private readonly Func<IGaugeProjectBuilder> _projectBuilder;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public StartCommand() : this(() => new GaugeListener(), () => new GaugeProjectBuilder())
        {
        }

        public StartCommand(Func<IGaugeListener> gaugeListener, Func<IGaugeProjectBuilder> projectBuilder)
        {
            _gaugeListener = gaugeListener;
            _projectBuilder = projectBuilder;
        }

        [DebuggerHidden]
        public void Execute()
        {
            if (!TryBuild())
            {
                return;
            }
            _gaugeListener.Invoke().PollForMessages();
        }

        private bool TryBuild()
        {
            var customBuildPath = Utils.TryReadEnvValue("GAUGE_CUSTOM_BUILD_PATH");
            if (!string.IsNullOrEmpty(customBuildPath))
                return true;

            try
            {
                return _projectBuilder.Invoke().BuildTargetGaugeProject();
            }
            catch (NotAValidGaugeProjectException)
            {
                Logger.Fatal("Cannot locate a Project File in {0}", Utils.GaugeProjectRoot);
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}