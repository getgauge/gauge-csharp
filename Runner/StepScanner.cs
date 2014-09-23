using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib;

namespace Gauge.CSharp.Runner
{
    internal class StepScanner
    {
        private readonly GaugeApiConnection ApiConnection;
        private readonly Hashtable StepTable;

        public StepScanner(GaugeApiConnection apiConnection)
        {
            ApiConnection = apiConnection;
            StepTable = new Hashtable();
        }

        public StepRegistry CreateStepRegistry()
        {
            var enumerateFiles = Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.dll", SearchOption.AllDirectories);
            foreach (var specAssembly in enumerateFiles)
            {
                ScanAssembly(specAssembly);
            }
            return new StepRegistry(StepTable);
        }

        private void ScanAssembly(string specAssembly)
        {
            var assembly = Assembly.LoadFile(specAssembly);
            foreach (var type in assembly.GetTypes())
            {
                ProcessType(type);
            }
        }

        private void ProcessType(Type type)
        {
            foreach (var method in type.GetMethods())
            {
                ProcessMethod(method);
            }
        }

        private void ProcessMethod(MethodInfo method)
        {
            var step = method.GetCustomAttributes<Step>(false);
            foreach (var stepValue in step.SelectMany(s => ApiConnection.GetStepValue(s.Names, false)))
            {
                StepTable.Add(stepValue, method);
            }
        }
    }
}