namespace Gauge.CSharp.Runner
{
    public interface IMethodScanner
    {
        IStepRegistry GetStepRegistry();
        IHookRegistry GetHookRegistry();
    }
}