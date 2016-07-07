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

using System.Collections.Generic;
using System.Linq;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    public class SpecExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor
    {
        private readonly ISandbox _sandbox;

        public SpecExecutionStartingProcessor(IMethodExecutor methodExecutor, ISandbox sandbox)
            : base(methodExecutor)
        {
            _sandbox = sandbox;
        }

        protected override ExecutionInfo GetExecutionInfo(Message request)
        {
            return request.SpecExecutionStartingRequest.CurrentExecutionInfo;
        }

        protected override string HookType
        {
            get { return "BeforeSpec"; }
        }

        protected override List<string> GetApplicableTags(Message request)
        {
            return GetExecutionInfo(request).CurrentSpec.TagsList.ToList();
        }

        public override Message Process(Message request)
        {
            _sandbox.StartExecutionScope("spec");
            return base.Process(request);
        }
    }
}