@echo off
echo Fixing .NET Standard Error
echo ==========================
echo.

echo This error occurs because some packages require .NET Standard 2.1
echo but your project targets .NET Framework 4.8 which only supports up to .NET Standard 2.0
echo.

echo Solution 1: Installing .NET Standard 2.1 Runtime...
echo Downloading .NET Standard 2.1 runtime...
powershell -Command "Invoke-WebRequest -Uri 'https://download.microsoft.com/download/8/8/5/88544F33-836A-49A5-8B67-451C74209A22/dotnet-runtime-2.1.30-win-x64.exe' -OutFile 'dotnet-runtime-2.1.30-win-x64.exe'"

if exist dotnet-runtime-2.1.30-win-x64.exe (
    echo Installing .NET Standard 2.1 runtime...
    dotnet-runtime-2.1.30-win-x64.exe /quiet
    echo Runtime installed successfully!
) else (
    echo Failed to download runtime. Trying alternative solution...
)

echo.
echo Solution 2: Installing compatible package versions...
echo.

echo Installing MediatR 9.0.0 (compatible with .NET Framework 4.8)...
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

echo Installing Microsoft.Extensions.DependencyInjection 3.1.32...
nuget install Microsoft.Extensions.DependencyInjection -Version 3.1.32 -OutputDirectory packages
echo.

echo ================================================
echo Package installation completed!
echo ================================================
echo.
echo Please try running your application again.
echo If the error persists, you may need to:
echo 1. Restart Visual Studio
echo 2. Clean and rebuild the solution
echo 3. Check that the packages.config was updated correctly
echo.

pause
