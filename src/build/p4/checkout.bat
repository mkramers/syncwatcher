@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

SET fileList=%1
SET cl=%2

for /f %%a in (%fileList%) do (
SET file=%%a
echo Checking out !file! to CL %cl%
p4 edit -c %cl% !file!
)

ENDLOCAL