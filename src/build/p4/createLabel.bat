@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

SET labelName=%~1
SET description=%~2
SET owner=%3
SET viewsList=%4
SET labelFile=label.txt

(
	@echo Label:	%labelName%
	@echo.
	@echo Owner:	%owner%
	@echo.
	@echo Description:
	@echo 	%description%
	@echo.
	@echo Options:	unlocked noautoreload	
	@echo.
	@echo View:
	@echo 	//depot/...
)> %labelFile%

call p4 label -i < %labelFile%

del %labelFile%

for /f %%a in (%viewsList%) do (
	SET directory=%%a
	p4 tag -l %labelName% !directory!
)

ENDLOCAL