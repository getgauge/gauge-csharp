using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gauge.CSharp.Runner.InstanceManagement.ContainerBuilder;
using NLog;

namespace Gauge.CSharp.Runner.InstanceManagement
{
    public class DependencyInjectingInstanceManager : IClassInstanceManager
    {
        private static readonly Logger Logger = LogManager.GetLogger("DependencyInjectingInstanceManager");

        private readonly Stack<ILifetimeScope> _scope = new Stack<ILifetimeScope>();
        private readonly IContainer _container;


        public DependencyInjectingInstanceManager(IDiContainerBuilder diContainerBuilder)
        {
            _container = diContainerBuilder.BuildContainer();
        }


        public object Get(Type declaringType)
        {
            return GetCurrentScope().Resolve(declaringType);
        }


        public void StartScope(string tag)
        {
            _scope.Push(GetCurrentScope().BeginLifetimeScope(tag));
            Logger.Debug("Started Scope: " + _scope.Peek());
        }


        public void CloseScope()
        {
            Logger.Debug("End Scope: " + _scope.Peek());
            _scope.Pop().Dispose();
        }


        public void ClearCache()
        {
            //no cache
        }


        private ILifetimeScope GetCurrentScope()
        {
            return _scope.Any() ? _scope.Peek() : _container;
        }
    }
}
