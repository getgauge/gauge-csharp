﻿// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gauge.CSharp.Core;
using NLog;
using NuGet;

namespace Gauge.CSharp.Runner
{
    public class SetupPhaseExecutor : IPhaseExecutor
    {
        private static readonly string ProjectName = new DirectoryInfo(Utils.GaugeProjectRoot).Name;
        private static readonly string ProjectRootDir = Utils.GaugeProjectRoot;
        const string packageID = "Gauge.CSharp.Lib";
        private static readonly SemanticVersion MaxLibVersion = GetMaxNugetVersion();
        private static readonly Logger Logger = LogManager.GetLogger("install");

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
                Logger.Info("skip  {0}", ProjectName);
            }
            else
            {
                Logger.Info("create  {0}", ProjectName);
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
                Logger.Info("skip  {0}", destFileName);
            }
            else
            {
                var version = MaxLibVersion.ToString();
                var normalizedVersion = MaxLibVersion.ToNormalizedString();

                File.Copy(Path.Combine(skeletonPath, filePath), destFileNameFull);
                var fileContent = File.ReadAllText(destFileNameFull)
                    .Replace(@"$safeprojectname$", ProjectName)
                    .Replace("$guid1$", Guid.NewGuid().ToString())
                    .Replace("$guid2$", Guid.NewGuid().ToString())
                    .Replace("$nugetLibVersion$", version)
                    .Replace("$nugetLibNormalizedVersion$", normalizedVersion);

                File.WriteAllText(destFileNameFull, fileContent);
                Logger.Info("create  {0}", destFileName);
            }
        }

        private static void InstallDependencies()
        {
            Logger.Info("Installing Nuget Package : {0}, version: {1}", packageID, MaxLibVersion);
            var packagePath = Path.Combine(Utils.GaugeProjectRoot, "packages");
            var repo = CreatePackageRepository();
            var packageManager = new PackageManager(repo, packagePath);
            packageManager.InstallPackage(packageID, MaxLibVersion);
            Logger.Info("Done Installing Nuget Package!");
        }

        private static IPackageRepository CreatePackageRepository()
        {
            return PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
        }

        private static SemanticVersion GetMaxNugetVersion()
        {
            return CreatePackageRepository()
                .FindPackagesById(packageID)
                .Max(p => p.Version);
        }
    }
}