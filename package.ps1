Remove-Item "$($pwd)\artifacts" -recurse

& "$(Split-Path $MyInvocation.MyCommand.Path)\build.ps1"

$outputDir= "$($pwd)\artifacts\gauge-csharp"

$outputPath= "$($pwd)\artifacts\gauge-csharp\bin"
$skelDir="$($outputDir)\skel"
$nugetDir = "$($pwd)\artifacts\gauge-csharp-nuget"

@($skelDir, $nugetDir) | %{ New-Item -Itemtype directory $_ -Force}

Write-host "Copying Skeleton files for Gauge CSharp project"

# Copy the skeleton files
Copy-Item "$($pwd)\Gauge.Project.Skel\AssemblyInfo.cs" -Destination "$($skelDir)\Properties" -Force
Copy-Item "$($pwd)\Gauge.Project.Skel\Gauge.Spec.csproj" -Destination $skelDir -Force
Copy-Item "$($pwd)\Gauge.Project.Skel\StepImplementation.cs" -Destination $skelDir -Force
Copy-Item "$($pwd)\Gauge.Project.Skel\packages.config" -Destination $skelDir -Force
Copy-Item "$($pwd)\Gauge.Project.Skel\Gauge.Spec.sln" -Destination $skelDir -Force

# Copy the .nuget folder - this is required since the solution is configured to restore packages.
Copy-Item "$($pwd)\.nuget" -Destination $skelDir -recurse

Copy-Item "$($pwd)\Runner\csharp.json" -Destination $outputDir -Force

$nugetInstallScript= {param($outputPath, $nugetDir)
    $nuget = "$($pwd)\.nuget\NuGet.exe"
    $env:OutDir=$outputPath # required for nuget to pick up the file from this location
    &$nuget pack Lib\Gauge.CSharp.Lib.csproj /p Configuration=release -OutputDirectory "$($nugetDir)" -Verbosity detailed -ExcludeEmptyDirectories
}

Invoke-Command -ScriptBlock $nugetInstallScript -ArgumentList $outputPath, $nugetDir

Import-Module Pscx

# zip!
$zipScript= {
    set-location $outputDir
    $version=(Get-Item "$($outputPath)\Gauge.CSharp.Runner.exe").VersionInfo.ProductVersion
    gci -recurse | Write-Zip -OutputPath "$(Split-Path $outputDir)\gauge-csharp-$($version).zip"
}

Invoke-Command -ScriptBlock $zipScript