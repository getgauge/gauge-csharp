@echo off
echo "building Gauge CSharp project"
call C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe Gauge.CSharp.sln /nologo /p:configuration=release;OutputPath=%~dp0\artifacts

set SKELDIR=%~dp0artifacts\skel

mkdir %SKELDIR%

xcopy ..\gauge-visualstudio\Gauge.VisualStudio.Template\AssemblyInfo.cs %SKELDIR%\Properties\ /y
xcopy ..\gauge-visualstudio\Gauge.VisualStudio.Template\Gauge.Spec.csproj %SKELDIR%\ /y
xcopy ..\gauge-visualstudio\Gauge.VisualStudio.Template\StepImplementation.cs %SKELDIR%\ /y
xcopy ..\gauge-visualstudio\Gauge.VisualStudio.Template\packages.config %SKELDIR%\ /y

exit /b %ERRORLEVEL%