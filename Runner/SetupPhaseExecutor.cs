using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Gauge.CSharp.Runner.Communication;

namespace Gauge.CSharp.Runner
{
    public class SetupPhaseExecutor : IPhaseExecutor
    {
        private static readonly string ProjectName = new DirectoryInfo(Utils.GaugeProjectRoot).Name;
        private static readonly string ProjectRootDir = Utils.GaugeProjectRoot;
        public void Execute()
        {
            CheckAndCreateDirectory(Path.Combine(ProjectRootDir, "Properties"));
            
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

        private static void CheckAndCreateDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Console.Out.WriteLine(" skip  {0}", ProjectName);
            }
            else
            {
                Console.Out.WriteLine(" create  {0}", ProjectName);
                Directory.CreateDirectory(directory);
            }
        }

        private static void CopyFile(string filePath)
        {
            CopyFile(filePath, string.Empty);
        }

        private static void CopyFile(string filePath, string destPath, string rootDir = null)
        {
            var skeletonPath = Path.GetFullPath("skel");
            var destFileName = string.IsNullOrEmpty(destPath) ? filePath : destPath;
            var destFileNameFull = Path.Combine(string.IsNullOrEmpty(rootDir) ? ProjectRootDir : rootDir, destFileName);
            if (File.Exists(destFileNameFull))
            {
                Console.Out.WriteLine(" skip  {0}", destFileName);
            }
            else
            {
                File.Copy(Path.Combine(skeletonPath, filePath), destFileNameFull);
                var fileContent = File.ReadAllText(destFileNameFull)
                    .Replace(@"$safeprojectname$", ProjectName)
                    .Replace("$guid1$", Guid.NewGuid().ToString())
                    .Replace("$guid2$", Guid.NewGuid().ToString());
                File.WriteAllText(destFileNameFull, fileContent);
                Console.Out.WriteLine(" create  {0}", destFileName);
            }
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
                    Arguments = string.Format(@"install {0} -o packages", "packages.config")
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