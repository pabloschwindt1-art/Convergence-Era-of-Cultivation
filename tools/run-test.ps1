# Lanza una instancia de prueba AISLADA de RimWorld sin molestar al usuario:
#  - carpeta de datos propia (ModsConfig y Prefs aislados: español, modo desarrollador, corre en segundo plano)
#  - ventana pequeña que se minimiza enseguida sin robar el foco
#  - log propio; espera a que termine (o al timeout) y lo cierra
#
#   powershell -ExecutionPolicy Bypass -File tools\run-test.ps1 [-GameArgs "-quicktest -cvgselftest"] [-TimeoutSec 900] [-Mods "a,b,c"]
param(
    [string]$GameArgs = "-quicktest -cvgselftest",
    [int]$TimeoutSec = 900,
    [string]$Mods = "brrainz.harmony,ludeon.rimworld,ludeon.rimworld.royalty,ludeon.rimworld.ideology,ludeon.rimworld.biotech,ludeon.rimworld.anomaly,ludeon.rimworld.odyssey,prospero1088.convergence.core,prospero1088.convergence.resources,prospero1088.convergence.formations,prospero1088.convergence.races,prospero1088.convergence.traitsdestinies,prospero1088.convergence.sects,prospero1088.convergence.alchemy,prospero1088.convergence.apparel,prospero1088.convergence.faunaflora,prospero1088.convergence.stories,prospero1088.convergence.grafts,prospero1088.convergence.qitech,prospero1088.convergence.furniture,prospero1088.convergence.biotechwuxia,prospero1088.convergence.space",
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld",
    # Tamaño de la ventana de prueba (pequeña por defecto; más grande solo para capturas de interfaz, p. ej. -cvgdevshot)
    [switch]$Visible,   # ventana normal (no minimizada) para capturas de pantalla (-cvgshowcase)
    [int]$Width = 640,
    [int]$Height = 360,
    # Idioma del juego de prueba: Spanish (por defecto) o English (capturas para Workshop)
    [ValidateSet("Spanish", "English")][string]$Language = "Spanish"
)

# Pausa pedida por el usuario (está usando Steam/el juego): tools/.pausa-rimworld existe => no lanzar
$pausa = Join-Path $PSScriptRoot ".pausa-rimworld"
while (Test-Path $pausa) { Write-Host "Pausa del usuario: espero..."; Start-Sleep -Seconds 20 }

if (Get-Process RimWorldWin64 -ErrorAction SilentlyContinue) {
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
if (Get-Process RimWorldWin64 -ErrorAction SilentlyContinue) {
    Remove-Item $lock -Force; Write-Host "Ya hay un RimWorld abierto: no se lanza otro."; exit 2
}

$data = Join-Path $env:TEMP "cvg_rwtest"
$cfg = Join-Path $data "Config"
New-Item -ItemType Directory -Force $cfg | Out-Null
$log = Join-Path $data "rw.log"

# ModsConfig aislado (solo los mods instalados se cargan; los que falten se ignoran)
$active = ($Mods.Split(",") | ForEach-Object { "    <li>$($_.Trim())</li>" }) -join "`n"
@"
<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>1.6</version>
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
public static class Win { [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd); }
"@
$argLine = "$GameArgs -savedatafolder=`"$data`" -logFile `"$log`" -screen-fullscreen 0 -screen-width $Width -screen-height $Height"
$p = Start-Process -FilePath (Join-Path $GameDir "RimWorldWin64.exe") -ArgumentList $argLine -WorkingDirectory $GameDir -PassThru -WindowStyle $(if ($Visible) { "Normal" } else { "Minimized" })
try { $p.PriorityClass = "BelowNormal" } catch {}  # que la PC del usuario siga fluida

# Minimizar sin activar (SW_SHOWMINNOACTIVE = 7) en cuanto aparezca la ventana, y repetir unos segundos por si Unity la restaura
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Milliseconds 500
    $p.Refresh()
    if (-not $Visible -and $p.MainWindowHandle -ne [IntPtr]::Zero) { [Win]::ShowWindow($p.MainWindowHandle, 7) | Out-Null }
    Get-Process RimWorldWin64 -ErrorAction SilentlyContinue | ForEach-Object { try { $_.PriorityClass = 'BelowNormal' } catch {} }
    if ($p.HasExited) { break }
}

# Esperar a que el autotest termine (línea final en el log) o al timeout
$sw = [Diagnostics.Stopwatch]::StartNew()
$lastSize = -1; $idle = 0
while (-not $p.HasExited -and $sw.Elapsed.TotalSeconds -lt $TimeoutSec) {
    Start-Sleep -Seconds 5
    if (-not (Test-Path $log)) { continue }
    $size = (Get-Item $log).Length
    if ($size -eq $lastSize) { $idle += 5 } else { $idle = 0; $lastSize = $size }
    # Hay varios autotests seguidos (cada uno termina en "ALL OK" o "FAILURES: n"):
    # se da por terminado cuando ya hubo al menos un resultado y el log deja de crecer 45 s.
    if ($idle -ge 45 -and (Select-String -Path $log -Pattern "ALL OK|FAILURES:" -Quiet)) { break }
}
if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }
Remove-Item $lock -Force -ErrorAction SilentlyContinue
Write-Host "Log: $log"
if (Test-Path $log) { Select-String -Path $log -Pattern "ALL OK|FAILURES|Exception|\[Convergence\]" | Select-Object -Last 40 | ForEach-Object { $_.Line } }
