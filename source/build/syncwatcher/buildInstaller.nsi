!define CONFIGURATION "Release"
!define BIN "..\..\projects\SyncWatcherTray\bin\${CONFIGURATION}"

RequestExecutionLevel admin ;Require admin rights (When UAC is turned on)

# define name of installer
OutFile "syncwatcher.exe"
 
# define installation directory
InstallDir $PROGRAMFILES64\SyncWatcherTray
 
# start default section
Section
 
    # set the installation directory as the destination for the following actions
    SetOutPath $INSTDIR
	
	File "${BIN}\Common.dll"
	File "${BIN}\FilebotApi.dll"
	File "${BIN}\FontAwesome.WPF.dll"
	File "${BIN}\GalaSoft.MvvmLight.dll"
	File "${BIN}\GalaSoft.MvvmLight.Extras.dll"
	File "${BIN}\GalaSoft.MvvmLight.Platform.dll"
	File "${BIN}\Hardcodet.Wpf.TaskbarNotification.dll"
	File "${BIN}\log4net.dll"
	File "${BIN}\MultiSelectTreeView.dll"
	File "${BIN}\MVVM.dll"
	File "${BIN}\SyncWatcherTray.exe"
	File "${BIN}\SyncWatcherTray.exe.config"
	File "${BIN}\System.Numerics.Vectors.dll"
	File "${BIN}\System.Windows.Interactivity.dll"
	File "${BIN}\Themes.dll"
	File "${BIN}\WinSCP.exe"
	File "${BIN}\WinScpApi.dll"
	File "${BIN}\WinSCPnet.dll"
	
    SetOutPath "$INSTDIR\Config"
	File /r "${BIN}\Config\*.*"
	
    SetOutPath "$INSTDIR\filebot"
	File /r "${BIN}\filebot\*.*"
	
    SetOutPath "$INSTDIR\settings"
	File /r "${BIN}\settings\*.*"
				
	#create startup shortcut
	SetOutPath $INSTDIR
	CreateShortCut "$SMPROGRAMS\Startup\SyncWatcherTray.lnk" "$INSTDIR\SyncWatcherTray.exe" \
	"" "$INSTDIR\SyncWatcherTray.exe" 2 SW_SHOWNORMAL \
	ALT|CTRL|SHIFT|F5 "Run SyncWatcherTray"
 
    # create the uninstaller
    WriteUninstaller "$INSTDIR\uninstall.exe"
SectionEnd
 
# uninstaller section start
Section "uninstall"
 
    # first, delete the uninstaller
    Delete "$INSTDIR\uninstall.exe"
		
	Delete "$SMPROGRAMS\Startup\SyncWatcherTray.lnk"
		
	RMDir /r "$INSTDIR\Config"
	RMDir /r "$INSTDIR\filebot"
	RMDir /r "$INSTDIR\settings"	
				
	Delete "$INSTDIR\Common.dll"
	Delete "$INSTDIR\FilebotApi.dll"
	Delete "$INSTDIR\FontAwesome.WPF.dll"
	Delete "$INSTDIR\GalaSoft.MvvmLight.dll"
	Delete "$INSTDIR\GalaSoft.MvvmLight.Extras.dll"
	Delete "$INSTDIR\GalaSoft.MvvmLight.Platform.dll"
	Delete "$INSTDIR\Hardcodet.Wpf.TaskbarNotification.dll"
	Delete "$INSTDIR\log4net.dll"
	Delete "$INSTDIR\MultiSelectTreeView.dll"
	Delete "$INSTDIR\MVVM.dll"
	Delete "$INSTDIR\SyncWatcherTray.exe"
	Delete "$INSTDIR\SyncWatcherTray.exe.config"
	Delete "$INSTDIR\System.Numerics.Vectors.dll"
	Delete "$INSTDIR\System.Windows.Interactivity.dll"
	Delete "$INSTDIR\Themes.dll"
	Delete "$INSTDIR\WinSCP.exe"
	Delete "$INSTDIR\WinScpApi.dll"
	Delete "$INSTDIR\WinSCPnet.dll"
		
	RMDir $INSTDIR
	  
# uninstaller section end
SectionEnd