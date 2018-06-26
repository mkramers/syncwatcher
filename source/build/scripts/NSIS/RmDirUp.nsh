Function un.RMDirUP
         !define RMDirUP '!insertmacro RMDirUPCall'
 
         !macro RMDirUPCall _PATH
                push '${_PATH}'
                Call un.RMDirUP
         !macroend
 
         ; $0 - current folder
         ClearErrors
 
         Exch $0
         ;DetailPrint "ASDF - $0\.."
         RMDir "$0\.."
 
         IfErrors Skip
         ${RMDirUP} "$0\.."
         Skip:
 
         Pop $0
FunctionEnd