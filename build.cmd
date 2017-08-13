@echo off
msbuild Certifier.sln /t:Clean /t:Build /p:Configuration=Release;TargetFrameworkVersion=v4.6
if %errorlevel% NEQ 0 (goto fail)

packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe Core.Test\bin\Release\Core.Test.dll Crypto.Test\bin\Release\Crypto.Test.dll Ui.Console.Test\bin\Release\Ui.Console.Test.dll
if %errorlevel% NEQ 0 (goto fail)

:success
echo Build Succeeded!
exit /b

:fail
echo Build Failed.
exit /b %errorlevel%