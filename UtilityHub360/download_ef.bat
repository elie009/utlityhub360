@echo off
echo Downloading Entity Framework DLLs...

REM Create lib directory if it doesn't exist
if not exist "..\packages\EntityFramework.6.4.4\lib\net45" mkdir "..\packages\EntityFramework.6.4.4\lib\net45"

REM Download Entity Framework using PowerShell
powershell -Command "& {Invoke-WebRequest -Uri 'https://www.nuget.org/api/v2/package/EntityFramework/6.4.4' -OutFile '..\packages\EntityFramework.6.4.4\EntityFramework.6.4.4.nupkg'}"

REM Extract the nupkg file (rename to zip first)
ren "..\packages\EntityFramework.6.4.4\EntityFramework.6.4.4.nupkg" "..\packages\EntityFramework.6.4.4\EntityFramework.6.4.4.zip"
powershell -Command "& {Expand-Archive -Path '..\packages\EntityFramework.6.4.4\EntityFramework.6.4.4.zip' -DestinationPath '..\packages\EntityFramework.6.4.4' -Force}"

REM Copy DLLs to bin directory
copy "..\packages\EntityFramework.6.4.4\lib\net45\EntityFramework.dll" "bin\"
copy "..\packages\EntityFramework.6.4.4\lib\net45\EntityFramework.SqlServer.dll" "bin\"

echo Entity Framework DLLs downloaded and copied to bin directory.
pause
