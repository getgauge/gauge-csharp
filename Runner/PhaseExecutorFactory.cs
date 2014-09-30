namespace Gauge.CSharp.Runner
{
    internal class PhaseExecutorFactory
    {
        public static IPhaseExecutor GetExecutor(string phase)
        {
            switch (phase)
            {
                case "--init":
                    return new SetupPhaseExecutor();
                default:
                    return new StartPhaseExecutor();
            }
        }
    }
}