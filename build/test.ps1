& "$(Split-Path $MyInvocation.MyCommand.Path)\build.ps1" -buildWithTest $true

$nunit = "$($pwd)\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe"

if(!(Test-Path $nunit))
{
    Write-Host "Nunit runner not found in $pwd"
    exit 1
}
&$nunit "$($pwd)\artifacts\gauge-csharp\bin\Gauge.CSharp.Runner.UnitTests.dll" /xml:"$($pwd)\artifacts\gauge-csharp\bin\gauge.csharp.runner.unittests.xml"
