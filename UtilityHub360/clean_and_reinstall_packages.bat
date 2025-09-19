@echo off
echo Cleaning and Reinstalling Packages
echo ===================================
echo.

echo Step 1: Cleaning existing packages...
if exist packages rmdir /s /q packages
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
echo Cleaned packages, bin, and obj directories
echo.

echo Step 2: Installing .NET Framework 4.8 compatible packages...
echo.

echo Installing MediatR 8.1.0...
nuget install MediatR -Version 8.1.0 -OutputDirectory packages
echo.

echo Installing MediatR Extensions 8.1.0...
nuget install MediatR.Extensions.Microsoft.DependencyInjection -Version 8.1.0 -OutputDirectory packages
echo.

echo Installing AutoMapper 9.0.0...
nuget install AutoMapper -Version 9.0.0 -OutputDirectory packages
echo.

echo Installing Microsoft.Extensions.DependencyInjection 2.2.0...
nuget install Microsoft.Extensions.DependencyInjection -Version 2.2.0 -OutputDirectory packages
echo.

echo Installing Microsoft.Extensions.Logging 2.2.0...
nuget install Microsoft.Extensions.Logging -Version 2.2.0 -OutputDirectory packages
echo.

echo Installing Microsoft.Extensions.Logging.Abstractions 2.2.0...
nuget install Microsoft.Extensions.Logging.Abstractions -Version 2.2.0 -OutputDirectory packages
echo.

echo ================================================
echo Package installation completed!
echo ================================================
echo.
echo These versions are fully compatible with .NET Framework 4.8
echo and should not require .NET Standard 2.1.
echo.
echo Please restart Visual Studio and rebuild your project.
echo.

pause
