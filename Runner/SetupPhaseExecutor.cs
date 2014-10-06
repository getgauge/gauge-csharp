using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib;

namespace Gauge.CSharp.Runner
{
    internal class SetupPhaseExecutor : IPhaseExecutor
    {
        public void Execute()
        {
            if (Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.csproj").Any())
            {
                Console.Out.WriteLine("Directory is not empty, skipping Gauge initialization.");
                Environment.Exit(1);
            }
            Directory.CreateDirectory(Path.Combine(Utils.GaugeProjectRoot, "Properties"));
            new List<string>
            {
                "Gauge.Spec.csproj",
                Path.Combine("Properties", "AssemblyInfo.cs"),
                "StepImplementation.cs",
                "packages.config"
            }.ForEach(CopyFile); 
            InstallDependencies();
        }

        private static void CopyFile(string filePath)
        {
            var skeletonPath = Path.GetFullPath("skel");
            var destFileName = Path.Combine(Utils.GaugeProjectRoot, filePath);
            File.Copy(Path.Combine(skeletonPath, filePath), destFileName, true);
            var fileContent = File.ReadAllText(destFileName).Replace(@"$safeprojectname$", "Gauge.Spec").Replace("$guid1$", Guid.NewGuid().ToString());
            File.WriteAllText(destFileName, fileContent);
            Console.Out.WriteLine(" create  {0}", filePath);
        }

        private static void InstallDependencies()
        {
            Console.Out.WriteLine(" create  {0}", Path.Combine(".nuget", "nuget.exe"));
            var nugetExePath = Path.Combine(Utils.GaugeProjectRoot, ".nuget", "nuget.exe");
            File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "nuget.exe"),
                nugetExePath, true);

            var nugetProcess = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = Utils.GaugeProjectRoot,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = nugetExePath,
                    RedirectStandardError = true,
                    Arguments = "install packages.config -o packages"
                }
            };
            nugetProcess.OutputDataReceived += (sender, args) => Console.Out.WriteLine(args.Data);
            nugetProcess.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;
                Console.Out.WriteLine(" {0}", args.Data);
                Environment.Exit(1);
            };
            nugetProcess.Start();
            nugetProcess.BeginOutputReadLine();
            nugetProcess.BeginErrorReadLine();
            nugetProcess.WaitForExit();
        }
    }
}