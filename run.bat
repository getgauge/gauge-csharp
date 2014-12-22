@echo off
IF "%1"=="" goto NO_ARG

:exec
powershell.exe -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile -WindowStyle Hidden -Command "& '.\build\%1.ps1' '%2'"
goto :eof

:NO_ARG
echo 1 argument required. 
echo usage: %0 [build/package/test]
exit /B 1