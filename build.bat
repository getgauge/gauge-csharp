@echo off
echo "building Gauge CSharp"
call %SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe Gauge.CSharp.sln /nologo /p:configuration=release;OutputPath=%~dp0\artifacts
exit /b %errorlevel%