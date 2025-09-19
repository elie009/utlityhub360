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

echo Installing System.Text.Json 4.7.2 (required by MediatR 9.0.0)...
nuget install System.Text.Json -Version 4.7.2 -OutputDirectory packages
echo.

echo Package installation completed!
echo.
echo Now building project...
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug
if %errorlevel% neq 0 (
    echo Trying alternative MSBuild path...
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug
    if %errorlevel% neq 0 (
        echo MSBuild not found. Please build the project manually in Visual Studio.
        echo The packages have been installed successfully.
    )
)

if %errorlevel% equ 0 (
    echo.
    echo ================================================
    echo SUCCESS: Compatible packages installed!
    echo ================================================
    echo.
    echo The netstandard error should now be resolved.
) else (
    echo.
    echo ================================================
    echo ERROR: Build failed. Please check the errors above.
    echo ================================================
)

echo.
pause
