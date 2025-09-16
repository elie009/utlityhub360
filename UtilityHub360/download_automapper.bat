@echo off
echo Downloading AutoMapper package...

REM Create packages directory if it doesn't exist
if not exist "..\packages\AutoMapper.12.0.1" mkdir "..\packages\AutoMapper.12.0.1"

REM Download AutoMapper using PowerShell
powershell -Command "Invoke-WebRequest -Uri 'https://www.nuget.org/api/v2/package/AutoMapper/12.0.1' -OutFile '..\packages\AutoMapper.12.0.1\AutoMapper.12.0.1.nupkg'"

REM Extract the nupkg file (rename to zip first)
ren "..\packages\AutoMapper.12.0.1\AutoMapper.12.0.1.nupkg" "..\packages\AutoMapper.12.0.1\AutoMapper.12.0.1.zip"
powershell -Command "Expand-Archive -Path '..\packages\AutoMapper.12.0.1\AutoMapper.12.0.1.zip' -DestinationPath '..\packages\AutoMapper.12.0.1' -Force"

REM Copy DLLs to bin directory
copy "..\packages\AutoMapper.12.0.1\lib\net48\AutoMapper.dll" "bin\"

echo AutoMapper package downloaded and copied to bin directory.
pause
