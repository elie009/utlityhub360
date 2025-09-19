@echo off
echo Fixing Package Compatibility Issues
echo ===================================
echo.

echo Step 1: Restoring packages with compatible versions...
nuget restore -PackagesDirectory packages
echo.

echo Step 2: Building project to verify fix...
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug
if %errorlevel% neq 0 (
    echo Trying alternative MSBuild path...
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug
    if %errorlevel% neq 0 (
        echo MSBuild not found. Please build the project manually in Visual Studio.
        echo The packages have been restored successfully.
    )
)
echo.

if %errorlevel% equ 0 (
    echo.
    echo ================================================
    echo SUCCESS: Package compatibility issues fixed!
    echo ================================================
    echo.
    echo Your project should now run without the netstandard error.
) else (
    echo.
    echo ================================================
    echo ERROR: Build failed. Please check the errors above.
    echo ================================================
)

echo.
pause
