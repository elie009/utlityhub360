@echo off
echo Reinstalling AutoMapper
echo =======================
echo.

echo Step 1: Removing old AutoMapper reference...
powershell -Command "(Get-Content 'UtilityHub360.csproj') -replace '    <Reference Include=\"AutoMapper\">.*?</Reference>', ''" | Set-Content 'UtilityHub360_temp.csproj'
move UtilityHub360_temp.csproj UtilityHub360.csproj

echo Step 2: Installing AutoMapper 9.0.0...
nuget install AutoMapper -Version 9.0.0 -OutputDirectory packages

echo Step 3: Adding AutoMapper reference to project file...
powershell -Command "
$content = Get-Content 'UtilityHub360.csproj'
$newRef = '    <Reference Include=\"AutoMapper\">', '      <HintPath>..\packages\AutoMapper.9.0.0\lib\net461\AutoMapper.dll</HintPath>', '    </Reference>'
$insertIndex = ($content | Select-String -Pattern '</Reference>' | Select-Object -Last 1).LineNumber
$content[0..$insertIndex] + $newRef + $content[($insertIndex+1)..($content.Length-1)] | Set-Content 'UtilityHub360.csproj'
"

echo Step 4: Cleaning and rebuilding...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo.
echo AutoMapper reinstalled successfully!
echo Please restart Visual Studio and rebuild the project.
pause
