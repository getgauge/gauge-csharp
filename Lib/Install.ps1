param($installPath, $toolsPath, $package, $project)

Write-Host "Initializing a Gauge Project for" $project.Name

gauge --init csharp

$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

$specsFolder = $solution.AddSolutionFolder("specs")
$specsSolutionItems = Get-Interface $specsFolder.ProjectItems ([EnvDTE.ProjectItems])
resolve-path "specs\hello_world.spec" | %{ $specsSolutionItems.AddFromFile($_.Path) }

$stepImpl = $project.Name + "\StepImplementation.cs"
resolve-path $stepImpl | %{ $project.ProjectItems.AddFromFile($_.Path) }