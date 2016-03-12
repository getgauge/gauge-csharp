# Copyright 2015 ThoughtWorks, Inc.

# This file is part of Gauge-CSharp.

# Gauge-CSharp is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.

# Gauge-CSharp is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.

# You should have received a copy of the GNU General Public License
# along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

param([string]$buildWithTest='')

function RestoreNugetAndBuild
{
  param($sln, $outputDir)
  $verbosity = "minimal"
  if($env:MSBUILD_VERBOSITY)
  {
    $verbosity = $env:MSBUILD_VERBOSITY
  }

  New-Item -Itemtype directory $outputPath -Force
  $msbuild="$($env:systemroot)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
  Write-Host "Restoring Nuget for $($sln)"
  $nuget = "$($pwd)\build\NuGet.exe"
  &$nuget restore $sln
  &$msbuild $sln /t:rebuild /m /nologo /p:configuration=release /p:OutDir="$($outputDir)" /v:"$($verbosity)"
  if($LastExitCode -ne 0)
  {
      throw "Build failed $($sln)"
  }
}

# Build the package CSharp-lib
$outputPath= [IO.Path]::Combine($pwd,"artifacts\gauge-csharp-lib\")
$sln = "Gauge.CSharp.Lib.sln"
RestoreNugetAndBuild $sln $outputPath

# Build the package CSharp-Core
$outputPath= [IO.Path]::Combine($pwd,"artifacts\gauge-csharp-core\")
$sln = "Gauge.CSharp.Core.sln"
RestoreNugetAndBuild $sln $outputPath

# Build the runner
$outputPath= [IO.Path]::Combine($pwd,"artifacts\gauge-csharp\bin\")
$sln = "Gauge.CSharp.NoTests.sln"
if ($buildWithTest) {
    $sln="Gauge.CSharp.sln"
    $sampleProj = resolve-path "IntegrationTestSample\IntegrationTestSample.sln"
    $sampleProjOutputPath = [IO.Path]::Combine($pwd,"IntegrationTestSample\gauge-bin")
    RestoreNugetAndBuild $sampleProj $sampleProjOutputPath
}
RestoreNugetAndBuild $sln $outputPath
