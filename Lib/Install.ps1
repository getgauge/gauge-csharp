param($installPath, $toolsPath, $package, $project)

Write-Host "Initializing a Gauge Project for" $project.Name

$initScript = {param($path)
Set-Location $path

gauge --init java

$projectItems = Get-Interface $project.ProjectItems ([EnvDTE.ProjectItems])

@("env", "specs") | resolve-path | %{$projectItems.AddFromDirectory($_.Path)}

@("StepImplementation.cs", "manifest.json") | resolve-path | %{$projectItems.AddFromFile($_.Path)}
}

$projectRootDir = Split-Path -parent $project.FullName

Invoke-Command -ScriptBlock $initScript -ArgumentList $projectRootDir