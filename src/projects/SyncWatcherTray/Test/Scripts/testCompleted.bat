@echo off

cls 

set mode=%1
echo mode: %mode%

set dest=D:\Unsorted\completed2

if "%mode%" == "clean" (
	call :clean
	goto :eof
)

if "%mode%" == "refresh" (

	call :clean
	call :copy
	goto :eof
)

if "%mode%" == "copy" (
	call :copy
	goto :eof
)

:clean
	echo Cleaning %dest%
	if not exist %dest%, mkdir %dest%
	del /q "%dest%\*"
	FOR /D %%p IN ("%dest%\*.*") DO rmdir "%%p" /s /q
	goto :eof
	
:copy
	robocopy D:\Unsorted\testdata %dest%
	goto :eof