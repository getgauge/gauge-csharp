using Autofac;

namespace Gauge.CSharp.Runner.InstanceManagement.ContainerBuilder
{
    public interface IDiContainerBuilder
    {
        IContainer BuildContainer();
    }
}
