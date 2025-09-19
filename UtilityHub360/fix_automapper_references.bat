@echo off
echo Fixing AutoMapper References
echo ============================
echo.

echo Step 1: Verifying AutoMapper package exists...
if exist "packages\AutoMapper.9.0.0\lib\net461\AutoMapper.dll" (
    echo ✓ AutoMapper package found
) else (
    echo ✗ AutoMapper package not found
    echo Installing AutoMapper...
    nuget install AutoMapper -Version 9.0.0 -OutputDirectory packages
)

echo.
echo Step 2: Checking project file references...
findstr /C:"AutoMapper" UtilityHub360.csproj
echo.

echo Step 3: Cleaning project...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
echo Cleaned build directories

echo.
echo Step 4: Rebuilding project...
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug /t:Rebuild
if %errorlevel% neq 0 (
    echo Trying alternative MSBuild...
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug /t:Rebuild
)

echo.
echo ================================================
echo AutoMapper fix completed!
echo ================================================
echo.
echo If errors persist, please:
echo 1. Close Visual Studio completely
echo 2. Reopen Visual Studio
echo 3. Clean and rebuild the solution
echo.

pause
