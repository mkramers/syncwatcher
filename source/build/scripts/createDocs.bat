@echo off

set configurationFile=%1
set outputDir=%2
set docsDir=%3
set outputFilename=%4
set logFile=%5

set latexDir=%docsDir%\latex
set makeFile=%latexDir%\make.bat
set generatedPdf=refman.pdf

set skipMode=false
if "%skipMode%"=="true", goto :skip

if exist %docsDir%, echo Cleaning docs dir: %docsDir% & rmdir %docsDir% /s /q
if %ERRORLEVEL% GTR 0 goto :error

if exist %outputDir%, echo Cleaning output: %outputDir% & rmdir %outputDir% /s /q
if %ERRORLEVEL% GTR 0 goto :error

echo Parsing doxygen using: %configurationFile%
doxygen %configurationFile% > %logFile%
if %ERRORLEVEL% GTR 0 goto :error

(
	echo.
	echo. Creating pdf
	echo. 
) >> %logFile%

echo Creating pdf using: %makeFile%
call %makeFile% >> %logFile%
if %ERRORLEVEL% GTR 0 goto :error

:skip

(
	echo.
	echo. Moving pdf output...
	echo. 
) >> %logFile%

echo Copying generated docs to %outputDir%
robocopy %latexDir% %outputDir% %generatedPdf% /nc /ns /np >> %logFile%
if %ERRORLEVEL% GTR 3 goto :error

echo Renaming output to %outputFilename%
ren %outputDir%\refman.pdf %outputFilename% >> %logFile%
if %ERRORLEVEL% GTR 0 goto :error

if "%skipMode%"=="true", goto :eof

echo Cleaning docs dir: %docsDir%
rmdir %docsDir% /s /q
if %ERRORLEVEL% GTR 0 goto :error

goto :eof

:error
echo Error!
::exit 3