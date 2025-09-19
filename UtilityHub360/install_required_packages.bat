@echo off
echo Installing Required Packages
echo ============================
echo.

echo Installing MediatR 8.1.0...
nuget install MediatR -Version 8.1.0 -OutputDirectory packages

echo.
echo Installing AutoMapper 9.0.0...
nuget install AutoMapper -Version 9.0.0 -OutputDirectory packages

echo.
echo Installing MediatR Extensions 8.1.0...
nuget install MediatR.Extensions.Microsoft.DependencyInjection -Version 8.1.0 -OutputDirectory packages

echo.
echo Installing Microsoft.Extensions.DependencyInjection 2.2.0...
nuget install Microsoft.Extensions.DependencyInjection -Version 2.2.0 -OutputDirectory packages

echo.
echo Package installation completed!
echo Please restart Visual Studio and rebuild your project.
pause
