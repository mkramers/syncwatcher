@echo off

cls

set buildConfig=Release

REM get git describe label
set tmpFile=gitdescribe.tmp
git describe --tags --long --dirty --always > %tmpFile%
set /p gitDescribe=<%tmpFile%
del %tmpFile%

REM get git branch name
set tmpFile=gitBranch.tmp
git rev-parse --abbrev-ref HEAD > %tmpFile%
set /p gitBranch=<%tmpFile%
del %tmpFile%

set buildNumber=666

echo Git Branch %gitBranch%
echo Git Label %gitDescribe%
echo Build Number %buildNumber%

powershell -File .\build.ps1 -buildNumber="%buildNumber%" -gitVersion="%gitDescribe%" -gitBranch="%gitBranch%" --buildconfig="%buildConfig%"

exit /b