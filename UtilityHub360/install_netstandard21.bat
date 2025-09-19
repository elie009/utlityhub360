@echo off
echo Installing .NET Standard 2.1 Runtime
echo ====================================
echo.

echo This will install the .NET Standard 2.1 runtime which is required
echo by some of the newer NuGet packages in your project.
echo.

echo Downloading .NET Standard 2.1 runtime...
powershell -Command "Invoke-WebRequest -Uri 'https://download.microsoft.com/download/8/8/5/88544F33-836A-49A5-8B67-451C74209A22/dotnet-runtime-2.1.30-win-x64.exe' -OutFile 'dotnet-runtime-2.1.30-win-x64.exe'"

if exist dotnet-runtime-2.1.30-win-x64.exe (
    echo.
    echo Installing .NET Standard 2.1 runtime...
    echo Please follow the installation wizard...
    dotnet-runtime-2.1.30-win-x64.exe
    echo.
    echo Runtime installation completed!
    echo Please restart your application after installation.
) else (
    echo.
    echo Failed to download the runtime.
    echo Please download it manually from:
    echo https://dotnet.microsoft.com/download/dotnet/2.1
)

echo.
pause
