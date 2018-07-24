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
	if exist %dest%, rmdir /s /q %dest%
	mkdir %dest%	
	goto :eof
	
:copy
	robocopy D:\Unsorted\testdata %dest%
	goto :eof