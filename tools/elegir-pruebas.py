#!/usr/bin/env python3
"""Elige qué autotests correr según lo que cambió (criterio de la skill rimworld-mod-tester).

    python -I tools/elegir-pruebas.py              # cambios sin commitear + último commit
    python -I tools/elegir-pruebas.py --desde 97be234   # todo lo cambiado desde ese commit
    python -I tools/elegir-pruebas.py archivo1 archivo2 # archivos a mano

Imprime: qué autotests corresponden (y por qué), cuánto tardan, cuáles son lentos (piden permiso al usuario) y el comando
listo para tools/run-test.ps1. No lanza nada.
"""
import re
import subprocess
import sys

# Duración aproximada en minutos de UNA corrida en 1.7 con los 15 mods (incluye ~40 s de carga). Medido 2026-10-23.
DURACION = {
    "-cvgselftest": 1.5, "-cvgstoriestest": 0.5, "-cvgformationstest": 1.5, "-cvgformflow": 3, "-cvggraftstest": 1.5,
    "-cvgfactionstest": 1.5, "-cvgfavorstest": 1, "-cvgqitechtest": 1.5, "-cvgqinettest": 0.5, "-cvgqitechwartest": 3,
    "-cvgfurnituretest": 1.5, "-cvgcropstest": 0.5, "-cvgislandtest": 1.5, "-cvgflighttest": 4, "-cvgislandspacetest": 5,
    "-cvgbandtotemtest": 1.5, "-cvgflowtest": 3, "-cvgmechtabtest": 1.5, "-cvgrootflow": 3,
    # LENTAS: solo si cambió ese código y con permiso del usuario
    "-cvgworktest": 6, "-cvgchargetest": 8, "-cvgpuppetfighttest": 25, "-cvgplaytest": 30,
}
LENTAS = {"-cvgworktest", "-cvgchargetest", "-cvgpuppetfighttest", "-cvgplaytest"}

# (patrón sobre la ruta, autotests, motivo). Se aplican TODAS las reglas que coinciden.
REGLAS = [
    # Solo textos / documentación / arte: no hace falta abrir el juego
    (r"^docs/|^art/|\.md$|^tools/(?!run-test)", [], "documentación, arte fuente o herramientas: sin juego"),
    (r"/Languages/", ["SOLO_TEXTO"], "traducciones: alcanza con cargar (el Core self-test revisa las traducciones)"),
    (r"/Textures/", ["SOLO_ARTE"], "texturas: sin autotest; si hace falta verlas, captura en 1.6 (-cvgbenchshot=CVG_X)"),
    (r"/About/", [], "About.xml: tools/check-workshop.py, sin juego"),
    # Código y defs por módulo
    (r"^Source/Convergence\.DevTools/CoreSelfTest", ["-cvgselftest"], "el propio Core self-test"),
    (r"^Source/Convergence\.Core/Cultivation/(RootSurgery|SealedRoot|SpiritRoot|RootPurification)|^Source/Convergence\.DevTools/RootFlowTest|SpiritRoots\.xml|Ritual_RootPurification|QiTech_RootEngineering",
     ["-cvgselftest", "-cvgrootflow"], "raíces que cambian: cirugías en altar y mesa de qi, ritual"),
    (r"^Source/Convergence\.Core/QiNetwork/|^Mods/Convergence\.QiTech/.*Network", ["-cvgqinettest"], "red de qi"),
    (r"^Source/Convergence\.Core/Space/|^Mods/Convergence\.Space/", ["-cvgislandtest"], "isla voladora"),
    (r"^Source/Convergence\.Core/Biotech/BandTotem", ["-cvgbandtotemtest"], "tótem de hilos"),
    (r"^Source/Convergence\.Core/Biotech/Puppet|^Source/Convergence\.Stories/(ScenPuppets|WorkFlowTest)|^Mods/Convergence\.BiotechWuxia/.*Puppet(?!Weapons)",
     ["-cvgworktest"], "títeres (LENTA: preguntar; peleas solo si cambió el balance de combate)"),
    (r"^Source/Convergence\.Stories/PuppetFightTest", ["-cvgpuppetfighttest"], "prueba de peleas de títeres (LENTA)"),
    (r"^Source/Convergence\.Stories/ChargeFlowTest|^Source/Convergence\.Core/Biotech/.*Charg", ["-cvgchargetest"], "recarga de títeres (LENTA)"),
    (r"^Source/Convergence\.Core/Cultivation/BreakthroughUtility|^Source/Convergence\.Core/Tribulation/", ["-cvgselftest", "-cvgflowtest"], "avance de reino"),
    (r"^Source/Convergence\.Core/Debug/FurnitureSelfTest|^Mods/Convergence\.Furniture/", ["-cvgfurnituretest", "-cvgselftest"], "muebles (y salas de trabajo del Core)"),
    (r"^Source/Convergence\.Core/Debug/CropsDefense|^Mods/Convergence\.FaunaFlora/", ["-cvgcropstest", "-cvgselftest"], "fauna y flora"),
    (r"^Source/Convergence\.Core/Ideology/SectIdeo", [], "test del viejo Sects (retirado): no se corre"),
    (r"^Source/Convergence\.Core/|^Mods/Convergence\.Core/", ["-cvgselftest"], "Core"),
    (r"^Mods/Convergence\.(Alchemy|Apparel|Races|Resources|TraitsDestinies)/", ["-cvgselftest"],
     "módulo solo XML: el Core self-test carga recetas, ingeribles, pawns, objetos y salas de trabajo"),
    (r"^Mods/Convergence\.BiotechWuxia/(?!.*Puppet(?!Weapons))", ["-cvgselftest"], "Biotech Wuxia (sin títeres)"),
    (r"^Source/Convergence\.Stories/(?!PuppetFightTest|ChargeFlowTest|WorkFlowTest|ScenPuppets|MechTabTest)|^Mods/Convergence\.Stories/",
     ["-cvgstoriestest"], "historias, escenarios y reinos secretos"),
    (r"^Source/Convergence\.Stories/MechTabTest", ["-cvgmechtabtest"], "botón Mecas"),
    (r"^Source/Convergence\.Formations/|^Mods/Convergence\.Formations/", ["-cvgformationstest"], "formaciones"),
    (r"^Source/Convergence\.Formations/(FormationFlowTest|.*Turret)", ["-cvgformflow"], "formaciones y torretas en combate"),
    (r"^Source/Convergence\.Grafts/|^Mods/Convergence\.Grafts/", ["-cvggraftstest"], "injertos"),
    (r"^Source/Convergence\.Factions/|^Mods/Convergence\.Factions/", ["-cvgfactionstest", "-cvgfavorstest"], "facciones y favores"),
    (r"^Source/Convergence\.QiTech/|^Mods/Convergence\.QiTech/", ["-cvgqitechtest"], "Qi-Tech"),
    (r"^Source/Convergence\.QiTech/QiTechMilitary|^Mods/Convergence\.QiTech/.*(Weapon|Turret)", ["-cvgqitechwartest"], "armas y torretas de Qi-Tech"),
    (r"^Mods/Convergence\.Sects/", [], "Sects viejo (retirado): no se prueba"),
    (r"^tools/run-test\.ps1$", ["-cvgformationstest"], "lanzador de pruebas: una corrida corta para verlo andar"),
]


def changed_files(desde):
    def git(*a):
        return subprocess.run(["git", *a], capture_output=True, text=True, encoding="utf-8").stdout.split("\n")
    files = set()
    if desde:
        files.update(git("diff", "--name-only", desde))
    else:
        files.update(git("diff", "--name-only", "HEAD"))
        files.update(git("diff", "--name-only", "HEAD~1", "HEAD"))
    files.update(f for f in git("ls-files", "--others", "--exclude-standard") if f.startswith(("Source/", "Mods/")))
    return sorted(f.strip() for f in files if f.strip() and "/obj/" not in f and "/Assemblies/" not in f)


TEXT_LINE = re.compile(r'^\s*(<(description|label|labelNoun|jobString|reportString|discoveryText|[\w.]+\.(description|label|labelNoun|'
                       r'jobString|reportString|discoveryText))>.*|<!--.*|)\s*$')


def text_only(f, desde):
    """True si en el XML solo cambiaron textos (descripciones, labels, comentarios): eso no cambia el comportamiento."""
    if not f.endswith('.xml'):
        return False
    rev = [desde] if desde else ['HEAD']
    out = subprocess.run(["git", "diff", "-U0", *rev, "--", f], capture_output=True, text=True, encoding="utf-8").stdout
    lines = [l[1:] for l in out.split('\n') if l[:1] in '+-' and not l.startswith(('+++', '---'))]
    return bool(lines) and all(TEXT_LINE.match(l) for l in lines)


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    args = sys.argv[1:]
    desde = None
    if args[:1] == ["--desde"]:
        desde, args = args[1], args[2:]
    files = args or changed_files(desde)
    if not files:
        print("Sin cambios: no hay nada que probar.")
        return
    tests, motivos, texto, arte = [], {}, False, False
    for f in files:
        f = f.replace("\\", "/")
        if text_only(f, desde):
            texto = True
            continue
        hit = False
        for pat, ts, why in REGLAS:
            if re.search(pat, f):
                hit = True
                for t in ts:
                    if t == "SOLO_TEXTO": texto = True; continue
                    if t == "SOLO_ARTE": arte = True; continue
                    if t not in tests: tests.append(t)
                    motivos.setdefault(t, set()).add(why)
                # las reglas sin autotest (docs, arte, About) cortan: no siguen a la regla general del módulo
                if not ts or ts == ["SOLO_TEXTO"] or ts == ["SOLO_ARTE"]:
                    break
        if not hit:
            print("  ? sin regla:", f, "(agregar una a REGLAS en tools/elegir-pruebas.py)")
    if texto and not tests:
        tests.append("-cvgselftest"); motivos["-cvgselftest"] = {"solo traducciones: una carga con el Core self-test"}
    rapidas = [t for t in tests if t not in LENTAS]
    lentas = [t for t in tests if t in LENTAS]
    print(f"Archivos cambiados: {len(files)}")
    if not tests:
        print("No hace falta abrir el juego." + (" (Texturas: captura en 1.6 solo si el usuario quiere verlas.)" if arte else ""))
        return
    for t in rapidas + lentas:
        print(f"  {t:22} ~{DURACION.get(t, 2):g} min  {'LENTA ' if t in LENTAS else ''}<- {', '.join(sorted(motivos[t]))}")
    if rapidas:
        # Una partida por prueba: varias en la misma partida se pisan (las historias disparan incursiones, visitas y diálogos; las
        # sobras de una caen en el área de otra). Solo -cvgselftest y -cvgstoriestest se verificaron juntas.
        grupos, juntas = [], [t for t in rapidas if t in ("-cvgselftest", "-cvgstoriestest")]
        if juntas: grupos.append(juntas)
        grupos += [[t] for t in rapidas if t not in juntas]
        total = sum(sum(DURACION.get(t, 2) for t in g) for g in grupos)
        print(f"\n{len(grupos)} corrida(s), una detrás de otra (~{total:.0f} min en total):")
        for g in grupos:
            print(f'  powershell -ExecutionPolicy Bypass -File tools\\run-test.ps1 -Version 1.7 -GameArgs "-quicktest {" ".join(g)}" -Mods todos')
    if lentas:
        print(f"\nLENTAS ({', '.join(lentas)}, ~{sum(DURACION[t] for t in lentas):g} min): NO correrlas sin preguntarle al usuario.")
    if arte:
        print("Texturas cambiadas: sin autotest (captura en 1.6 si el usuario quiere verlas).")


if __name__ == "__main__":
    main()
