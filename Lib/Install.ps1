param($installPath, $toolsPath, $package, $project)
Write-Host "Initializing a Gauge Project for $project.Name" 
gauge --init csharp