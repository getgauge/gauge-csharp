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
using System.Diagnostics;
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Exceptions;
using Gauge.Messages;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class StartPhaseExecutor : IPhaseExecutor
    {
        private readonly MessageProcessorFactory _messageProcessorFactory;

        private static StartPhaseExecutor _instance;
        private static readonly Logger logger = LogManager.GetLogger("Build");

        public static StartPhaseExecutor GetDefaultInstance()
        {
            return _instance ?? (_instance = new StartPhaseExecutor());
        }

        private StartPhaseExecutor()
        {
            var customBuildPath = Environment.GetEnvironmentVariable("gauge_custom_build_path");
            if (string.IsNullOrEmpty(customBuildPath))
            {
                try
                {
                    GaugeBuildManager.BuildTargetGaugeProject(logger);
                }
                catch (NotAValidGaugeProjectException)
                {
                    logger.Fatal("Cannot locate a Gauge Project in {0}", Utils.GaugeProjectRoot);
                    Environment.Exit(1);
                }
            }
            ISandbox sandbox;
            try
            {
                sandbox = SandboxFactory.Create();
            }
            catch (InvalidOperationException)
            {
                logger.Warn("No Gauge Assembly found at: {0}", Utils.GetGaugeBinDir());
                GaugeBuildManager.BuildTargetGaugeProject(logger);
                sandbox = SandboxFactory.Create();
            }
            _messageProcessorFactory = new MessageProcessorFactory(sandbox);
        }

        [DebuggerHidden]
        public void Execute()
        {
            try
            {
                using (var gaugeConnection = new GaugeConnection(new TcpClientWrapper(Utils.GaugePort)))
                {
                    while (gaugeConnection.Connected)
                    {
                        var messageBytes = gaugeConnection.ReadBytes();
                        var message = Message.ParseFrom(messageBytes.ToArray());

                        var processor = _messageProcessorFactory.GetProcessor(message.MessageType);
                        var response = processor.Process(message);
                        gaugeConnection.WriteMessage(response);
                        if (message.MessageType == Message.Types.MessageType.KillProcessRequest)
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
            }
        }
    }
}