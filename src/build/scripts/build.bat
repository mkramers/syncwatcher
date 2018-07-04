@echo off

set name=%1
set version=%~2
set final=%~3

set logDir=%final%\log
set log=%logDir%\log.txt
set doxyLog=%logDir%\%name%.doxygen.txt
set copyLog=%logDir%\%name%.copy.txt
set inspectLog=%logDir%\inspect.txt
set stage=staging
set scripts=..\scripts

set solutions=solutions.txt
set stages=stages.txt
set installerScript=buildInstaller.nsi
set installerVersionFile=version.txt

echo --------------------------------------------

if "%name%"=="" (
	echo Error: Name unspecified. Usage:
	echo.
	echo 	build Project "1.0.0.0" ..\final
	exit /b 1
	goto :eof
)

if "%version%"=="" (
	echo Error: Version unspecified. Usage:
	echo.
	echo 	build Project "1.0.0.0" ..\final
	exit /b 1
	goto :eof
)

if "%final%"=="" (
	echo Error: Final path unspecified. Usage:
	echo.
	echo 	build "1.0.0.0" ..\final
	exit /b 1
	goto :eof
)

echo Starting build: %name% v%version%.

if not exist "%final%", mkdir "%final%"
if not exist "%logDir%", mkdir "%logDir%"

echo Removing staging dir: %stage%
if exist %stage%, rmdir /s /q %stage%
mkdir %stage%

set prebuild=prebuild.bat
if exist %prebuild% (
	echo Running prebuild script
	call prebuild %stage%
	if ERRORLEVEL 1 (
		call :onError "Prebuild script failed."
		goto :eof
	)
)

::build

echo Compiling %name% source...
call %scripts%\compile %solutions% %logDir%
if ERRORLEVEL 1 (
	call :onError "Compile failed."
	goto :eof
)
::echo Inspecting code...
::set inspectExe=c:\tools\resharper\inspectcode.exe
::if not exist %inspectExe%, echo Unable to find %inspectExe% & exit 1
::call %scripts%\inspectSolutions %solutions% %logDir% %inspectExe% >> %inspectLog%

::echo Generating docs...
::call %scripts%\createDocs %name%.doxyfile %final%\docs docs %name%.pdf %doxyLog%

::stage

echo Staging %name% release...
call %scripts%\stage %stage% %stages% %copyLog%

call robocopy . %stage% *.nsi >> %copyLog%
call robocopy . %stage% %installerVersionFile% >> %copyLog%

echo !define Version %version% > %stage%\version.txt

if ERRORLEVEL 3 (
	call :onError "Staging failed."
	goto :eof
)

::package

echo Create installer from script: %installerScript%...
set nsisLog=%logDir%\nsis.txt
makensis.exe %stage%\%installerScript% > %nsisLog%

if ERRORLEVEL 1 (
	call :onError "Create installer failed."
	goto :eof
)

call del %installerVersionFile% /s /q

if ERRORLEVEL 1 (
	call :onError "Installer cleanup failed."
	goto :eof
)

::finalize

echo Copying to final: "%final%"

call robocopy %stage% "%final%" *.exe /MOVE /np /nc >> %copyLog%
if ERRORLEVEL 4 (
	call :onError "Finalize failed"
	goto :eof
)

::cleanup

echo Cleanup...
call rmdir /s /q %stage%

if ERRORLEVEL 1 (
echo %ERRORLEVEL%
	call :onError "Cleanup failed"
	goto :eof
)

echo Build succeeded!
echo --------------------------------------------
exit /b 0
goto :eof

:onError
set message=%~1
echo 	Error: %message%. Exiting!
echo --------------------------------------------
exit /b 1
goto :eof