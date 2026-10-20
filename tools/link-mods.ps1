# Enlaza cada mod de C:\MODS_Rimmworld\Mods a la carpeta Mods del juego con un junction,
# para que RimWorld vea cada compilación sin copiar archivos. Se puede ejecutar varias veces.
param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld"
)

$repoMods = Join-Path $PSScriptRoot "..\Mods" | Resolve-Path
$gameMods = Join-Path $GameDir "Mods"

Get-ChildItem $repoMods -Directory | ForEach-Object {
    $link = Join-Path $gameMods $_.Name
    if (Test-Path $link) {
        Write-Host "Ya existe: $link"
    } else {
        New-Item -ItemType Junction -Path $link -Target $_.FullName | Out-Null
        Write-Host "Enlazado: $link -> $($_.FullName)"
    }
}
