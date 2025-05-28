[Setup]
AppName=UnifiedEmail
AppVersion=1.0
DefaultDirName={pf}\UnifiedEmail
DefaultGroupName=UnifiedEmail
OutputDir=Output
OutputBaseFilename=UnifiedEmailInstaller
Compression=lzma
SolidCompression=yes

[Files]
Source: "C:\Users\zaldo\Documents\GitHub\UnifiedEmail\UnifiedEmail\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\UnifiedEmail"; Filename: "{app}\UnifiedEmail.exe"
Name: "{commondesktop}\UnifiedEmail"; Filename: "{app}\UnifiedEmail.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"
