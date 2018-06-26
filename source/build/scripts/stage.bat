@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

set stage=%1
set stagesFile=%2
set copyLog=%3

for /F "usebackq tokens=* delims=" %%A in (%stagesFile%) do (
    set entry=%%A
    call :stageProject
)
goto :eof

:stageProject
for /F "usebackq tokens=1,2,3 delims=~" %%1 in ('%entry:,=~%') do (
    set projectOutput=%%1
    set projectStage=%%2
	set exludeFilter=%%3
			
	if exist %stage%\%%2, rmdir /s /q %stage%\%%2
	mkdir %stage%\%%2

	call robocopy %%1 %stage%\%%2 %%~3 /s /np /nc >> %copyLog%
)

ENDLOCAL