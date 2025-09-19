@echo off
echo Testing AutoMapper Package
echo ==========================
echo.

echo Checking if AutoMapper DLL exists...
if exist "packages\AutoMapper.9.0.0\lib\net461\AutoMapper.dll" (
    echo ✓ AutoMapper DLL found
    echo File size: 
    dir "packages\AutoMapper.9.0.0\lib\net461\AutoMapper.dll"
) else (
    echo ✗ AutoMapper DLL not found
    echo Installing AutoMapper...
    nuget install AutoMapper -Version 9.0.0 -OutputDirectory packages
)

echo.
echo Checking project file reference...
findstr /C:"AutoMapper" UtilityHub360.csproj

echo.
echo Testing compilation...
echo using AutoMapper; > test.cs
echo class Test { } >> test.cs
csc /reference:"packages\AutoMapper.9.0.0\lib\net461\AutoMapper.dll" test.cs
if %errorlevel% equ 0 (
    echo ✓ AutoMapper compiles successfully
    del test.cs test.exe
) else (
    echo ✗ AutoMapper compilation failed
)

echo.
pause
