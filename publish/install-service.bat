@echo off
echo Installing UtilityHub360 as Windows Service...

sc create "UtilityHub360" binPath="C:\path\to\your\publish\folder\UtilityHub360.exe" start=auto
sc description "UtilityHub360" "UtilityHub360 API Service"
sc start "UtilityHub360"

echo Service installed and started successfully!
pause
