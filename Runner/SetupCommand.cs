// Copyright 2015 ThoughtWorks, Inc.
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
using Gauge.CSharp.Runner.Extensions;
using NLog;
using NuGet;

namespace Gauge.CSharp.Runner
{
    public class SetupCommand : IGaugeCommand
    {
        private readonly IPackageRepositoryFactory _packageRepositoryFactory;
        private static readonly string ProjectName = new DirectoryInfo(Utils.GaugeProjectRoot).Name;
        private static readonly string ProjectRootDir = Utils.GaugeProjectRoot;
        public const string PackageId = "Gauge.CSharp.Lib";

        public static readonly string NugetEndpoint = string.IsNullOrEmpty(Utils.TryReadEnvValue("NUGET_ENDPOINT"))
            ? @"https://packages.nuget.org/api/v2"
            : Utils.TryReadEnvValue("NUGET_ENDPOINT");

        private static SemanticVersion _maxLibVersion;
        private static readonly Logger Logger = LogManager.GetLogger(string.Empty);
        private IPackageRepository _packageRepository;

        public SetupCommand() : this(PackageRepositoryFactory.Default)
        {
        }

        public SetupCommand(IPackageRepositoryFactory packageRepositoryFactory)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            SafeProjectName = ProjectName.ToValidCSharpIdentifier();
        }

        public string SafeProjectName { get; private set; }

        public SemanticVersion MaxLibVersion
        {
            get { return _maxLibVersion = _maxLibVersion ?? GetMaxNugetVersion(); }
        }

        public void Execute()
        {
            CheckAndCreateDirectory(Path.Combine(ProjectRootDir, "Properties"));
            
            new List<string>
            {
                Path.Combine("Properties", "AssemblyInfo.cs"),
                Path.Combine("env", "default", "csharp.properties"),
                "StepImplementation.cs",
                "packages.config"
            }.ForEach(CopyFile); 
            
            CopyFile("Gauge.Spec.csproj", string.Format("{0}.csproj", SafeProjectName));
            CopyFile("Gauge.Spec.sln", string.Format("{0}.sln", SafeProjectName));
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

        private void CopyFile(string filePath)
        {
            CopyFile(filePath, string.Empty);
        }

        private void CopyFile(string filePath, string destPath)
        {
            var skeletonPath = Path.GetFullPath("skel");
            var destFileName = string.IsNullOrEmpty(destPath) ? filePath : destPath;
            var destFileNameFull = Path.GetFullPath(Path.Combine(ProjectRootDir, destFileName));

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
                    .Replace("$safeprojectname$", SafeProjectName)
                    .Replace("$guid1$", Guid.NewGuid().ToString())
                    .Replace("$guid2$", Guid.NewGuid().ToString())
                    .Replace("$nugetLibVersion$", version)
                    .Replace("$gaugeprojectfile$", ProjectFilePath)
                    .Replace("$nugetLibNormalizedVersion$", normalizedVersion);

                File.WriteAllText(destFileNameFull, fileContent);
                Logger.Info("create  {0}", destFileName);
            }
        }

        private string ProjectFilePath
        {
            get
            {
                return Path.GetFullPath(Path.Combine(ProjectRootDir, string.Concat(SafeProjectName, ".csproj")))
                        .Replace(@"\", "/");
            }
        }

        private void InstallDependencies()
        {
            var packagePath = Path.Combine(Utils.GaugeProjectRoot, "packages");
            var packageConfigPath = Path.Combine(Utils.GaugeProjectRoot, "packages.config");
            var referenceFile = new PackageReferenceFile(packageConfigPath);
            var packageReferences = referenceFile.GetPackageReferences(true);
            var packageManager = new PackageManager(PackageRepository, packagePath);

            foreach (var packageReference in packageReferences)
            {
                Logger.Info("Installing Nuget Package : {0}, version: {1}", packageReference.Id, packageReference.Version);
                var package = PackageRepository.FindPackage(packageReference.Id, packageReference.Version);
                packageManager.InstallPackage(package, false, false);
            }

            Logger.Info("Done Installing Nuget Package!");
        }

        private IPackageRepository PackageRepository
        {
            get
            {
                return _packageRepository = _packageRepository ?? _packageRepositoryFactory.CreateRepository(NugetEndpoint);
            }
        }

        private SemanticVersion GetMaxNugetVersion()
        {
            return PackageRepository
                .FindPackagesById(PackageId)
                .Max(p => p.Version);
        }
    }
}