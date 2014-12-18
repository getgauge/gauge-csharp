namespace Gauge.CSharp.Runner
{
    public class PhaseExecutorFactory
    {
        public static IPhaseExecutor GetExecutor(string phase)
        {
            switch (phase)
            {
                case "--init":
                    return new SetupPhaseExecutor();
                default:
                    return StartPhaseExecutor.GetDefaultInstance();
            }
        }
    }
}