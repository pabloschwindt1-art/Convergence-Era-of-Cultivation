"""Auditoría funcional (solo lectura): cosas que no hacen lo que dicen o que chocan con vanilla. Escribe docs/AUDITORIA_FUNCIONAL.md.

Resuelve la herencia de defs (ParentName) contra los defs de vanilla de la copia 1.6 (C:\\Games\\RimWorld_1.6\\Data).
Uso: python tools/audit_funcional.py
"""
import glob
import os
import re
import xml.etree.ElementTree as ET
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODS = os.path.join(ROOT, 'Mods')
SRC = os.path.join(ROOT, 'Source')
VANILLA = r'C:\Games\RimWorld_1.6\Data'
SKIP = {'Convergence.Sects'}

findings = defaultdict(list)  # (gravedad, sección) -> [(mod, def, detalle)]


def add(sev, sec, mod, name, detail):
    findings[(sev, sec)].append((mod, name, detail))


def parse(p):
    try:
        return ET.parse(p).getroot()
    except ET.ParseError:
        return None


# ---------- Carga de defs (vanilla + nuestros) con herencia ----------
named = {}       # Name (abstractos y no) -> elemento
defs = {}        # (tag, defName) -> (elemento, mod o None)
by_name = {}     # defName -> (tag, elemento, mod)
ours = []        # (mod, archivo, elemento)
for f in glob.glob(os.path.join(VANILLA, '*', 'Defs', '**', '*.xml'), recursive=True):
    r = parse(f)
    if r is None:
        continue
    for el in r:
        if el.get('Name'):
            named.setdefault(el.get('Name'), el)
        n = el.findtext('defName')
        if n:
            defs[(el.tag, n)] = (el, None)
            by_name.setdefault(n, (el.tag, el, None))
mods = [m for m in sorted(os.listdir(MODS)) if os.path.isdir(os.path.join(MODS, m)) and m not in SKIP]
xml_text = {}    # archivo -> texto (para búsquedas de uso)
for mod in mods:
    for f in glob.glob(os.path.join(MODS, mod, '**', '*.xml'), recursive=True):
        try:
            xml_text[f] = open(f, encoding='utf-8-sig', errors='replace').read()
        except OSError:
            continue
        if os.sep + 'Languages' + os.sep in f:
            continue
        r = parse(f)
        if r is None or r.tag != 'Defs':
            continue
        for el in r:
            if el.get('Name'):
                named[el.get('Name')] = el
            n = el.findtext('defName')
            if n:
                defs[(el.tag, n)] = (el, mod)
                by_name[n] = (el.tag, el, mod)
                ours.append((mod, f, el))

cs_text = {}
for mod in os.listdir(SRC):
    for f in glob.glob(os.path.join(SRC, mod, '**', '*.cs'), recursive=True):
        if os.sep + 'obj' + os.sep in f or os.sep + 'bin' + os.sep in f:
            continue
        cs_text[f] = open(f, encoding='utf-8-sig', errors='replace').read()


def chain(el):
    """El def y sus padres (por ParentName)."""
    seen = 0
    while el is not None and seen < 20:
        yield el
        p = el.get('ParentName')
        el = named.get(p) if p else None
        seen += 1


def get(el, path):
    for e in chain(el):
        v = e.find(path)
        if v is not None:
            return v
    return None


def text(el, path):
    v = get(el, path)
    return v.text.strip() if v is not None and v.text else None


def comps(el):
    out = []
    for e in chain(el):
        c = e.find('comps')
        if c is not None:
            if c.get('Inherit') == 'False':
                out += list(c)
                break
            out += list(c)
    return out


def comp_classes(el):
    return {c.get('Class') or '' for c in comps(el)}


def exts(el):
    out = []
    for e in chain(el):
        m = e.find('modExtensions')
        if m is not None:
            out += [x.get('Class') or '' for x in m]
    return out


# ---------- Textos en español ----------
es_desc = defaultdict(set)  # idioma -> {defName con .description}
for f, t in xml_text.items():
    for L in ('Spanish', 'SpanishLatin'):
        if os.sep + L + os.sep in f and os.sep + 'DefInjected' + os.sep in f:
            for m in re.finditer(r'<([A-Za-z0-9_]+)\.(?:description|degreeDatas\.\d+\.description|stages\.\d+\.description)>', t):
                es_desc[L].add(m.group(1))

all_text = '\n'.join(t for f, t in xml_text.items() if os.sep + 'Languages' + os.sep not in f)
all_cs = '\n'.join(cs_text.values())


def used_elsewhere(name, own_el):
    """Cuántas veces aparece el defName fuera de su propio def (XML sin idiomas + C#)."""
    own = ET.tostring(own_el, encoding='unicode')
    n = len(re.findall(r'\b' + re.escape(name) + r'\b', all_text)) - len(re.findall(r'\b' + re.escape(name) + r'\b', own))
    n += len(re.findall(r'"' + re.escape(name) + r'"', all_cs))
    return n


# ---------- 1. Roles de ideología con umbrales que se pisan ----------
for mod, f, el in ours:
    if el.tag != 'PreceptDef':
        continue
    a, d = text(el, 'activationBelieverCount'), text(el, 'deactivationBelieverCount')
    if a is not None and d is not None and int(d) >= int(a) and int(a) > 0:
        add('ALTA', 'Roles que se activan y desactivan sin fin', mod, el.findtext('defName'), f'activación {a}, desactivación {d}')
for name, el in named.items():
    a, d = el.findtext('activationBelieverCount'), el.findtext('deactivationBelieverCount')
    if a and d and int(d) >= int(a) > 0 and el.get('Abstract') == 'True':
        add('ALTA', 'Roles que se activan y desactivan sin fin', '(base)', name, f'activación {a}, desactivación {d}')

# ---------- 2. Arte: vistas que faltan ----------
tex = set()
for f in glob.glob(os.path.join(MODS, '*', '**', 'Textures', '**', '*.png'), recursive=True):
    tex.add(f.split(os.sep + 'Textures' + os.sep, 1)[1].replace('\\', '/')[:-4])
for mod, f, el in ours:
    if el.tag != 'ThingDef' or el.get('Abstract') == 'True':
        continue
    gc, tp = text(el, 'graphicData/graphicClass'), text(el, 'graphicData/texPath')
    if not tp or 'CVG' not in tp:
        continue
    name = el.findtext('defName')
    if gc == 'Graphic_Multi':
        missing = [v for v in ('south', 'north') if tp + '_' + v not in tex]
        if tp + '_east' not in tex and tp + '_west' not in tex:
            missing.append('east/west')
        if missing:
            add('MEDIA', 'Gráficos de varias vistas a los que les faltan vistas', mod, name, f'{tp}: falta {", ".join(missing)}')
    if text(el, 'category') == 'Building' and gc == 'Graphic_Single':
        size = text(el, 'size') or '(1,1)'
        rot = text(el, 'rotatable')
        dr = text(el, 'graphicData/drawRotated')
        m = re.match(r'\((\d+),\s*(\d+)\)', size)
        if m and m.group(1) != m.group(2) and rot != 'false' and dr == 'false':
            add('MEDIA', 'Edificios rectangulares que se giran sin rotar el dibujo', mod, name, f'tamaño {size}, Graphic_Single con drawRotated false: al girarlo el dibujo no acompaña la huella')

# ---------- 3. Cosas inalcanzables ----------
produced = set()
for mod, f, el in ours:
    if el.tag == 'RecipeDef':
        for p in el.findall('products/*'):
            produced.add(p.tag)
for (tag, n), (el, mod) in list(defs.items()):
    if mod and tag == 'ThingDef' and get(el, 'recipeMaker') is not None:
        produced.add(n)
for mod, f, el in ours:
    if el.tag != 'ThingDef' or el.get('Abstract') == 'True' or text(el, 'category') != 'Item':
        continue
    name = el.findtext('defName')
    if name in produced or text(el, 'thingClass') in ('Corpse', 'MinifiedThing') or name.endswith('_Corpse'):
        continue
    if used_elsewhere(name, el) == 0 and not get(el, 'tradeTags') is not None:
        add('MEDIA', 'Objetos que no se consiguen de ninguna forma', mod, name, 'no se fabrica, no lo nombra ningún otro def (comercio, botín, evento) ni el código')
for mod, f, el in ours:
    if el.tag == 'RecipeDef' and el.get('Abstract') != 'True':
        users = [li.text for li in el.findall('recipeUsers/li')]
        name = el.findtext('defName')
        if not users and used_elsewhere(name, el) == 0:
            add('MEDIA', 'Recetas que no están en ninguna mesa', mod, name, 'sin recipeUsers y ninguna mesa la lista')
        for u in users:
            if u and u not in by_name:
                add('ALTA', 'Recetas que apuntan a una mesa que no existe', mod, name, u)
for mod, f, el in ours:
    if el.tag == 'ResearchProjectDef':
        name = el.findtext('defName')
        if used_elsewhere(name, el) == 0 and el.find('researchMods') is None:   # researchMods = hace algo al terminarla
            add('MEDIA', 'Investigaciones que no desbloquean nada', mod, name, 'ningún def ni código la pide')
        if text(el, 'techLevel') is None:
            add('BAJA', 'Investigaciones sin nivel tecnológico', mod, name, 'sin techLevel (el juego lo marca como error de config)')

# ---------- 4. Lore ----------
def sentences(s):
    return len([x for x in re.split(r'(?<=[.!?])\s+', s.strip()) if len(x) > 3])

LORE_TAGS = {'ThingDef', 'TraitDef', 'HediffDef', 'GeneDef', 'XenotypeDef', 'AbilityDef', 'ResearchProjectDef', 'PawnKindDef'}
for mod, f, el in ours:
    if el.tag not in LORE_TAGS or el.get('Abstract') == 'True':
        continue
    name = el.findtext('defName')
    if el.tag == 'ThingDef' and (text(el, 'category') in ('Ethereal', 'Projectile', 'Mote', 'Filth') or text(el, 'designationCategory') is None and text(el, 'category') == 'Building' and get(el, 'minifiedDef') is None and 'Mineable' not in name):
        # edificios no construibles (vetas, efectos) y proyectiles: sin regla de lore
        if text(el, 'category') != 'Building':
            continue
    descs = [el.findtext('description')] + [d.findtext('description') for d in el.findall('degreeDatas/li')]
    descs = [d for d in descs if d]
    if not descs:
        if el.tag in ('ThingDef', 'TraitDef', 'GeneDef', 'XenotypeDef', 'ResearchProjectDef') and text(el, 'category') not in ('Projectile', 'Mote', 'Ethereal', 'Filth'):
            add('BAJA', 'Sin descripción', mod, name, el.tag)
        continue
    if min(sentences(d) for d in descs) < 2:
        add('BAJA', 'Descripciones de menos de 2 frases (la regla pide lore de 2-3)', mod, name, f'{el.tag}: "{min(descs, key=len)[:110]}"')
    for L in ('Spanish', 'SpanishLatin'):
        if name not in es_desc[L]:
            add('BAJA', 'Descripción sin traducir', mod, name, f'{L} ({el.tag})')

# ---------- 5. Datos sospechosos ----------
for mod, f, el in ours:
    if el.tag != 'ThingDef' or el.get('Abstract') == 'True':
        continue
    name = el.findtext('defName')
    if text(el, 'category') == 'Item' and not name.endswith('_Corpse') and text(el, 'thingClass') != 'MinifiedThing':
        mv = text(el, 'statBases/MarketValue')
        if mv is not None and float(mv) == 0 and get(el, 'recipeMaker') is None:
            add('BAJA', 'Objetos con valor 0', mod, name, 'MarketValue 0')
        if text(el, 'statBases/Mass') is None:
            add('BAJA', 'Objetos sin masa', mod, name, 'sin Mass (vanilla usa 1 kg por defecto)')
for mod, f, el in ours:
    if el.tag in ('HediffDef', 'ThoughtDef') and el.get('Abstract') != 'True':
        name = el.findtext('defName')
        if el.tag == 'HediffDef' and not el.findtext('label'):
            add('BAJA', 'Condiciones sin etiqueta', mod, name, 'HediffDef sin label')
        if el.tag == 'ThoughtDef':
            stages = el.findall('stages/li')
            if stages and all(not s.findtext('label') for s in stages) and not el.findtext('label'):
                add('BAJA', 'Pensamientos sin etiqueta', mod, name, 'ninguna etapa tiene label')

# ---------- 6. La descripción promete algo que el def no hace ----------
for mod, f, el in ours:
    if el.tag != 'ThingDef' or el.get('Abstract') == 'True':
        continue
    name = el.findtext('defName')
    d = (el.findtext('description') or '')
    dl = d.lower()
    cc = comp_classes(el)
    ex = exts(el)
    if re.search(r'raises? (the )?qi concentration|qi concentration of the room', dl) and not any('FengShui' in x for x in ex) and not any('Qi' in c for c in cc):
        add('ALTA', 'Dice que sube la concentración de qi y no tiene con qué', mod, name, 'sin DefModExtension_FengShui ni comp de qi')
    # solo cuenta el qi (el campo element de FengShui no lo usa ningún código); qi 0 no sube nada
    fs_qi = 0.0
    for e in chain(el):
        for x in (e.find('modExtensions') or []):
            if 'FengShui' in (x.get('Class') or ''):
                try: fs_qi = max(fs_qi, float(x.findtext('qi') or 0))
                except ValueError: pass
    if fs_qi > 0 and 'qi' not in dl:
        add('BAJA', 'Sube el qi pero la descripción no lo dice', mod, name, f'DefModExtension_FengShui con qi {fs_qi:g}')
    if re.search(r'no electricity|needs no power|without power', dl) and any('PowerTrader' in c for c in cc):
        add('ALTA', 'Dice que no usa electricidad y consume energía', mod, name, 'tiene CompPowerTrader')
    m = re.search(r'level (\d) (alchemy furnace|spirit forge|furnace|forge)', dl)
    if m:
        req = None
        for e in chain(el):
            for x in (e.find('modExtensions') or []):
                if 'RequiresWorkbenchLevel' in (x.get('Class') or ''):
                    req = x.findtext('level')
        rm = get(el, 'recipeMaker')
        if req is None:
            # la extensión puede estar en la receta
            for rmod, rf, rel in ours:
                if rel.tag == 'RecipeDef' and rel.find('products/' + name) is not None:
                    for x in (rel.find('modExtensions') or []):
                        if 'RequiresWorkbenchLevel' in (x.get('Class') or ''):
                            req = x.findtext('level')
        if req != m.group(1):
            add('ALTA', 'El nivel de mesa que pide la descripción no coincide con el def', mod, name, f'dice nivel {m.group(1)}, el def pide {req or "ninguno"}')
    ing = get(el, 'ingestible')
    if ing is not None and text(el, 'category') == 'Item':
        doers = get(el, 'ingestible/outcomeDoers')
        hediff_on = re.search(r'\b(heals?|restores?|cures?|removes?)\b', dl)
        if hediff_on and (doers is None or len(list(doers)) == 0) and not get(el, 'comps') is not None:
            add('ALTA', 'Se come y dice que cura o repone, pero no tiene ningún efecto', mod, name, 'ingestible sin outcomeDoers')
    if text(el, 'category') == 'Building' and 'refuel' not in ''.join(cc).lower() and re.search(r'\bburns? (wood|spirit stones?|fuel)\b', dl) and 'stone' not in name.lower():
        add('MEDIA', 'Dice que quema combustible pero no tiene tanque', mod, name, 'sin CompRefuelable')
    if 'PlantGrower' in (text(el, 'thingClass') or ''):
        add('ALTA', 'Edificios que funcionan como maceta', mod, name, 'Building_PlantGrower: vanilla siembra encima de su dibujo')

# ---------- 7. Clases C# que ningún XML usa (funciones que nunca se activan) ----------
BASES = ('ThingComp', 'CompProperties', 'HediffComp', 'HediffCompProperties', 'DefModExtension', 'IngestionOutcomeDoer', 'StatPart',
         'ThoughtWorker', 'IncidentWorker', 'RoomRoleWorker', 'RecipeWorker', 'Verb', 'CompUseEffect', 'CompProperties_UseEffect',
         'JobDriver', 'WorkGiver', 'PlaceWorker', 'Hediff', 'Building', 'Building_WorkTable', 'SpecialThingFilterWorker', 'RoomStatWorker',
         'GenStep', 'QuestNode', 'SymbolResolver', 'StorytellerComp', 'RitualOutcomeComp', 'RitualObligationTrigger', 'ScenPart',
         'MentalState', 'InteractionWorker', 'ConditionalStatAffecter', 'CompAbilityEffect', 'Gene', 'PawnRenderNode', 'PawnRenderNodeWorker',
         'Projectile', 'Bullet', 'StockGenerator', 'ThingSetMaker', 'MapComponent', 'WorldComponent', 'GameComponent', 'Alert', 'MainTabWindow',
         'ITab', 'Designator', 'LordJob', 'LordToil', 'RoyalTitlePermitWorker', 'PreceptComp', 'RoleRequirement', 'Thought_Situational', 'Thought_Memory')
all_xml_lower = all_text
for f, t in cs_text.items():
    mod = os.path.relpath(f, SRC).split(os.sep)[0]
    for m in re.finditer(r'public\s+(?:abstract\s+)?class\s+(\w+)\s*:\s*([\w.]+)', t):
        cls, base = m.group(1), m.group(2).split('.')[-1]
        if 'abstract' in t[m.start():m.end()]:
            continue
        if not any(base == b or base.startswith(b) for b in BASES):
            continue
        if base.startswith(('MapComponent', 'WorldComponent', 'GameComponent', 'Alert', 'MainTabWindow', 'Designator', 'ITab')) or 'Test' in cls or 'Shot' in cls:
            continue  # se registran solos
        if re.search(r'\b' + cls + r'\b', all_xml_lower):
            continue
        # usado desde código: new X, GetModExtension<X>/GetComp<X>/<X>, o typeof(X) dentro de una clase que el XML nombra
        # (CompProperties_AbilityCone -> compClass = typeof(CompAbilityEffect_Cone)); StatParts y parches van por new X
        if re.search(r'new\s+' + cls + r'\b|<' + cls + r'>', all_cs):
            continue
        owner_named = False
        for t2 in cs_text.values():
            for tm in re.finditer(r'typeof\(' + cls + r'\)', t2):
                owners = re.findall(r'class\s+(\w+)', t2[:tm.start()])
                if owners and re.search(r'\b' + owners[-1] + r'\b', all_xml_lower):
                    owner_named = True
        if not owner_named:
            add('MEDIA', 'Código que ningún XML usa (la función nunca se activa)', mod, cls, f'clase {cls} : {base} en {os.path.relpath(f, ROOT)}')

# ---------- Informe ----------
order = {'ALTA': 0, 'MEDIA': 1, 'BAJA': 2}
out = ['# Auditoría funcional', '', 'Generado por `tools/audit_funcional.py` (solo lectura). Convergence.Sects no se audita.', '',
       '| Gravedad | Sección | Hallazgos |', '|---|---|---|']
keys = sorted(findings, key=lambda k: (order[k[0]], k[1]))
for k in keys:
    out.append(f'| {k[0]} | {k[1]} | {len(findings[k])} |')
for k in keys:
    out += ['', f'## [{k[0]}] {k[1]}', '']
    for mod, name, d in sorted(findings[k])[:300]:
        out.append(f'- **{mod}** `{name}` — {d}')
    if len(findings[k]) > 300:
        out.append(f'- … y {len(findings[k]) - 300} más')
p = os.path.join(ROOT, 'docs', 'AUDITORIA_FUNCIONAL.md')
open(p, 'w', encoding='utf-8').write('\n'.join(out) + '\n')
for k in keys:
    print(f'{k[0]:5} {len(findings[k]):4}  {k[1]}')
