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
        public ExecutionContext ExecutionInfoFrom(Messages.ExecutionInfo currentExecutionInfo) {
            if (currentExecutionInfo == null)
                return new ExecutionContext();
            
            return new ExecutionContext(SpecificationFrom(currentExecutionInfo.CurrentSpec), ScenarioFrom(currentExecutionInfo.CurrentScenario),
                    StepFrom(currentExecutionInfo.CurrentStep));
        }

        public ExecutionContext.Specification SpecificationFrom(Messages.SpecInfo currentSpec)
        {
            return currentSpec != null ? new ExecutionContext.Specification(currentSpec.Name, currentSpec.FileName, currentSpec.IsFailed, currentSpec.Tags.ToArray()) : new ExecutionContext.Specification();
        }

        public ExecutionContext.Scenario ScenarioFrom(Messages.ScenarioInfo currentScenario)
        {
            return currentScenario != null ? new ExecutionContext.Scenario(currentScenario.Name, currentScenario.IsFailed, currentScenario.Tags.ToArray()) : new ExecutionContext.Scenario();
        }

        public ExecutionContext.StepDetails StepFrom(Messages.StepInfo currentStep)
        {
            return currentStep?.Step != null ? new ExecutionContext.StepDetails(currentStep.Step.ActualStepText, currentStep.IsFailed) : new ExecutionContext.StepDetails();
        }
    }
}