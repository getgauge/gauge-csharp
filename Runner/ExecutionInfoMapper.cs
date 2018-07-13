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

using System.Linq;
using Gauge.CSharp.Lib;

namespace Gauge.CSharp.Runner {

    public class ExecutionInfoMapper {
        public ExecutionContext ExecutionInfoFrom(Messages.ExecutionInfo currentExecutionInfo) {
            if (currentExecutionInfo == null)
                return new ExecutionContext();
            
            return new ExecutionContext(SpecificationFrom(currentExecutionInfo.CurrentSpec), ScenarioFrom(currentExecutionInfo.CurrentScenario),
                    StepFrom(currentExecutionInfo.CurrentStep));
        }

        private ExecutionContext.Specification SpecificationFrom(Messages.SpecInfo currentSpec)
        {
            return currentSpec != null ? new ExecutionContext.Specification(currentSpec.Name, currentSpec.FileName, currentSpec.IsFailed, currentSpec.Tags.ToArray()) : new ExecutionContext.Specification();
        }

        private ExecutionContext.Scenario ScenarioFrom(Messages.ScenarioInfo currentScenario)
        {
            return currentScenario != null ? new ExecutionContext.Scenario(currentScenario.Name, currentScenario.IsFailed, currentScenario.Tags.ToArray()) : new ExecutionContext.Scenario();
        }

        private ExecutionContext.StepDetails StepFrom(Messages.StepInfo currentStep)
        {
            if (currentStep == null || currentStep.Step == null)
                return new ExecutionContext.StepDetails();

            return new ExecutionContext.StepDetails(currentStep.Step.ActualStepText, currentStep.IsFailed);
        }
    }
}