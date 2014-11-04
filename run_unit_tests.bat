call build.bat
if %errorlevel% neq 0 exit /b %errorlevel%

call .\packages\NUnit.Runners.2.6.3\tools\nunit-console artifacts\Gauge.CSharp.Runner.UnitTests.dll /xml:artifacts\gauge.csharp.runner.unittests.xml

exit /b %ERRORLEVEL%