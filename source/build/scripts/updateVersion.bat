@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

SET assembliesList=%1
SET version=%2

SET avuPath="..\bin\avu2.exe"

::update version files

echo --------------------------------------------
echo Updating versions from files in %assembliesList%

for /f %%a in (%assembliesList%) do (
	set assembly=%%a
	call p4 edit !assembly!
	if ERRORLEVEL 1 (
		call :onError "Failed to check out file."
		goto :eof
	)
	
	echo Updating !assembly! v%version%
	call %avuPath% !assembly! %version%	
	if ERRORLEVEL 1 (
		call :onError "Failed to update assembly."
		goto :eof
	)
)

echo --------------------------------------------
exit /b 0
goto :eof

:onError
set message=%~1
echo 	Error: %message%: %ERRORLEVEL%. Exiting!
echo --------------------------------------------
exit /b 1
goto :eof

ENDLOCAL