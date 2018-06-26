@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

SET cl=%1
SET filesList=%2

FOR /F "tokens=*" %%i IN (%filesList%) DO (
	set file=%%i
	echo move !file! to cl %cl%
	p4 reopen -c %cl% !file!
)

ENDLOCAL