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
        private const string ProjectName = "Gauge.Spec";
        static readonly string ProjectRootDir = Path.Combine(Utils.GaugeProjectRoot, ProjectName);
        public void Execute()
        {
            Directory.CreateDirectory(ProjectRootDir);
            if (Directory.EnumerateFiles(ProjectRootDir, "*.csproj").Any())
            {
                Console.Out.WriteLine("Directory is not empty, skipping Gauge initialization.");
                Environment.Exit(1);
            }
            Directory.CreateDirectory(Path.Combine(ProjectRootDir, "Properties"));
            
            new List<string>
            {
                Path.Combine("Properties", "AssemblyInfo.cs"),
                "StepImplementation.cs",
                "packages.config"
            }.ForEach(CopyFile); 
            
            CopyFile("Gauge.Spec.csproj", string.Format("{0}.csproj", ProjectName));
            CopyFile("Gauge.Spec.sln", string.Format("{0}.sln", ProjectName), Utils.GaugeProjectRoot);
            InstallDependencies();
        }

        private static void CopyFile(string filePath)
        {
            CopyFile(filePath, string.Empty);
        }

        private static void CopyFile(string filePath, string destPath, string rootDir = null)
        {
            var skeletonPath = Path.GetFullPath("skel");
            var destFileName = Path.Combine(string.IsNullOrEmpty(rootDir) ? ProjectRootDir : rootDir,
                string.IsNullOrEmpty(destPath) ? filePath : destPath);
            File.Copy(Path.Combine(skeletonPath, filePath), destFileName, true);
            var fileContent = File.ReadAllText(destFileName)
                .Replace(@"$safeprojectname$", "Gauge.Spec")
                .Replace("$guid1$", Guid.NewGuid().ToString())
                .Replace("$guid2$", Guid.NewGuid().ToString());
            File.WriteAllText(destFileName, fileContent);
            Console.Out.WriteLine(" create  {0}", filePath);
        }

        private static void InstallDependencies()
        {
            var nugetExePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "nuget.exe");

            var nugetProcess = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = Utils.GaugeProjectRoot,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = nugetExePath,
                    RedirectStandardError = true,
                    Arguments = string.Format(@"install {0} -o packages", Path.Combine(ProjectName, "packages.config"))
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