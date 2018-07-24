@echo off

cls 

set clean=%1
echo clean? %clean%

set dest=D:\Unsorted\completed2

if "%clean%" == "clean" (
	echo Cleaning %dest%
	if exist %dest%, rmdir /s /q %dest%
	mkdir %dest%
)

robocopy D:\Unsorted\testdata %dest%