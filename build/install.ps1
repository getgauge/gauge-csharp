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

param([string]$artifactPath='')

if ($artifactPath -ne '') {
    $artifactPath=resolve-path $artifactPath
}
else {
    $artifactPath=join-path $pwd, "artifacts"
}

$pluginFile = gci $artifactPath\gauge-csharp*.zip | select -f 1

$pluginVersion=$pluginFile.Name | Select-String '(gauge-csharp-)(.*).zip' | %{$_.Matches[0].Groups[2].Value}

Import-Module Pscx
$destinationPath="$($env:APPDATA)\gauge\plugins\csharp\$($pluginVersion)"
$tempFolder="$($pluginVersion).tmp"
$tempFolderPath="$($env:APPDATA)\gauge\plugins\csharp\$($pluginVersion).tmp"
Try
{
    Write-host "Installing gauge-csharp version: $($pluginVersion) from $($pluginFile)"
    if(Test-Path $destinationPath)
    {
        Rename-Item $destinationPath $tempFolder -Force
    }
    New-Item -Itemtype directory $destinationPath -Force
    Expand-Archive $pluginFile $destinationPath
    if(Test-Path $tempFolderPath)
    {
        Remove-Item $tempFolderPath -Force -recurse
    }
}
Catch
{
    Write-host "Failed to install $($pluginVersion), Rolling back"
    Write-host $_.Exception.Message
    Remove-Item -recurse $destinationPath -Force
    if(Test-Path $tempFolderPath)
    {
        Rename-Item $tempFolderPath $pluginVersion -Force
    }
}
