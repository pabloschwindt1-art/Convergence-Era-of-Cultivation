using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

// Validador de Defs de Convergence contra los tipos reales de RimWorld (reflexión, sin arrancar el juego).
// Comprueba en NUESTROS XML: que cada etiqueta sea un campo del tipo, que Class= exista y herede del tipo del campo,
// enums válidos, números/booleanos parseables, Type (compClass, workerClass…) existentes, compClass no nulo en
// CompProperties/HediffCompProperties/AbilityCompProperties, referencias a Defs existentes (y aviso si la Def solo
// existe en una DLC fuera de una carpeta condicional), DefInjected con defName/campo existentes y Keyed completos
// en los tres idiomas, más las claves "CVG_*".Translate() del código.
//
// Uso:  DefValidator.exe <carpeta RimWorld> <carpeta repo>
// Ejemplo: tools/DefValidator/bin/DefValidator.exe "C:/Program Files (x86)/Steam/steamapps/common/RimWorld" C:/MODS_Rimmworld
internal static class Program
{
    private static readonly List<string> errors = new List<string>();
    private static readonly List<string> warnings = new List<string>();
    private static readonly Dictionary<string, Type> typesByFullName = new Dictionary<string, Type>();
    private static readonly Dictionary<string, List<Type>> typesByName = new Dictionary<string, List<Type>>();
    // Tipo de Def → defNames (de vanilla, DLC y nuestros mods). Origen por defName para avisar de DLC.
    private static readonly Dictionary<Type, Dictionary<string, string>> defs = new Dictionary<Type, Dictionary<string, string>>();
    private static Type defType, compPropsType, hediffCompPropsType, abilityCompPropsType, simpleCurveType, statModifierType;
    private static string[] managedDirs;

    private static readonly string[] SearchNamespaces = { "", "Verse.", "RimWorld.", "RimWorld.Planet.", "Verse.AI.", "Verse.Sound.", "LudeonTK.", "Convergence." };

    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Uso: DefValidator <carpeta RimWorld> <carpeta repo>");
            return 2;
        }
        string game = args[0];
        string repo = args[1];
        string managed = Path.Combine(game, "RimWorldWin64_Data", "Managed");
        string harmony = Directory.GetDirectories(Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") ?? "", ".nuget", "packages", "lib.harmony"))
            .Select(d => Path.Combine(d, "lib", "net472")).FirstOrDefault(Directory.Exists);
        managedDirs = new[] { managed, harmony }.Where(d => d != null).ToArray();
        AppDomain.CurrentDomain.AssemblyResolve += Resolve;

        var assemblies = new List<Assembly> { Assembly.LoadFrom(Path.Combine(managed, "Assembly-CSharp.dll")) };
        foreach (var dll in Directory.GetFiles(Path.Combine(repo, "Mods"), "*.dll", SearchOption.AllDirectories))
            if (dll.Contains(Path.DirectorySeparatorChar + "Assemblies" + Path.DirectorySeparatorChar)) assemblies.Add(Assembly.LoadFrom(dll));
        foreach (var asm in assemblies) IndexTypes(asm);

        defType = T("Verse.Def");
        compPropsType = T("Verse.CompProperties");
        hediffCompPropsType = T("Verse.HediffCompProperties");
        abilityCompPropsType = T("Verse.AbilityCompProperties");
        simpleCurveType = T("Verse.SimpleCurve");
        statModifierType = T("RimWorld.StatModifier");

        // Defs del juego (Core y DLC) y de nuestros mods.
        string data = Path.Combine(game, "Data");
        foreach (var dir in Directory.GetDirectories(data))
        {
            string source = Path.GetFileName(dir);
            string defsDir = Path.Combine(dir, "Defs");
            if (Directory.Exists(defsDir)) IndexDefs(defsDir, source);
        }
        var ourFiles = new List<string>();
        foreach (var mod in Directory.GetDirectories(Path.Combine(repo, "Mods")))
            foreach (var file in Directory.GetFiles(mod, "*.xml", SearchOption.AllDirectories))
                if (IsDefsFile(file))
                {
                    ourFiles.Add(file);
                    IndexDefsFile(file, "ours");
                }

        foreach (var file in ourFiles) ValidateDefsFile(file, repo);
        foreach (var mod in Directory.GetDirectories(Path.Combine(repo, "Mods"))) ValidateLanguages(mod, repo);
        ValidateCodeKeys(repo);

        foreach (var w in warnings) Console.WriteLine("WARN  " + w);
        foreach (var e in errors) Console.WriteLine("ERROR " + e);
        Console.WriteLine($"{ourFiles.Count} Defs files, {errors.Count} error(s), {warnings.Count} warning(s).");
        return errors.Count == 0 ? 0 : 1;
    }

    private static Assembly Resolve(object sender, ResolveEventArgs e)
    {
        string name = new AssemblyName(e.Name).Name + ".dll";
        foreach (var dir in managedDirs)
        {
            string path = Path.Combine(dir, name);
            if (File.Exists(path)) return Assembly.LoadFrom(path);
        }
        return null;
    }

    private static void IndexTypes(Assembly asm)
    {
        Type[] types;
        try { types = asm.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
        foreach (var t in types)
        {
            string full = t.FullName?.Replace('+', '.');
            if (full == null) continue;
            typesByFullName[full] = t;
            if (!typesByName.TryGetValue(t.Name, out var list)) typesByName[t.Name] = list = new List<Type>();
            list.Add(t);
        }
    }

    private static Type T(string fullName) => typesByFullName.TryGetValue(fullName, out var t) ? t : null;

    /// <summary>Como GenTypes.GetTypeInAnyAssembly: nombre completo o corto en los espacios de nombres del juego.</summary>
    private static Type ResolveType(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (var ns in SearchNamespaces)
            if (typesByFullName.TryGetValue(ns + name, out var t)) return t;
        return null;
    }

    private static bool IsDefsFile(string file)
    {
        string norm = file.Replace('\\', '/');
        return norm.Contains("/Defs/") && !norm.Contains("/Languages/");
    }

    private static void IndexDefs(string dir, string source)
    {
        foreach (var file in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) IndexDefsFile(file, source);
    }

    private static void IndexDefsFile(string file, string source)
    {
        XmlDocument doc;
        try
        {
            doc = new XmlDocument();
            doc.Load(file);
        }
        catch (Exception ex)
        {
            if (source == "ours") errors.Add($"{file}: XML no parsea: {ex.Message}");
            return;
        }
        if (doc.DocumentElement == null) return;
        foreach (XmlNode n in doc.DocumentElement.ChildNodes)
        {
            if (!(n is XmlElement el)) continue;
            var t = ResolveType(el.Name);
            if (t == null) continue;
            string defName = el["defName"]?.InnerText.Trim();
            if (string.IsNullOrEmpty(defName)) continue;
            if (!defs.TryGetValue(t, out var set)) defs[t] = set = new Dictionary<string, string>();
            if (!set.ContainsKey(defName)) set[defName] = source;
        }
    }

    /// <summary>Origen de la Def (Core, Royalty…, ours) buscando en el tipo y sus subclases; null si no existe.</summary>
    private static string DefSource(Type type, string defName)
    {
        foreach (var kv in defs)
            if (type.IsAssignableFrom(kv.Key) && kv.Value.TryGetValue(defName, out var src)) return src;
        return null;
    }

    private static void ValidateDefsFile(string file, string repo)
    {
        var doc = new XmlDocument();
        try { doc.Load(file); }
        catch { return; }
        string rel = file.Substring(repo.Length).TrimStart('\\', '/').Replace('\\', '/');
        // Las carpetas Mods/<DLC>/ solo cargan con la DLC: ahí se pueden usar sus Defs. Un mod que depende de una DLC
        // (About.xml, modDependencies) también puede usarla en todas sus carpetas.
        bool conditional = Regex.IsMatch(rel, @"/Mods/[^/]+/Defs/");
        var modMatch = Regex.Match(rel, @"^Mods/([^/]+)/");
        string dlcDeps = "";
        if (modMatch.Success)
        {
            string about = Path.Combine(repo, "Mods", modMatch.Groups[1].Value, "About", "About.xml");
            if (File.Exists(about)) dlcDeps = File.ReadAllText(about).ToLowerInvariant();
        }
        if (doc.DocumentElement == null || doc.DocumentElement.Name != "Defs")
        {
            errors.Add($"{rel}: la raíz debe ser <Defs>");
            return;
        }
        foreach (XmlNode n in doc.DocumentElement.ChildNodes)
        {
            if (!(n is XmlElement el)) continue;
            var t = ResolveType(el.Name);
            if (t == null || !defType.IsAssignableFrom(t))
            {
                errors.Add($"{rel}: <{el.Name}> no es un tipo de Def");
                continue;
            }
            string id = el["defName"]?.InnerText ?? el.GetAttribute("Name");
            var ctx = new Ctx { file = rel, conditional = conditional, dependencies = dlcDeps };
            ValidateObject(el, t, $"{el.Name}[{id}]", ctx);
        }
    }

    private sealed class Ctx
    {
        public string file;
        public bool conditional;
        public string dependencies = "";
    }

    private static FieldInfo FindField(Type t, string name)
    {
        for (var cur = t; cur != null; cur = cur.BaseType)
        {
            var f = cur.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (f != null) return f;
            foreach (var g in cur.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                foreach (var attr in g.GetCustomAttributesData())
                    if (attr.AttributeType.Name == "LoadAliasAttribute" && attr.ConstructorArguments.Count > 0 &&
                        (string)attr.ConstructorArguments[0].Value == name) return g;
        }
        return null;
    }

    private static bool HasCustomLoader(Type t) =>
        t.GetMethod("LoadDataFromXmlCustom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;

    /// <summary>Tipos que el juego lee de un texto (ParseHelper) o con cargador propio.</summary>
    private static bool IsTextParsed(Type t)
    {
        if (t.IsPrimitive || t == typeof(string) || t.IsEnum || t == typeof(Type)) return true;
        if (HasCustomLoader(t)) return true;
        if (t.GetMethod("FromString", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null) != null) return true;
        switch (t.Name)
        {
            case "IntRange": case "FloatRange": case "Vector2": case "Vector3": case "Vector4": case "Color": case "Color32":
            case "ColorInt": case "IntVec2": case "IntVec3": case "Rot4": case "Rect": case "CurvePoint": case "QualityRange": case "NameTriple":
            case "TaggedString": case "Nullable`1": case "Gradient":
                return true;
        }
        return false;
    }

    private static void ValidateObject(XmlElement el, Type t, string path, Ctx ctx)
    {
        if (HasCustomLoader(t)) return;
        if (t == simpleCurveType)
        {
            if (el["points"] == null) Err(ctx, $"{path}: SimpleCurve sin <points>");
            return;
        }
        foreach (XmlNode n in el.ChildNodes)
        {
            if (!(n is XmlElement c)) continue;
            var f = FindField(t, c.Name);
            if (f == null)
            {
                Err(ctx, $"{path}: <{c.Name}> no es un campo de {t.FullName}");
                continue;
            }
            ValidateValue(c, f.FieldType, path + "." + c.Name, ctx);
        }
        CheckCompClass(el, t, path, ctx);
    }

    private static void ValidateValue(XmlElement el, Type ft, string path, Ctx ctx)
    {
        if (el.GetAttribute("IsNull").Equals("true", StringComparison.OrdinalIgnoreCase)) return;
        string cls = el.GetAttribute("Class");
        if (!string.IsNullOrEmpty(cls))
        {
            var ct = ResolveType(cls);
            if (ct == null)
            {
                Err(ctx, $"{path}: Class=\"{cls}\" no existe");
                return;
            }
            if (!ft.IsAssignableFrom(ct))
            {
                Err(ctx, $"{path}: Class=\"{cls}\" no hereda de {ft.FullName}");
                return;
            }
            ft = ct;
        }
        if (Nullable.GetUnderlyingType(ft) is Type under) ft = under;

        bool hasChildren = el.ChildNodes.OfType<XmlElement>().Any();
        string text = el.InnerText.Trim();

        if (ft == typeof(string)) return;
        if (ft == typeof(Type))
        {
            if (ResolveType(text) == null) Err(ctx, $"{path}: el tipo \"{text}\" no existe");
            return;
        }
        if (ft.IsEnum)
        {
            CheckEnum(ft, text, path, ctx);
            return;
        }
        if (ft.IsPrimitive)
        {
            CheckPrimitive(ft, text, path, ctx);
            return;
        }
        if (defType.IsAssignableFrom(ft))
        {
            if (hasChildren)
            {
                // Def en línea (raro): validarla como objeto.
                ValidateObject(el, ft, path, ctx);
                return;
            }
            CheckDefRef(ft, text, path, ctx, el.GetAttribute("MayRequire"));
            return;
        }
        if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(List<>))
        {
            ValidateList(el, ft.GetGenericArguments()[0], path, ctx);
            return;
        }
        if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var key = ft.GetGenericArguments()[0];
            foreach (var c in el.ChildNodes.OfType<XmlElement>())
                if (c.Name != "li" && defType.IsAssignableFrom(key)) CheckDefRef(key, c.Name, path + ".key", ctx, c.GetAttribute("MayRequire"));
            return;
        }
        if (!hasChildren)
        {
            if (!IsTextParsed(ft) && text.Length > 0) Err(ctx, $"{path}: {ft.Name} se esperaba con sub-elementos, no texto");
            return;
        }
        ValidateObject(el, ft, path, ctx);
    }

    private static void ValidateList(XmlElement el, Type item, string path, Ctx ctx)
    {
        foreach (var c in el.ChildNodes.OfType<XmlElement>())
        {
            if (c.Name == "li")
            {
                // ThingDefCountClass y similares se escriben <DefName>n</DefName>, no <li><thingDef>…</thingDef>…</li>.
                if (HasCustomLoader(item) && c.ChildNodes.OfType<XmlElement>().Any() && item.Name.EndsWith("CountClass"))
                    Err(ctx, $"{path}: {item.Name} se escribe <DefName>cantidad</DefName>, no con <li>");
                else ValidateValue(c, item, path + "[]", ctx);
                continue;
            }
            // Listas con sintaxis abreviada <defName>valor</defName> (StatModifier, ThingDefCountClass…).
            if (item == statModifierType)
            {
                CheckDefRef(T("RimWorld.StatDef"), c.Name, path, ctx, c.GetAttribute("MayRequire"));
                CheckPrimitive(typeof(float), c.InnerText.Trim(), path + "." + c.Name, ctx);
            }
            else if (HasCustomLoader(item))
            {
                // <HediffDef>X</HediffDef> (DefHyperlink): tipo de Def + defName. Si no, <defName>n</defName> (ThingDefCountClass…).
                var asType = ResolveType(c.Name);
                if (asType != null && defType.IsAssignableFrom(asType)) CheckDefRef(asType, c.InnerText.Trim(), path, ctx, c.GetAttribute("MayRequire"));
                else if (!defs.Values.Any(d => d.ContainsKey(c.Name))) Err(ctx, $"{path}: <{c.Name}> no es una Def conocida");
            }
            else Err(ctx, $"{path}: elemento <{c.Name}> en una lista de {item.Name} (se esperaba <li>)");
        }
    }

    private static void CheckEnum(Type ft, string text, string path, Ctx ctx)
    {
        bool flags = ft.GetCustomAttributesData().Any(a => a.AttributeType == typeof(FlagsAttribute));
        var names = Enum.GetNames(ft);
        foreach (var part in (flags ? text.Split(',') : new[] { text }).Select(p => p.Trim()))
            if (!names.Contains(part)) Err(ctx, $"{path}: \"{part}\" no es un valor de {ft.Name} ({string.Join(", ", names.Take(12))}{(names.Length > 12 ? "…" : "")})");
    }

    private static void CheckPrimitive(Type ft, string text, string path, Ctx ctx)
    {
        bool ok;
        var inv = CultureInfo.InvariantCulture;
        if (ft == typeof(bool)) ok = text.Equals("true", StringComparison.OrdinalIgnoreCase) || text.Equals("false", StringComparison.OrdinalIgnoreCase);
        else if (ft == typeof(float) || ft == typeof(double)) ok = double.TryParse(text, NumberStyles.Float, inv, out _);
        else if (ft == typeof(char)) ok = text.Length == 1;
        else ok = long.TryParse(text, NumberStyles.Integer, inv, out _);
        if (!ok) Err(ctx, $"{path}: \"{text}\" no es un {ft.Name} válido");
    }

    private static void CheckDefRef(Type ft, string defName, string path, Ctx ctx, string mayRequire)
    {
        if (ft == null || string.IsNullOrEmpty(defName)) return;
        string src = DefSource(ft, defName);
        if (src == null)
        {
            if (string.IsNullOrEmpty(mayRequire)) Err(ctx, $"{path}: no existe {ft.Name} \"{defName}\"");
            return;
        }
        bool dependsOnDlc = ctx.dependencies.Contains("<packageid>ludeon.rimworld." + src.ToLowerInvariant() + "</packageid>");
        if (src != "Core" && src != "ours" && !ctx.conditional && !dependsOnDlc && string.IsNullOrEmpty(mayRequire))
            warnings.Add($"{ctx.file}: {path}: {ft.Name} \"{defName}\" solo existe en {src} (falla sin la DLC; usar MayRequire o Mods/{src})");
    }

    /// <summary>compClass nulo: el error que salta al cargar (p. ej. CompProperties_AbilityGiveHediff sin compClass).</summary>
    private static void CheckCompClass(XmlElement el, Type t, string path, Ctx ctx)
    {
        Type baseType = compPropsType != null && compPropsType.IsAssignableFrom(t) ? compPropsType
            : hediffCompPropsType != null && hediffCompPropsType.IsAssignableFrom(t) ? hediffCompPropsType
            : abilityCompPropsType != null && abilityCompPropsType.IsAssignableFrom(t) ? abilityCompPropsType : null;
        if (baseType == null || el["compClass"] != null) return;
        object instance;
        try { instance = Activator.CreateInstance(t, true); }
        catch { return; }
        var f = FindField(t, "compClass");
        if (f != null && f.GetValue(instance) == null) Err(ctx, $"{path}: {t.Name} sin <compClass> y su constructor no lo pone (compClass nulo)");
    }

    private static void Err(Ctx ctx, string msg) => errors.Add(ctx.file + ": " + msg);

    // ---------------- Idiomas ----------------

    private static void ValidateLanguages(string mod, string repo)
    {
        foreach (var langRoot in Directory.GetDirectories(mod, "Languages", SearchOption.AllDirectories))
        {
            // Keyed: mismas claves en los tres idiomas.
            var keyed = new Dictionary<string, HashSet<string>>();
            foreach (var lang in Directory.GetDirectories(langRoot))
            {
                string name = Path.GetFileName(lang);
                var set = new HashSet<string>();
                string keyedDir = Path.Combine(lang, "Keyed");
                if (Directory.Exists(keyedDir))
                    foreach (var f in Directory.GetFiles(keyedDir, "*.xml"))
                        foreach (var el in LoadLanguageData(f, repo)) set.Add(el.Name);
                keyed[name] = set;

                string injDir = Path.Combine(lang, "DefInjected");
                if (!Directory.Exists(injDir)) continue;
                foreach (var typeDir in Directory.GetDirectories(injDir))
                {
                    var t = ResolveType(Path.GetFileName(typeDir));
                    if (t == null)
                    {
                        errors.Add($"{Rel(typeDir, repo)}: carpeta DefInjected de un tipo que no existe");
                        continue;
                    }
                    foreach (var f in Directory.GetFiles(typeDir, "*.xml"))
                        foreach (var el in LoadLanguageData(f, repo))
                        {
                            var parts = el.Name.Split('.');
                            // comps.0 cambia con la herencia (el índice no es estable): usar el handle (nombre del compClass).
                            for (int pi = 0; pi + 1 < parts.Length; pi++)
                                if (parts[pi] == "comps" && parts[pi + 1].All(char.IsDigit))
                                    errors.Add($"{Rel(f, repo)}: <{el.Name}>: usar comps.<compClass> (p. ej. comps.CompUsable) en vez del índice");
                            if (DefSource(t, parts[0]) == null && !IsImpliedDef(parts[0]))
                                errors.Add($"{Rel(f, repo)}: <{el.Name}>: no existe {t.Name} \"{parts[0]}\"");
                            else if (parts.Length > 1 && FindField(t, parts[1]) == null)
                                errors.Add($"{Rel(f, repo)}: <{el.Name}>: {t.Name} no tiene el campo \"{parts[1]}\"");
                        }
                }
            }
            if (!keyed.TryGetValue("English", out var en)) continue;
            foreach (var kv in keyed.Where(k => k.Key != "English"))
            {
                foreach (var missing in en.Except(kv.Value)) errors.Add($"{Rel(langRoot, repo)}/{kv.Key}: falta la clave Keyed {missing}");
                foreach (var extra in kv.Value.Except(en)) warnings.Add($"{Rel(langRoot, repo)}/{kv.Key}: clave Keyed {extra} que no está en English");
            }
        }
    }

    /// <summary>Defs que el juego crea solo (recetas de recipeMaker, planos, marcos, cadáveres…).</summary>
    private static bool IsImpliedDef(string defName)
    {
        var thingDef = T("Verse.ThingDef");
        foreach (var prefix in new[] { "Make_", "Blueprint_", "Frame_", "Corpse_", "Meat_", "Install_" })
            if (defName.StartsWith(prefix) && DefSource(thingDef, defName.Substring(prefix.Length)) != null) return true;
        return false;
    }

    private static IEnumerable<XmlElement> LoadLanguageData(string file, string repo)
    {
        var doc = new XmlDocument();
        try { doc.Load(file); }
        catch (Exception ex)
        {
            errors.Add($"{Rel(file, repo)}: XML no parsea: {ex.Message}");
            yield break;
        }
        foreach (XmlNode n in doc.DocumentElement.ChildNodes)
            if (n is XmlElement el) yield return el;
    }

    private static void ValidateCodeKeys(string repo)
    {
        var english = new HashSet<string>();
        foreach (var f in Directory.GetFiles(Path.Combine(repo, "Mods"), "*.xml", SearchOption.AllDirectories))
            if (f.Replace('\\', '/').Contains("/Languages/English/Keyed/"))
                foreach (var el in LoadLanguageData(f, repo)) english.Add(el.Name);
        var rx = new Regex("\"(CVG_[A-Za-z0-9_]+)\"\\s*\\.Translate");
        foreach (var f in Directory.GetFiles(Path.Combine(repo, "Source"), "*.cs", SearchOption.AllDirectories))
        {
            if (f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)) continue;
            foreach (Match m in rx.Matches(File.ReadAllText(f)))
                if (!english.Contains(m.Groups[1].Value)) errors.Add($"{Rel(f, repo)}: la clave {m.Groups[1].Value} no está en Keyed (English)");
        }
    }

    private static string Rel(string path, string repo) => path.Substring(repo.Length).TrimStart('\\', '/').Replace('\\', '/');
}
