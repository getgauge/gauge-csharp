// Copyright 2015 ThoughtWorks, Inc.

// This file is part of Gauge-Java.

// This program is free software.
//
// It is dual-licensed under:
// 1) the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version;
// or
// 2) the Eclipse Public License v1.0.
//
// You can redistribute it and/or modify it under the terms of either license.
// We would then provide copied of each license in a separate .txt file with the name of the license as the title of the file.

using System.Linq;
using Gauge.CSharp.Lib;

namespace Gauge.CSharp.Runner {

    public class ExecutionInfoMapper {
        public ExecutionContext executionInfoFrom(Gauge.Messages.ExecutionInfo currentExecutionInfo) {
            if (currentExecutionInfo == null)
                return new ExecutionContext();
            
            return new ExecutionContext(specificationFrom(currentExecutionInfo.CurrentSpec), scenarioFrom(currentExecutionInfo.CurrentScenario),
                    stepFrom(currentExecutionInfo.CurrentStep));
        }

        public ExecutionContext.Specification specificationFrom(Gauge.Messages.SpecInfo currentSpec) {
            if (currentSpec != null) {
                return new ExecutionContext.Specification(currentSpec.Name, currentSpec.FileName, currentSpec.IsFailed, currentSpec.Tags);
            }
            return new ExecutionContext.Specification();
        }

        public ExecutionContext.Scenario scenarioFrom(Gauge.Messages.ScenarioInfo currentScenario) {
            if (currentScenario != null) {
                return new ExecutionContext.Scenario(currentScenario.Name, currentScenario.IsFailed, currentScenario.Tags);
            }
            return new ExecutionContext.Scenario();
        }

        public ExecutionContext.StepDetails stepFrom(Gauge.Messages.StepInfo currentStep) {
            if (currentStep != null) {
                return new ExecutionContext.StepDetails(currentStep.Step.ActualStepText, currentStep.IsFailed);
            }
            return new ExecutionContext.StepDetails();
        }
    }
}