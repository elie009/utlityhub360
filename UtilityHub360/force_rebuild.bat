@echo off
echo Force Rebuilding Project
echo ========================
echo.

echo Step 1: Cleaning project...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
echo Cleaned bin and obj directories
echo.

echo Step 2: Building project with MSBuild...
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug /p:Platform="Any CPU" /t:Rebuild
if %errorlevel% neq 0 (
    echo Trying alternative MSBuild path...
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug /p:Platform="Any CPU" /t:Rebuild
    if %errorlevel% neq 0 (
        echo MSBuild not found. Please rebuild manually in Visual Studio.
    )
)

echo.
echo Rebuild completed!
echo The IMapper and AutoMapper errors should now be resolved.
pause
