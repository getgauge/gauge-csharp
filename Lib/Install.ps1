# Copyright 2015 ThoughtWorks, Inc.
#
# This file is part of Gauge-CSharp.
#
# Gauge-CSharp is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# Gauge-Ruby is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

param($installPath, $toolsPath, $package, $project)

Write-Host "Initializing a Gauge Project for" $project.Name

$initScript = {param($path)
Set-Location $path

gauge --init csharp

$projectItems = Get-Interface $project.ProjectItems ([EnvDTE.ProjectItems])

@("env", "specs") | resolve-path | %{$projectItems.AddFromDirectory($_.Path)}

@("StepImplementation.cs", "manifest.json") | resolve-path | %{$projectItems.AddFromFile($_.Path)}
}

$projectRootDir = Split-Path -parent $project.FullName

Invoke-Command -ScriptBlock $initScript -ArgumentList $projectRootDir