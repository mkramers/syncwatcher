@echo off

set outputDir=final\docs

set project=fusionmrpro
pushd ..\%project%
call ..\scripts\createDocs %project%.doxyfile %outputDir% docs %project%.pdf > doxylog.txt
popd