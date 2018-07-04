@echo off
cls

SET TOOL_PATH=.fake

.paket\paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

IF NOT EXIST "%TOOL_PATH%\fake.exe" (
  dotnet tool install fake-cli --tool-path ./%TOOL_PATH% --version 5.*
)

"%TOOL_PATH%/fake.exe" -s run build.fsx %*

exit /b %errorlevel%
