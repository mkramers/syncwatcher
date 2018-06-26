@echo off

cls

set name=%1
set buildNumber=%2
set finalPath=%~3
set archive=%~4

set currentDir=%cd%
set finalTemp=%currentDir%\temp
set logDir=%finalTemp%\log
set copyLog=%logDir%\%name%.copy.txt
set p4Log=%logDir%\p4.txt
set scripts=..\scripts

set assemblies=assemblies.txt
set currentVersionFile=%currentDir%\currentVersion.txt
set versionsFile=%currentDir%\versions.xml

echo.
echo ...
echo Starting %name% buildNew.
echo ...
echo.


if "%name%"=="" (
	echo Error: Name must be specified.
	goto displayUsage
	goto :eof
)
if "%buildNumber%"=="" (
	echo Error: Build number must be specified.
	goto displayUsage
	goto :eof
)
if "%finalPath%"=="" (
	echo Error: Final path must be specified.
	goto displayUsage
	goto :eof
)

if exist %finalTemp%, echo Cleaning final: %finalTemp% & rmdir %finalTemp% /s /q
mkdir %finalTemp%

if exist %finalPath%, echo Cleaning final: %finalPath% & rmdir %finalPath% /s /q

if exist %logDir%, echo Cleaning log dir: %logDir% &  rmdir %logDir% /s /q
mkdir %logDir%

pushd %scripts%
set buildToolsLog=%logDir%\buildtools.build.txt
echo Building build tools @%buildToolsLog%...
call getBuildTools ..\bin %logDir%
popd

if ERRORLEVEL 1 (
	echo Error: Unable to generate build tools!
	goto :finalize
)

echo Getting version number...
set tempVersionFile=tmp.txt
call ..\bin\BuildTool -getfrombuildnumber %currentVersionFile% %buildNumber% %tempVersionFile% >> %p4Log%
set /p version=<%tempVersionFile%
del %tempVersionFile%
if ERRORLEVEL 1 (
	call :onError "Failed to get version number."
	goto :finalize
)
if "%version%"=="" (
	call :onError "Failed to get version number."
	goto :finalize
)

echo Updating versions to v%version%...
call %scripts%\updateVersion %assemblies% %version%
if ERRORLEVEL 1 (
	call :onError "Failed to update version(s)."
	goto :finalize
)

echo Building v%version%...
call %scripts%\build %name% %version% "%finalTemp%"
if ERRORLEVEL 1 (
	call :onError "Failed to build."
	goto :finalize
)

goto :finalize

:displayUsage
echo Usage: buildNew ^<name^> ^<buildNumber^> ^<versionfile^> ^<outputdir
goto :eof

:onError
set message=%~1
echo 	Error: %message%: %ERRORLEVEL%. Exiting!
echo --------------------------------------------
exit /b 1
goto :eof

:finalize

set error=%ERRORLEVEL%

::log may change!
set logDir=%finalTemp%\log
set log=%logDir%\log.txt

echo Moving to destination: "%finalPath%"
robocopy %finalTemp% "%finalPath%" /s /ns /np /nc >> %copyLog%
if ERRORLEVEL 4 (
	call :onError "Finalize failed."
	goto :eof
)

echo Reverting assemblies from %assemblies%
for /f %%a in (%assemblies%) do (
	call p4 revert %%a >> %p4Log%
)

echo Cleanup build tools
if exist  ..\bin, rmdir /s /q  ..\bin

if exist  %finalTemp%, rmdir /s /q  %finalTemp%

if ERRORLEVEL 1 (
	call :onError "Cleanup failed."
goto :eof
)

if %error% EQU 0 (
	echo Build new succeeded!
	
	echo Archiving to %archive%
	call robocopy "%finalPath%" "%archive%" /e /nc /np
	if %ERRORLEVEL% LEQ 3, exit /b 0
)

if %error% GTR 0 (
	echo Build failed! Exit code: %error%
	set error=1
)

echo --------------------------------------------
exit /b %error%

goto :eof