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