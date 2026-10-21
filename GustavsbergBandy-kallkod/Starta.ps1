$ErrorActionPreference = 'Stop'
$executable = Join-Path $PSScriptRoot 'app\WinUI.exe'
if (!(Test-Path -LiteralPath $executable)) {
    throw 'Bygg appen först enligt README.md, eller packa upp hela Windows-paketet.'
}
Start-Process -FilePath $executable -WorkingDirectory (Split-Path $executable)
