!include x64.nsh #Detect 64 bit OS
!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"
!include "..\NSIS\RmDirUp.nsh"

RequestExecutionLevel admin
CRCCheck force

!define MUI_ABORTWARNING
!define CSIDL_COMMONAPPDATA '0x23' ;Common Application Data path

!define PRODUCT "SystemCleaner"
!define INCLUDE_DEPENDENCIES
;!undef INCLUDE_DEPENDENCIES

;Name and file
Name "${PRODUCT}"
OutFile "${PRODUCT}.exe"

;!insertmacro MUI_PAGE_WELCOME  
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

!macro RemoveUninstallEntry file file2
  push ${file}
  push ${file2}
  call RemoveUninstallEntry
!macroend

Function RemoveUninstallEntry
	pop $R1
	pop $R0

	DetailPrint "$R0 $R1"  
	
	SetRegView 64
	DeleteRegKey HKLM "Software\${COMPANY}\${PRODUCT}"
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANY}\${PRODUCT}"
	
	DeleteRegKey /ifempty HKLM "Software\${COMPANY}"
	DeleteRegKey /ifempty HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANY}"
		
	SetShellVarContext all	
	Delete "$SMPROGRAMS\${COMPANY}\${PRODUCT}\${PRODUCT}.lnk"
	RmDir "$SMPROGRAMS\${COMPANY}\${PRODUCT}"
	Delete "$DESKTOP\${PRODUCT}.lnk"
	
FunctionEnd

Section "Install" Installation	
	
	SetRegView 64
	DeleteRegKey HKLM "Software\FocalHealthcare\FusionBx"
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANY}\${PRODUCT}"
	DeleteRegKey /ifempty HKLM "Software\${COMPANY}"
	DeleteRegKey /ifempty HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANY}"
	
SectionEnd