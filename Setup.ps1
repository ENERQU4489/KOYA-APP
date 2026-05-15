$AppName = "KOYA"
$ExeName = "KOYA-APP.exe"
$SourcePath = Join-Path $PSScriptRoot "bin\Release\net8.0-windows\win-x64\publish\$ExeName"
$InstallDir = Join-Path $env:LOCALAPPDATA "KOYA"
$ShortcutPath = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\$AppName.lnk"

if (-not (Test-Path $SourcePath)) {
    Write-Host "BŁĄD: Nie znaleziono pliku .exe w folderze publish!" -ForegroundColor Red
    exit
}

Write-Host "Instalowanie KOYA..." -ForegroundColor Cyan

# 1. Tworzenie folderu
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir
}

# 2. Kopiowanie pliku
Copy-Item $SourcePath (Join-Path $InstallDir $ExeName) -Force

# 3. Tworzenie skrótu w Menu Start
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = Join-Path $InstallDir $ExeName
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.IconLocation = Join-Path $InstallDir $ExeName
$Shortcut.Save()

# 4. Dodanie do Autostartu (opcjonalnie)
$RegistryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Set-ItemProperty -Path $RegistryPath -Name $AppName -Value (Join-Path $InstallDir $ExeName)

Write-Host "SUKCES! KOYA jest teraz w Menu Start i będzie startować z systemem." -ForegroundColor Green
Write-Host "Możesz teraz wpisać 'KOYA' w wyszukiwarkę Windows." -ForegroundColor White
