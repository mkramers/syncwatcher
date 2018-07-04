@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

set solutionsFile=%1
set inspectionLogDir=%2
set inspectCodeExe=%3

for /F "usebackq tokens=* delims=" %%A in (%solutionsFile%) do (
    set entry=%%A
    call :inspect
)

goto :eof

:inspect
for /F "usebackq tokens=1,2,3,4,5 delims=~" %%1 in ('%entry:,=~%') do (
    set solution=%%1
    set solutionName=%%5
	
	echo Inspecting !solutionName! ^(!solution!^) ...
	echo.	
	call %inspectCodeExe% !solution! -o=%inspectionLogDir%\!solutionName!.inspection.xml
)

ENDLOCAL