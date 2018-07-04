@echo off

set name=%1
set assemblies=%2
set currentVersionFile=%~3
set versionsFile=%~4

echo Getting version number...
echo.
set tempVersionFile=tmp.txt
call ..\bin\P4Executable -next %currentVersionFile% %versionsFile% %tempVersionFile%
set /p version=<%tempVersionFile%
del %tempVersionFile%

echo Updating versions to v%version%...
echo.
call ..\scripts\updateVersion %assemblies% %version%