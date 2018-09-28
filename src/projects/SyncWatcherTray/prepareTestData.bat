@echo off

set sourceRoot="F:\videos\TV Shows\The Simpsons\Season 04"
set destination="D:\sync"

REM set files="The Simpsons - S04E01*" "The Simpsons - S04E02*" "The Simpsons - S04E03*" "The Simpsons - S04E04*"
set files="The Simpsons - S04E05*"

robocopy %sourceRoot% %destination% %files% /MOV