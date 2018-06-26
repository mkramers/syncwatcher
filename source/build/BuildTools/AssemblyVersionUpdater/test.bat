@echo off

SET avuPath="bin/x64/Release/AssemblyVersionUpdater.exe"
SET path="Properties/AssemblyInfo.cs"
SET version="1.6.6.2"

%avuPath% %path% %version%