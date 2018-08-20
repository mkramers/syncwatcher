@echo off

set targetDir=%~1

set filebot=..\..\..\..\external\filebot
set destination="%targetDir%filebot"

echo.
echo -----post build
echo.

echo Cleaning %destination%...
if exist %destination%, rmdir /s /q %destination%

echo Copying %filebot% to %destination%...
(robocopy %filebot% %destination% /s /njh /njs /ndl /nfl /nc /ns /np /is ) ^& IF %ERRORLEVEL% GTR 3 exit %ERRORLEVEL%

set settings=..\..\settings
set settingsDestination="%targetDir%settings"

echo Cleaning %settingsDestination%...
if exist %settingsDestination%, rmdir /s /q %settingsDestination%

echo Copying %settings% to %settingsDestination%...
(robocopy %settings% %settingsDestination% /s /njh /njs /ndl /nfl /nc /ns /np /is) ^& IF %ERRORLEVEL% GTR 3 exit %ERRORLEVEL%

echo.
echo -----end post build
echo.

::prepare test data
::set testSource="F:\Videos\TV Shows\Anne\Season 01"
::set testSource="F:\Videos\TV Shows\Mad Men\Season 05"
::set testDestination=D:\Unsorted\completed
::(robocopy %testSource% %testDestination% /s /is /mov) ^& IF %ERRORLEVEL% GTR 3 exit %ERRORLEVEL%

exit 0