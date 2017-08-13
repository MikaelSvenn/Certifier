@echo off
msbuild Certifier.sln /t:Clean /t:Build /p:Configuration=Release;TargetFrameworkVersion=v4.6
if %errorlevel% NEQ 0 (goto fail)

packages\NUnit.ConsoleRunner.3.7.0\tools\nunit3-console.exe Core.Test\bin\Release\Core.Test.dll Crypto.Test\bin\Release\Crypto.Test.dll Ui.Console.Test\bin\Release\Ui.Console.Test.dll Integration.ConvertKey.Test\bin\Release\Integration.ConvertKey.Test.dll Integration.CreateKey.Test\bin\Release\Integration.CreateKey.Test.dll Integration.CreateSignature.Test\bin\Release\Integration.CreateSignature.Test.dll Integration.VerifyKey.Test\bin\Release\Integration.VerifyKey.Test.dll Integration.VerifySignature.Test\bin\Release\Integration.VerifySignature.Test.dll
if %errorlevel% NEQ 0 (goto fail)

:success
echo Build Succeeded!
exit /b

:fail
echo Build Failed.
exit /b %errorlevel%