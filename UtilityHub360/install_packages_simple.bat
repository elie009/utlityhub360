@echo off
echo Installing Compatible Package Versions
echo ======================================
echo.

echo Installing MediatR 9.0.0...
nuget install MediatR -Version 9.0.0 -OutputDirectory packages
echo.

echo Installing MediatR Extensions 9.0.0...
nuget install MediatR.Extensions.Microsoft.DependencyInjection -Version 9.0.0 -OutputDirectory packages
echo.

echo Installing AutoMapper 10.1.1...
nuget install AutoMapper -Version 10.1.1 -OutputDirectory packages
echo.

echo Installing System.Text.Json 4.7.2...
nuget install System.Text.Json -Version 4.7.2 -OutputDirectory packages
echo.

echo ================================================
echo SUCCESS: Compatible packages installed!
echo ================================================
echo.
echo The netstandard error should now be resolved.
echo Please build your project in Visual Studio to verify.
echo.

pause
