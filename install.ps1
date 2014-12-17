param([string]$artifactPath='')

if ($artifactPath -ne '') {
    $artifactPath=resolve-path $artifactPath
}
else {
    $artifactPath=$pwd
}
$pluginFile = gci artifacts\gauge-csharp*.zip | select -f 1

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
    Remove-Item -recurse $destinationPath -Force
    Rename-Item $tempFolderPath $pluginVersion -Force
}