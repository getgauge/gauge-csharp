& "$(Split-Path $MyInvocation.MyCommand.Path)\build.ps1" -buildWithTest $true

$nunit = "$($pwd)\packages\NUnit.Runners.2.6.3\tools\nunit-console.exe"

&$nunit "$($pwd)\artifacts\gauge-csharp\bin\Gauge.CSharp.Runner.UnitTests.dll" /xml:"$($pwd)\artifacts\gauge-csharp\bin\gauge.csharp.runner.unittests.xml"