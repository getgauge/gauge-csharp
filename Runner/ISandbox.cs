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
using Gauge.CSharp.Runner.Strategy;
using Gauge.Messages;

namespace Gauge.CSharp.Runner
{
    public interface ISandbox
    {
        ExecutionResult ExecuteMethod(GaugeMethod gaugeMethod, params object[] args);
        bool TryScreenCapture(out byte[] screenShotBytes);
        IHookRegistry GetHookRegistry();
        List<GaugeMethod> GetStepMethods();
        void InitializeDataStore(string dataStoreType);
        IEnumerable<string> GetStepTexts(GaugeMethod gaugeMethod);
        List<string> GetAllStepTexts();
        void ClearObjectCache();
        IEnumerable<string> GetAllPendingMessages();
        Type GetTargetType(string typeFullName);

		// Used only from tests.
		// Don't return Assembly here! assembly instance returned on sandbox side 
		// would be replaced by assembly instance on runner side, thus making any asserts on it useless.
		string TargetLibAssemblyVersion { get; }
        void StartExecutionScope(string tag);
        void CloseExectionScope();
        ProtoExecutionResult.Builder ExecuteHooks(string hookType, HooksStrategy strategy, IEnumerable<string> applicableTags, ExecutionInfo executionInfo);
        IEnumerable<string> Refactor(GaugeMethod methodInfo, IList<ParameterPosition> parameterPositions, IList<string> parametersList, string newStepValue);
    }
}