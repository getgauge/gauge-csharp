using System;

namespace Gauge.CSharp.Runner.InstanceManagement
{
    public class ClassInstanceManagerFactory
    {

        public IClassInstanceManager Create(IAssemblyLoader assemblyLoader)
        {
            bool usingDi = false;
            Boolean.TryParse(Environment.GetEnvironmentVariable("use_di"), out usingDi);

            return usingDi
                ? (IClassInstanceManager) new DependencyInjectingInstanceManager(new DiContainerBuilder(assemblyLoader))
                : new DefaultClassInstanceManager();
        }

    }
}
