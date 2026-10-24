#!/usr/bin/env python3
"""Índice de datos vanilla de RimWorld (Core y DLC) para consultar antes de implementar algo: "¿cómo lo hace vanilla?".

    python -I tools/vanilla-index.py                 # 1.7 de Steam (lo que juega el usuario)
    python -I tools/vanilla-index.py --data C:/Games/RimWorld_1.6/Data --out referencias/vanilla-1.6

Lee los Defs XML de cada DLC, resuelve la herencia (Name / ParentName / Abstract, como el juego: los hijos pisan al padre y las
listas se suman salvo Inherit="False"), toma la etiqueta en español de los .tar de idioma y escribe en referencias/vanilla/:
  INDEX.md            qué hay y cómo buscar
  defs/<Tipo>.xml     cada def YA RESUELTO (con lo heredado), uno por tipo: para grep del defName y ver todos sus campos
  labels.tsv          defName, tipo, DLC, etiqueta en inglés y en español (buscar por el nombre que usa el jugador)
  mechs.md, research.md, scenparts.md, buildings.md, apparel-stats.md: tablas listas
referencias/ no va a git: se regenera con este script cuando cambia la versión del juego.
"""
import argparse
import copy
import os
import re
import sys
import tarfile
import xml.etree.ElementTree as ET
from collections import defaultdict

ap = argparse.ArgumentParser()
ap.add_argument("--data", default=r"C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Data")
ap.add_argument("--out", default=os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "referencias", "vanilla"))
args = ap.parse_args()
sys.stdout.reconfigure(encoding="utf-8")

DLCS = ["Core", "Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey"]


def text(e, path, default=""):
    x = e.find(path)
    return (x.text or "").strip() if x is not None and x.text else default


# ---------- carga ----------
raw = []          # (dlc, element)
named = {}        # Name -> element (abstractos y concretos)
for dlc in DLCS:
    root = os.path.join(args.data, dlc, "Defs")
    for dp, _, fs in os.walk(root):
        for f in fs:
            if not f.endswith(".xml"):
                continue
            try:
                tree = ET.parse(os.path.join(dp, f))
            except ET.ParseError:
                continue
            for el in tree.getroot():
                if not isinstance(el.tag, str):
                    continue
                el.set("_dlc", dlc)
                el.set("_file", os.path.relpath(os.path.join(dp, f), args.data))
                raw.append((dlc, el))
                if el.get("Name"):
                    named[el.get("Name")] = el


def merge(parent, child):
    """Como XmlInheritance: los hijos pisan; las listas de <li> se suman salvo Inherit="False"."""
    out = copy.deepcopy(parent)
    for k, v in child.attrib.items():
        out.set(k, v)
    for c in child:
        if not isinstance(c.tag, str):
            continue
        if c.tag == "li":
            out.append(copy.deepcopy(c))
            continue
        existing = out.find(c.tag)
        if existing is None or c.get("Inherit") == "False" or len(c) == 0 or len(existing) == 0:
            if existing is not None:
                out.remove(existing)
            out.append(copy.deepcopy(c))
        else:
            out.remove(existing)
            out.append(merge(existing, c))
    if len(child) == 0 and child.text and child.text.strip():
        out.text = child.text
    return out


resolved_cache = {}


def resolve(el, depth=0):
    key = id(el)
    if key in resolved_cache:
        return resolved_cache[key]
    p = el.get("ParentName")
    if p and p in named and depth < 40:
        res = merge(resolve(named[p], depth + 1), el)
    else:
        res = copy.deepcopy(el)
    resolved_cache[key] = res
    return res


defs = defaultdict(list)   # tipo -> [(dlc, resolved)]
for dlc, el in raw:
    if el.get("Abstract") == "True":
        continue
    r = resolve(el)
    r.set("_dlc", dlc)
    if text(r, "defName"):
        defs[el.tag].append((dlc, r))

# cadena de padres (para ver de dónde hereda)
def parent_chain(el):
    out, p = [], el.get("ParentName")
    while p and p in named and len(out) < 40:
        out.append(p)
        p = named[p].get("ParentName")
    return out

parents_of = {}
for dlc, el in raw:
    d = text(el, "defName")
    if d:
        parents_of[(el.tag, d)] = parent_chain(el)

# ---------- etiquetas en español ----------
es_label = {}
for dlc in DLCS:
    langdir = os.path.join(args.data, dlc, "Languages")
    if not os.path.isdir(langdir):
        continue
    for f in os.listdir(langdir):
        if f.startswith("Spanish") and "Latin" not in f and f.endswith(".tar"):
            with tarfile.open(os.path.join(langdir, f)) as tar:
                for m in tar.getmembers():
                    if "DefInjected" not in m.name or not m.name.endswith(".xml"):
                        continue
                    try:
                        root = ET.fromstring(tar.extractfile(m).read())
                    except ET.ParseError:
                        continue
                    for e in root:
                        if e.tag.endswith(".label") and e.tag.count(".") == 1:
                            es_label[e.tag[:-6]] = (e.text or "").strip()

# ---------- salida ----------
os.makedirs(os.path.join(args.out, "defs"), exist_ok=True)
for tag, items in defs.items():
    root = ET.Element("Defs")
    for dlc, r in sorted(items, key=lambda x: text(x[1], "defName")):
        r = copy.deepcopy(r)
        pc = parents_of.get((tag, text(r, "defName")), [])
        r.set("_parents", ">".join(pc))
        root.append(r)
    ET.indent(root)
    ET.ElementTree(root).write(os.path.join(args.out, "defs", tag + ".xml"), encoding="utf-8", xml_declaration=True)

with open(os.path.join(args.out, "labels.tsv"), "w", encoding="utf-8") as f:
    f.write("defName\ttipo\tdlc\tlabel\tlabel_es\n")
    for tag, items in sorted(defs.items()):
        for dlc, r in items:
            d = text(r, "defName")
            f.write(f"{d}\t{tag}\t{dlc}\t{text(r, 'label')}\t{es_label.get(d, '')}\n")


def stat(r, s):
    return text(r, "statBases/" + s) or text(r, "race/" + s)


def md_table(path, title, header, rows, intro=""):
    with open(os.path.join(args.out, path), "w", encoding="utf-8") as f:
        f.write(f"# {title}\n\n{intro}\n\n| " + " | ".join(header) + " |\n|" + "---|" * len(header) + "\n")
        for row in rows:
            f.write("| " + " | ".join(str(c).replace("|", "/") for c in row) + " |\n")


things = {text(r, "defName"): (dlc, r) for dlc, r in defs["ThingDef"]}

# mecas: razas con mechWeightClass; banda desde statBases (BandwidthCost) del ThingDef
mech_rows = []
for d, (dlc, r) in sorted(things.items()):
    wc = text(r, "race/mechWeightClass")
    if not wc:
        continue
    works = ", ".join(li.text for li in r.findall("race/mechEnabledWorkTypes/li") if li.text)
    bw = text(r, "statBases/BandwidthCost") or "1"
    charger = "BasicRecharger (mech recharger)" if wc == "Light" else "StandardRecharger (large mech recharger)"
    mech_rows.append((d, text(r, "label"), es_label.get(d, ""), dlc, wc, bw, charger, works))
md_table("mechs.md", "Mecanoides (razas)", ["defName", "label", "es", "dlc", "peso", "banda", "recargador", "trabajos"], mech_rows,
         "Banda = statBases/BandwidthCost resuelto (1 si no lo define: el valor por defecto del stat). Light usa el recargador chico; "
         "Medium/Heavy/UltraHeavy el grande. PawnKind del meca = el mismo defName.")

res_rows = []
for dlc, r in sorted(defs["ResearchProjectDef"], key=lambda x: (x[1].findtext("techLevel") or "", text(x[1], "defName"))):
    d = text(r, "defName")
    pre = ", ".join(li.text for li in r.findall("prerequisites/li") if li.text)
    res_rows.append((d, text(r, "label"), es_label.get(d, ""), dlc, text(r, "techLevel"), text(r, "baseCost"),
                     pre, text(r, "requiredResearchBuilding"), text(r, "techprintCount")))
md_table("research.md", "Investigaciones", ["defName", "label", "es", "dlc", "nivel", "costo", "requisitos", "mesa", "techprints"], res_rows,
         "ScenPart_StartingResearch / FinishProject completan solos todos los requisitos previos.")

sp_rows = [(text(r, "defName"), text(r, "label"), text(r, "scenPartClass"), text(r, "category"), dlc) for dlc, r in defs["ScenPartDef"]]
md_table("scenparts.md", "Partes de escenario", ["def", "label", "clase", "categoría", "dlc"], sorted(sp_rows),
         "En un escenario: <li Class=\"<clase>\"><def>def</def>...</li>. Cosas iniciales minificables llegan desarmadas; "
         "los edificios no minificables solo con ScatterThings.")

b_rows = []
for d, (dlc, r) in sorted(things.items()):
    if text(r, "category") != "Building":
        continue
    cost = ", ".join(f"{c.tag} {c.text}" for c in (r.find("costList") or []))
    b_rows.append((d, text(r, "label"), es_label.get(d, ""), dlc, text(r, "size"), "sí" if text(r, "minifiedDef") else "no",
                   text(r, "costStuffCount"), cost, ", ".join(li.text for li in r.findall("researchPrerequisites/li") if li.text),
                   text(r, "designationCategory")))
md_table("buildings.md", "Edificios", ["defName", "label", "es", "dlc", "tamaño", "minificable", "stuff", "costo", "investigación", "menú"], b_rows)

a_rows = []
for d, (dlc, r) in sorted(things.items()):
    if r.find("apparel") is None:
        continue
    offs = ", ".join(f"{c.tag} {c.text}" for c in (r.find("equippedStatOffsets") or []))
    layers = ", ".join(li.text for li in r.findall("apparel/layers/li") if li.text)
    a_rows.append((d, text(r, "label"), es_label.get(d, ""), dlc, layers, stat(r, "ArmorRating_Sharp"), stat(r, "ArmorRating_Blunt"), offs))
md_table("apparel-stats.md", "Ropa y equipo", ["defName", "label", "es", "dlc", "capas", "armor sharp", "armor blunt", "equippedStatOffsets"], a_rows)

counts = sorted(((t, len(i)) for t, i in defs.items()), key=lambda x: -x[1])
with open(os.path.join(args.out, "INDEX.md"), "w", encoding="utf-8") as f:
    f.write(f"""# Datos vanilla de RimWorld (Core + DLC)

Generado por `tools/vanilla-index.py` desde `{args.data}`. Regenerar cuando cambie la versión del juego.
**Antes de implementar algo, mirar acá cómo lo hace vanilla** (y la wiki resumida en `referencias/wiki/INDEX.md`).

## Cómo buscar
- Por el nombre que usa el jugador (español o inglés): `grep -i "recargador" referencias/vanilla/labels.tsv`
- Todos los campos de un def, con lo heredado ya resuelto: `grep -n -A60 "<defName>StandardRecharger</defName>" referencias/vanilla/defs/ThingDef.xml`
  (atributo `_parents` = cadena de padres; `_dlc` = de qué DLC viene; `_file` = archivo original)
- Tablas: `mechs.md` (banda, peso, recargador, trabajos), `research.md` (requisitos, costo), `scenparts.md` (partes de escenario),
  `buildings.md` (tamaño, minificable, costo), `apparel-stats.md` (capas, armadura, bonos como MechBandwidth)
- Código: `ilspycmd -t <Clase> "<RimWorld>/Assembly-CSharp.dll"` (1.7) para ver la lógica exacta.

## Defs por tipo
""" + "\n".join(f"- `defs/{t}.xml`: {n}" for t, n in counts) + "\n")
print(f"{sum(n for _, n in counts)} defs de {len(counts)} tipos -> {args.out}")
