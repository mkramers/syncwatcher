!define PRODUCT "SyncWatcherTray"
!define CONFIGURATION "Release"
!define BIN "..\bin\${CONFIGURATION}"

RequestExecutionLevel admin ;Require admin rights (When UAC is turned on)

Name "${PRODUCT}" 

!system 'md "${PUBLISH_DIR}"'

OutFile "${PUBLISH_DIR}\syncwatchertray_${SHORTVERSION}.exe"
 
# define installation directory
InstallDir "$PROGRAMFILES64\${PRODUCT}"
 
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
	File "${BIN}\Newtonsoft.Json.dll"
	File "${BIN}\PlexTools.dll" 
	File "${BIN}\SyncWatcherTray.exe"
	File "${BIN}\SyncWatcherTray.exe.config"
	File "${BIN}\System.Numerics.Vectors.dll"
	File "${BIN}\System.Windows.Interactivity.dll"
	File "${BIN}\Themes.dll"
	File "${BIN}\WinSCP.exe"
	File "${BIN}\WinScpApi.dll"
	File "${BIN}\WinSCPnet.dll"
	File "${BIN}\Xceed.Wpf.AvalonDock.dll"
	File "${BIN}\Xceed.Wpf.AvalonDock.Themes.Aero.dll"
	File "${BIN}\Xceed.Wpf.AvalonDock.Themes.Metro.dll"
	File "${BIN}\Xceed.Wpf.AvalonDock.Themes.VS2010.dll"
	File "${BIN}\Xceed.Wpf.DataGrid.dll"
	File "${BIN}\Xceed.Wpf.Toolkit.dll"
	
    SetOutPath "$INSTDIR\Config"
	File /r "${BIN}\Config\*.*"
					
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
	Delete "$INSTDIR\Newtonsoft.Json.dll"
	Delete "$INSTDIR\PlexTools.dll" 
	Delete "$INSTDIR\SyncWatcherTray.exe"
	Delete "$INSTDIR\SyncWatcherTray.exe.config"
	Delete "$INSTDIR\System.Numerics.Vectors.dll"
	Delete "$INSTDIR\System.Windows.Interactivity.dll"
	Delete "$INSTDIR\Themes.dll"
	Delete "$INSTDIR\WinSCP.exe"
	Delete "$INSTDIR\WinScpApi.dll"
	Delete "$INSTDIR\WinSCPnet.dll"
	Delete "$INSTDIR\Xceed.Wpf.AvalonDock.dll"
	Delete "$INSTDIR\Xceed.Wpf.AvalonDock.Themes.Aero.dll"
	Delete "$INSTDIR\Xceed.Wpf.AvalonDock.Themes.Metro.dll"
	Delete "$INSTDIR\Xceed.Wpf.AvalonDock.Themes.VS2010.dll"
	Delete "$INSTDIR\Xceed.Wpf.DataGrid.dll"
	Delete "$INSTDIR\Xceed.Wpf.Toolkit.dll"
		
	RMDir $INSTDIR
	  
# uninstaller section end
SectionEnd