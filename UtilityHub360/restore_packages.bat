@echo off
echo Restoring NuGet packages...

REM Try to find and use NuGet.exe
if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\CommonExtensions\Microsoft\NuGet\NuGet.exe" (
    "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\CommonExtensions\Microsoft\NuGet\NuGet.exe" restore
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\CommonExtensions\Microsoft\NuGet\NuGet.exe" (
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\CommonExtensions\Microsoft\NuGet\NuGet.exe" restore
) else if exist "C:\Program Files (x86)\NuGet\nuget.exe" (
    "C:\Program Files (x86)\NuGet\nuget.exe" restore
) else (
    echo NuGet.exe not found. Please install NuGet or restore packages manually in Visual Studio.
    echo You can also download NuGet.exe from https://www.nuget.org/downloads
)

echo Package restore completed.
pause

