!include "EnumUsersReg.nsh"
Name EnumUsersReg
OutFile EnumUsersReg.exe
 
ShowInstDetails show
 
Section
; use ${un.EnumUsersReg} in uninstaller sections
${EnumUsersReg} CallbackFunction temp.key
SectionEnd
 
Function CallbackFunction
	Pop $0
	 
	ReadRegStr $0 HKU "$0\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" "Local AppData"

	${If} "$0" != ""
		DetailPrint "Found: $0"
	${EndIf}	
 
FunctionEnd