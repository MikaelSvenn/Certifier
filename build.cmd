@echo off
msbuild Certifier.sln /t:Clean /p:Configuration=Release;TargetFrameworkVersion=v4.6
if errorlevel 1 (goto fail)

packages\NUnit.ConsoleRunner.3.6.0\tools\nunit3-console.exe Core.Test\bin\Debug\Core.Test.dll
if errorlevel 1 (goto fail)

:success
echo Build OK!
exit /b

:fail
echo Build Failed.