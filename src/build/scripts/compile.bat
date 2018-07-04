@echo off

SETLOCAL ENABLEDELAYEDEXPANSION

set solutionsFile=%1
set logDir=%2

echo --------------------------------------------
echo Compiling solutions from %solutionsFile%

for /F "usebackq tokens=* delims=" %%A in (%solutionsFile%) do (
    set entry=%%A
    call :compile	
	if ERRORLEVEL 1 (
		call :onError "Failed to compile."
		goto :eof
	)
)

echo --------------------------------------------
exit /b 0
goto :eof

:compile
for /F "usebackq tokens=1,2,3,4,5 delims=~" %%1 in ('%entry:,=~%') do (
    set solution=%%1
    set toolsVersion=%%2
    set configuration=%%3
    set platform=%%4
    set solutionName=%%5
		
	echo Restoring nuget packages...
	..\BuildTools\nuget.exe restore !solution!
	
	if "!toolsVersion!" == "15.0" (
		REM set pathFile=msbuild15Path.txt
		REM call ..\scripts\MSBuildFinder\Program.exe !pathFile!
		REM set /p msbuild=<!pathFile!
		REM set msbuild="!msbuild!"
		REM del !pathFile!
		set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
		REM set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"	
	)
	if NOT "!toolsVersion!" == "15.0" (
		set msbuild="C:\Program Files (x86)\MSBuild\!toolsVersion!\Bin\MSBuild.exe"		
	)	
	
	call !msbuild! !solution! /t:Build /p:Platform="!platform!" /p:Configuration=!configuration! /v:minimal /maxcpucount:4 > %logDir%\!solutionName!.build.txt
)
goto :eof

:onError
set message=%~1
echo 	Error: %message%: (%ERRORLEVEL%). Exiting!
echo --------------------------------------------
exit /b 1
goto :eof
 
ENDLOCAL