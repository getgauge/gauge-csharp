& "$(Split-Path $MyInvocation.MyCommand.Path)\build.ps1" -buildWithTest $true

$nunit = "$($pwd)\packages\NUnit.Runners.2.6.4\tools\nunit-console.exe"

if(!(Test-Path $nunit))
{
    throw "Nunit runner not found in $pwd"
}
&$nunit "$($pwd)\artifacts\gauge-csharp\bin\Gauge.CSharp.Runner.UnitTests.dll" /xml:"$($pwd)\artifacts\gauge-csharp\bin\gauge.csharp.runner.unittests.xml"

# Hack to break on exit code. Powershell does not seem to propogate the exit code from test failures.
if($LastExitCode -ne 0)
{
    throw "Test execution failed."
}
