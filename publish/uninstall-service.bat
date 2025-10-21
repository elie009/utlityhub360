@echo off
echo Uninstalling UtilityHub360 Windows Service...

sc stop "UtilityHub360"
sc delete "UtilityHub360"

echo Service uninstalled successfully!
pause
