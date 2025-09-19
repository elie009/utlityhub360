@echo off
echo Rebuilding Project
echo ==================
echo.

echo The project file has been updated with correct package paths.
echo Please:
echo 1. Close Visual Studio if it's open
echo 2. Reopen Visual Studio
echo 3. Clean and rebuild the solution
echo.

echo Or run this command to build from command line:
echo "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" UtilityHub360.csproj /p:Configuration=Debug

echo.
echo The AutoMapper and MediatR errors should now be resolved.
pause
