@echo off
echo Running Entity Framework migration to rename Loans to LnLoans...
echo.

REM Check if the project builds first
echo Building project...
msbuild UtilityHub360.csproj /p:Configuration=Debug
if %errorlevel% neq 0 (
    echo ERROR: Project build failed. Please fix build errors before running migration.
    pause
    exit /b 1
)

echo.
echo Running Entity Framework migration...
echo.

REM Run the migration using Entity Framework
echo Executing: Update-Database -Script
powershell -Command "& {Add-Type -Path 'packages\EntityFramework.6.4.4\tools\EntityFramework.psm1'; Update-Database -Script -Verbose}"

if %errorlevel% equ 0 (
    echo.
    echo Migration script generated successfully!
    echo Please review the generated script before applying it to your database.
) else (
    echo.
    echo ERROR: Migration failed. Please check the error messages above.
)

echo.
pause
