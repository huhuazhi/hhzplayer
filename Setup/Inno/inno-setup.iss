
#define MyAppName "hhzplayer"
#define MyAppExeName "hhzplayer.exe"
#define MyAppSourceDir "..\..\MpvNet.Windows\bin\Release"
#define MyAppVersion GetFileVersion("..\..\MpvNet.Windows\bin\Release\hhzplayer.exe")

[Setup]
AppId={{9AA2B100-BEF3-44D0-B819-D8FC3C4D557D}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=Frank Skare (stax76)
ArchitecturesInstallIn64BitMode=x64
Compression=lzma2
DefaultDirName={autopf}\{#MyAppName}
OutputBaseFilename=hhzplayer-v{#MyAppVersion}-setup-x64
OutputDir=C:\Users\hhz\Desktop
DefaultGroupName={#MyAppName}
SetupIconFile=..\..\MpvNet.Windows\mpv-icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Files]
Source: "{#MyAppSourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Excludes: "win-x64,win-arm64"; Flags: ignoreversion recursesubdirs createallsubdirs;
