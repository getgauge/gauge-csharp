param([string]$buildWithTest='')
$outputPath= [IO.Path]::Combine($pwd,"artifacts\gauge-csharp\bin\")
New-Item -Itemtype directory $outputPath -Force
$msbuild="$($env:systemroot)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
$sln = "Gauge.CSharp.NoTests.sln"
if ($buildWithTest -eq 'true') {
    $sln="Gauge.CSharp.sln"
}

&$msbuild $sln /t:rebuild /m /nologo /p:configuration=release /p:OutDir="$($outputPath)"