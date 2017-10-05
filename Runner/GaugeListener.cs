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
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.Messages;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class GaugeListener : IGaugeListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MessageProcessorFactory _messageProcessorFactory;

        public GaugeListener() : this(new MessageProcessorFactory())
        {
        }

        public GaugeListener(MessageProcessorFactory messageProcessorFactory)
        {
            _messageProcessorFactory = messageProcessorFactory;
        }

        public void PollForMessages()
        {
            try
            {
                using (var gaugeConnection = new GaugeConnection(new TcpClientWrapper(Utils.GaugePort)))
                {
                    while (gaugeConnection.Connected)
                    {
                        var message = Message.Parser.ParseFrom(gaugeConnection.ReadBytes().ToArray());
                        var processor = _messageProcessorFactory.GetProcessor(message.MessageType);
                        var response = processor.Process(message);
                        gaugeConnection.WriteMessage(response);
                        if (message.MessageType == Message.Types.MessageType.KillProcessRequest)
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}