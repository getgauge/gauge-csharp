// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-CSharp.

// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// Gauge-Ruby is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Google.ProtocolBuffers;

namespace Gauge.CSharp.Runner.Communication
{
    public abstract class AbstractGaugeConnection : IDisposable
    {
        protected readonly ITcpClientWrapper TcpClientWrapper;

        protected AbstractGaugeConnection(ITcpClientWrapper tcpClientWrapper)
        {
            TcpClientWrapper = tcpClientWrapper;
        }

        public bool Connected
        {
            get { return TcpClientWrapper.Connected; }
        }

        public void WriteMessage(IMessageLite request)
        {
            var bytes = request.ToByteArray();
            var cos = CodedOutputStream.CreateInstance(TcpClientWrapper.GetStream());
            cos.WriteRawVarint64((ulong) bytes.Length);
            cos.Flush();
            TcpClientWrapper.GetStream().Write(bytes, 0, bytes.Length);
            TcpClientWrapper.GetStream().Flush();
        }

        public IEnumerable<byte> ReadBytes()
        {
            var networkStream = TcpClientWrapper.GetStream();
            var codedInputStream = CodedInputStream.CreateInstance(networkStream);
            var messageLength = codedInputStream.ReadRawVarint64();
            for (ulong i = 0; i < messageLength; i++)
            {
                yield return codedInputStream.ReadRawByte();
            }
        }

        protected static long GenerateMessageId()
        {
            return DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond;
        }

        public void Dispose()
        {
            TcpClientWrapper.Close();
        }
    }
}