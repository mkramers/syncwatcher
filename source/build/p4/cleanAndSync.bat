@echo on

SETLOCAL ENABLEDELAYEDEXPANSION

set directoryList=%1

for /f %%a in (%directoryList%) do (
	SET directory=%%a
	echo Cleaning !directory!...
	p4 clean !directory!
	echo Syncing !directory!
	p4 sync -f !directory!
)

ENDLOCAL