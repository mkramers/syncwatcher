@echo off

set buildBin=%~1
set logDir=%~2
set buildLog=%logDir%\buildtools.build.txt
set copyLog=%logDir%\buildtools.copy.txt

set solution=..\BuildTools\BuildTools.sln
set toolsVersion=14.0
set configuration=Release
set platform=Any CPU

::build bin
echo --------------------------------------------
echo Build build tools...
set binSource=..\BuildTools\BuildTool\bin\Release

if exist %binSource%, rmdir /s /q %binSource%
if ERRORLEVEL 1 (
	call :onError "Unable to remove %binSource%"
	goto :eof
)

echo Restoring nuget packages...
..\BuildTools\nuget.exe restore %solution%

set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
call %msbuild% %solution% /t:Rebuild /p:Platform="%platform%" /p:Configuration=%configuration% /v:minimal /maxcpucount:4 
REM >> %buildLog%
if ERRORLEVEL 1 (
	call :onError "Compile failed"
	goto :eof
)

::copy bin
echo Copying build tools to %buildBin%
robocopy /s /np /nc %binSource% %buildBin% > %copyLog%
if ERRORLEVEL 3 (
	call :onError "Copy build tools failed"
	goto :eof
)

echo --------------------------------------------
exit /b 0
goto :eof

:onError
set message=%~1
echo 	Error: %message%. Exiting!
echo --------------------------------------------
exit /b 1
goto :eof
