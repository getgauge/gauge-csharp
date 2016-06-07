using Autofac;
using NLog;

namespace Gauge.CSharp.Runner.InstanceManagement.ContainerBuilder
{
    public class DiContainerBuilder : IDiContainerBuilder
    {
        private static readonly Logger Logger = LogManager.GetLogger("DiDiContainerBuilder");

        private readonly IAssemblyLoader _assemblyLoader;


        public DiContainerBuilder(IAssemblyLoader assemblyLoader)
        {
            _assemblyLoader = assemblyLoader;
        }


        public IContainer BuildContainer()
        {
            var builder = new Autofac.ContainerBuilder();

            _assemblyLoader.AssembliesReferencingGaugeLib.ForEach(assembly =>
            {
                Logger.Info("Registering Modules in Assembly: " + assembly.FullName);
                builder.RegisterAssemblyModules(assembly);
            });

            return builder.Build();
        }
    }
}
