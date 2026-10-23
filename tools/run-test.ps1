# Lanza una instancia de prueba AISLADA de RimWorld sin molestar al usuario:
#  - carpeta de datos propia (ModsConfig y Prefs aislados: español, modo desarrollador, corre en segundo plano)
#  - ventana pequeña que se minimiza enseguida sin robar el foco
#  - log propio; espera a que termine (o al timeout) y lo cierra
#
#   powershell -ExecutionPolicy Bypass -File tools\run-test.ps1 [-Version 1.6|1.7] [-GameArgs "-quicktest -cvgselftest"] [-TimeoutSec 900] [-Mods "a,b,c" | -Mods todos]
#
# Dos instalaciones lado a lado: 1.6 = copia sin Steam en C:\Games\RimWorld_1.6 (con junctions a Mods/Convergence.*),
# 1.7 = la de Steam (rama beta "unstable"). -GameDir pisa la elección.
param(
    [ValidateSet("1.6", "1.7")][string]$Version = "1.6",
    [string]$GameArgs = "-quicktest -cvgselftest",
    [int]$TimeoutSec = 900,
    [string]$Mods = "brrainz.harmony,ludeon.rimworld,ludeon.rimworld.royalty,ludeon.rimworld.ideology,ludeon.rimworld.biotech,ludeon.rimworld.anomaly,ludeon.rimworld.odyssey,prospero1088.convergence.core,prospero1088.convergence.resources,prospero1088.convergence.formations,prospero1088.convergence.races,prospero1088.convergence.traitsdestinies,prospero1088.convergence.sects,prospero1088.convergence.alchemy,prospero1088.convergence.apparel,prospero1088.convergence.faunaflora,prospero1088.convergence.stories,prospero1088.convergence.grafts,prospero1088.convergence.qitech,prospero1088.convergence.furniture,prospero1088.convergence.biotechwuxia,prospero1088.convergence.space",
    [string]$GameDir = "",
    # Tamaño de la ventana de prueba (pequeña por defecto; más grande solo para capturas de interfaz, p. ej. -cvgdevshot)
    [switch]$Visible,   # ventana normal (no minimizada) para capturas de pantalla (-cvgshowcase)
    [int]$Width = 640,
    [int]$Height = 360,
    # Idioma del juego de prueba: Spanish (por defecto) o English (capturas para Workshop)
    [ValidateSet("Spanish", "English")][string]$Language = "Spanish",
    # Pedido del usuario: las pruebas van al monitor secundario (ROG) y él trabaja en el principal (LG). La ventana queda visible ahí,
    # sin minimizar ni robar el foco. -NoSecondMonitor vuelve a la ventana minimizada de siempre.
    [switch]$NoSecondMonitor
)

# Pausa pedida por el usuario (está usando Steam/el juego): tools/.pausa-rimworld existe => no lanzar
$pausa = Join-Path $PSScriptRoot ".pausa-rimworld"
while (Test-Path $pausa) { Write-Host "Pausa del usuario: espero..."; Start-Sleep -Seconds 20 }

# -Mods todos: las DLC, VEF y los 15 mods Convergence en orden de carga (sin el Sects viejo, retirado)
if ($Mods -eq "todos") { $Mods = "brrainz.harmony,ludeon.rimworld,ludeon.rimworld.royalty,ludeon.rimworld.ideology,ludeon.rimworld.biotech,ludeon.rimworld.anomaly,ludeon.rimworld.odyssey,oskarpotocki.vanillafactionsexpanded.core,prospero1088.convergence.core,prospero1088.convergence.traitsdestinies,prospero1088.convergence.resources,prospero1088.convergence.races,prospero1088.convergence.alchemy,prospero1088.convergence.formations,prospero1088.convergence.apparel,prospero1088.convergence.biotechwuxia,prospero1088.convergence.factions,prospero1088.convergence.faunaflora,prospero1088.convergence.stories,prospero1088.convergence.qitech,prospero1088.convergence.grafts,prospero1088.convergence.furniture,prospero1088.convergence.space" }

$steamDir = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld"
if (-not $GameDir) {
    $GameDir = if ($Version -eq "1.6" -and (Test-Path "C:\Games\RimWorld_1.6")) { "C:\Games\RimWorld_1.6" } else { $steamDir }
}
$installed = (Get-Content (Join-Path $GameDir "Version.txt") -ErrorAction SilentlyContinue | Select-Object -First 1)
if ($installed -notlike "$Version.*") { Write-Host "En $GameDir está RimWorld '$installed', no $Version."; exit 3 }
# 1.6 = Unity (RimWorldWin64.exe); 1.7 = motor Waylith (RimWorld.exe)
$is17 = $Version -eq "1.7"
$exe = if ($is17) { "RimWorld.exe" } else { "RimWorldWin64.exe" }
$procNames = "RimWorld", "RimWorldWin64"

if (Get-Process $procNames -ErrorAction SilentlyContinue) {
    Write-Host "Ya hay un RimWorld abierto: no se lanza otro."; exit 2
}

# No competir por la GPU con ComfyUI generando (la PC se cuelga): esperar a que la GPU se libere
for ($w = 0; $w -lt 120; $w++) {
    $util = 0; try { $util = [int](& nvidia-smi --query-gpu=utilization.gpu --format=csv,noheader,nounits) } catch {}
    if ($util -lt 60) { break }
    if ($w -eq 0) { Write-Host "GPU ocupada ($util%): espero a que termine la generación de arte..." }
    Start-Sleep -Seconds 10
}

# Candado atómico: dos sesiones podían pasar a la vez el control de arriba (la espera de la GPU deja un hueco) y lanzar dos
# juegos que escriben en el mismo log. CreateNew falla si el archivo ya existe; un candado de más de 2 h se da por abandonado.
$lock = Join-Path $PSScriptRoot ".rimworld-lock"
try { $fs = [IO.File]::Open($lock, 'CreateNew', 'Write', 'None'); $fs.Close() }
catch {
    if ((Test-Path $lock) -and (Get-Item $lock).LastWriteTime -lt (Get-Date).AddHours(-2)) { Remove-Item $lock -Force; $fs = [IO.File]::Open($lock, 'CreateNew', 'Write', 'None'); $fs.Close() }
    else { Write-Host "Otra prueba está arrancando o corriendo (tools/.rimworld-lock): no se lanza otra."; exit 2 }
}
if (Get-Process $procNames -ErrorAction SilentlyContinue) {
    Remove-Item $lock -Force; Write-Host "Ya hay un RimWorld abierto: no se lanza otro."; exit 2
}

Add-Type -AssemblyName System.Windows.Forms
$second = if ($NoSecondMonitor) { $null } else { [System.Windows.Forms.Screen]::AllScreens | Where-Object { -not $_.Primary } | Select-Object -First 1 }
if ($second) {
    $Visible = $true
    # Ventana grande en el monitor secundario (deja un margen para la barra de título)
    if ($Width -eq 640) { $Width = [Math]::Min(1920, $second.Bounds.Width - 80); $Height = [Math]::Min(1080, $second.Bounds.Height - 120) }
}
$data = Join-Path $env:TEMP $(if ($is17) { "cvg_rwtest17" } else { "cvg_rwtest" })
$cfg = Join-Path $data "Config"
New-Item -ItemType Directory -Force $cfg | Out-Null
# 1.7 ignora -logFile: escribe siempre LocalLow\...\Logs\Player.log (y su tamaño no se actualiza mientras está abierto)
$log = if ($is17) { Join-Path $env:USERPROFILE "AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Logs\Player.log" } else { Join-Path $data "rw.log" }
function Read-Log {
    try { $fs = [IO.File]::Open($log, 'Open', 'Read', 'ReadWrite,Delete'); $sr = New-Object IO.StreamReader($fs); $s = $sr.ReadToEnd(); $sr.Close(); $s } catch { "" }
}

# ModsConfig aislado (solo los mods instalados se cargan; los que falten se ignoran)
$active = ($Mods.Split(",") | ForEach-Object { "    <li>$($_.Trim())</li>" }) -join "`n"
@"
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>$Version</version>
  <activeMods>
$active
  </activeMods>
</ModsConfigData>
"@ | Set-Content -Encoding UTF8 (Join-Path $cfg "ModsConfig.xml")

$lang = if ($Language -eq "English") { "English" } else { "Spanish (Español(Castellano))" }
# Prefs aislado: idioma (español por defecto), modo desarrollador, seguir corriendo minimizado
@"
<?xml version="1.0" encoding="utf-8"?>
<PrefsData>
  <langFolderName>$lang</langFolderName>
  <devMode>True</devMode>
  <runInBackground>True</runInBackground>
  <resetModsConfigOnCrash>False</resetModsConfigOnCrash>
  <screenWidth>$Width</screenWidth>
  <screenHeight>$Height</screenHeight>
  <fullscreen>False</fullscreen>
</PrefsData>
"@ | Set-Content -Encoding UTF8 (Join-Path $cfg "Prefs.xml")

Add-Type @"
using System; using System.Runtime.InteropServices;
public static class Win {
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
}
"@
$argLine = "$GameArgs -savedatafolder=`"$data`" -logFile `"$log`" -screen-fullscreen 0 -screen-width $Width -screen-height $Height"
# 1.7: arrancar minimizada crashea el motor (swapchain 0x0, "graphics memory allocation failed"): se abre normal y se minimiza a los 12 s
$style = if ($Visible -or $is17) { "Normal" } else { "Minimized" }
$minFrom = if ($is17) { 24 } else { 0 }
$p = Start-Process -FilePath (Join-Path $GameDir $exe) -ArgumentList $argLine -WorkingDirectory $GameDir -PassThru -WindowStyle $style
try { $p.PriorityClass = "BelowNormal" } catch {}  # que la PC del usuario siga fluida

# Minimizar sin activar (SW_SHOWMINNOACTIVE = 7) en cuanto aparezca la ventana, y repetir unos segundos por si el juego la restaura
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Milliseconds 500
    $p.Refresh()
    if (-not $Visible -and $i -ge $minFrom -and $p.MainWindowHandle -ne [IntPtr]::Zero) { [Win]::ShowWindow($p.MainWindowHandle, 7) | Out-Null }
    # Al monitor secundario, sin activar (SWP_NOSIZE 0x1 | SWP_NOZORDER 0x4 | SWP_NOACTIVATE 0x10); se repite por si el juego la recoloca
    if ($second -and $p.MainWindowHandle -ne [IntPtr]::Zero) { [Win]::SetWindowPos($p.MainWindowHandle, [IntPtr]::Zero, $second.Bounds.X + 40, $second.Bounds.Y + 40, 0, 0, 0x15) | Out-Null }
    Get-Process $procNames -ErrorAction SilentlyContinue | ForEach-Object { try { $_.PriorityClass = 'BelowNormal' } catch {} }
    if ($p.HasExited) { break }
}

# Esperar a que el autotest termine (línea final en el log) o al timeout
# Resultados esperados: uno por autotest -cvg...test pedido; -cvgselftest escribe cuatro, -cvgfactionstest y -cvgqitechtest uno cada uno
$expected = 0
foreach ($a in ($GameArgs -split "\s+")) { if ($a -eq "-cvgselftest") { $expected += 4 } elseif ($a -match "^-cvg\w*test$|^-cvgformflow$") { $expected += 1 } }
if ($expected -lt 1) { $expected = 1 }
$sw = [Diagnostics.Stopwatch]::StartNew()
$lastSize = -1; $idle = 0
while (-not $p.HasExited -and $sw.Elapsed.TotalSeconds -lt $TimeoutSec) {
    Start-Sleep -Seconds 5
    $txt = Read-Log
    if (-not $txt) { continue }
    if ($txt.Length -eq $lastSize) { $idle += 5 } else { $idle = 0; $lastSize = $txt.Length }
    # Hay varios autotests seguidos (cada uno termina en "ALL OK" o "FAILURES: n"):
    # se da por terminado cuando ya hubo al menos un resultado y el log deja de crecer 45 s.
    # Terminado cuando hay tantos resultados como pruebas pedidas (-cvgselftest da 4: Core, recursos, linajes, rasgos) y el log
    # dejó de crecer 45 s. Antes bastaba un resultado: una prueba que espera horas de juego (formaciones) quedaba cortada.
    $done = ([regex]::Matches($txt, "ALL OK|FAILURES:")).Count
    if ($idle -ge 45 -and $done -ge $expected) { break }
}
if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }
Remove-Item $lock -Force -ErrorAction SilentlyContinue
Write-Host "RimWorld $installed - Log: $log"
$txt = Read-Log
if ($txt) { $txt -split "`n" | Select-String -Pattern "ALL OK|FAILURES|Exception|\[Convergence\]" | Select-Object -Last 40 | ForEach-Object { $_.Line } }
