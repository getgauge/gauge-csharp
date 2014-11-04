call build.bat
if %errorlevel% neq 0 exit /b %errorlevel%

set SKELDIR=%~dp0artifacts\skel

mkdir %SKELDIR%
mkdir %SKELDIR%\.nuget

echo "Copying Skeleton files for Gauge CSharp project"

xcopy .\Gauge.Project.Skel\AssemblyInfo.cs %SKELDIR%\Properties\ /y
xcopy .\Gauge.Project.Skel\Gauge.Spec.csproj %SKELDIR%\ /y
xcopy .\Gauge.Project.Skel\StepImplementation.cs %SKELDIR%\ /y
xcopy .\Gauge.Project.Skel\packages.config %SKELDIR%\ /y
xcopy .\Gauge.Project.Skel\Gauge.Spec.sln %SKELDIR%\ /y

xcopy .nuget %SKELDIR%\.nuget /y

if %errorlevel% neq 0 exit /b %errorlevel%

call .nuget\NuGet.exe pack Lib\Gauge.CSharp.Lib.csproj /p Configuration=release -OutputDirectory artifacts -Verbosity detailed -ExcludeEmptyDirectories

exit /b %ERRORLEVEL%