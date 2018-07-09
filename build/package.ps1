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

$nugetInstallScript= {param($outputPath, $nugetDir, $projectPath)
    $nuget = "$($pwd)\build\NuGet.exe"
    $env:OutDir=$outputPath # required for nuget to pick up the file from this location
    &$nuget pack "$($projectPath)" /p Configuration=release -OutputDirectory "$($nugetDir)" -ExcludeEmptyDirectories
}

$nugetDir = "$($pwd)\artifacts"
New-Item -Itemtype directory $nugetDir -Force | Out-Null

$outputDir= "$($pwd)\artifacts\gauge-csharp"

$outputPath= "$($pwd)\artifacts\gauge-csharp\bin"
$skelDir="$($outputDir)\skel"
$skelPropertiesDir = "$($skelDir)\Properties"
$skelEnvDir = "$($skelDir)\Env"
$skelDefaultPropertiesDir = "$($skelEnvDir)\Default"
@($skelDir, $skelPropertiesDir, $skelEnvDir, $skelDefaultPropertiesDir) | %{ New-Item -Itemtype directory $_ -Force | Out-Null}

Write-host "Copying Skeleton files for Gauge CSharp project"

# Copy the skeleton files
Copy-Item "$($pwd)\Gauge.Project.Skel\AssemblyInfo.cs" -Destination $skelPropertiesDir -Force | Out-Null
Copy-Item "$($pwd)\Gauge.Project.Skel\Gauge.Spec.csproj" -Destination $skelDir -Force | Out-Null
Copy-Item "$($pwd)\Gauge.Project.Skel\StepImplementation.cs" -Destination $skelDir -Force | Out-Null
Copy-Item "$($pwd)\Gauge.Project.Skel\packages.config" -Destination $skelDir -Force | Out-Null
Copy-Item "$($pwd)\Gauge.Project.Skel\Gauge.Spec.sln" -Destination $skelDir -Force | Out-Null
Copy-Item "$($pwd)\Gauge.Project.Skel\csharp.properties" -Destination "$($skelDir)\Env\Default" -Force | Out-Null
Copy-Item "$($pwd)\Runner\csharp.json" -Destination $outputDir -Force | Out-Null
Copy-Item "$($pwd)\Runner\notice.md" -Destination $outputDir -Force | Out-Null

# zip!
$version=(Get-Item "$($outputPath)\Gauge.CSharp.Runner.exe").VersionInfo.ProductVersion

if ($version) {
    Add-Type -Assembly "System.IO.Compression.FileSystem" ;
    $archivePath = "$(Split-Path $outputDir)\gauge-csharp-$($version).zip"
    [System.IO.Compression.ZipFile]::CreateFromDirectory($outputDir, $archivePath)
    Write-Host "Created " $archivePath    
}
else {
    throw "Cannot determine version number from : $($outputPath)\Gauge.CSharp.Runner.exe"
}

# Hack to break on exit code. Powershell does not seem to propogate the exit code from test failures.
if($LastExitCode -ne 0)
{
    throw "Test execution failed."
}
