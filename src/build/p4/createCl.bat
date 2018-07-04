@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

SET description=%~1
SET changelistFileName=clDescription.txt
SET clOutput=cl.txt
SET tempFile=temp.txt

(
	@echo Change:	new
	@echo.
	@echo Status:	new
	@echo.
	@echo Description:
	@echo 	%description%
)> %changelistFileName%

(p4 change -i < %changelistFileName%) > %tempFile%

del %changelistFileName%

FOR /F "tokens=*" %%i IN (%tempFile%) DO (
	set line=%%i
	set CHANGELIST=!line:~7,2!	
	@echo !CHANGELIST!>%clOutput%
	goto :found
)

:found
del %tempFile%

ENDLOCAL