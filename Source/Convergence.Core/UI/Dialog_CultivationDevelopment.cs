using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Convergence
{
    /// <summary>
    /// Ventana "Desarrollo" (botón de la pestaña Cultivo) con la estructura de la pestaña de Vanilla Psycasts Expanded
    /// (decisión del usuario: copiar lo que funciona, escrito por nosotros):
    /// - Izquierda: nombre, reino y etapa con barra de progreso, puntos de comprensión, stats de cultivo con icono, raíz
    ///   espiritual como fila de elementos, conjuntos de combate (arrastrar técnicas aprendidas, clic derecho quita) y
    ///   buscador.
    /// - Derecha: pestaña "Técnicas" con los caminos como cartas verticales con arte (5 por fila, scroll vertical). Cerrada:
    ///   arte oscurecido y botón "Desbloquear" (1 punto) o el motivo. Abierta: el árbol encima del arte, nivel 1 abajo.
    /// Solo Widgets y GUI.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Dialog_CultivationDevelopment : Window
    {
        private const float Gap = 10f;
        private const float CardLabelHeight = 30f;
        private const float NodeSize = 40f;
        private const float NodeSpacing = 54f;
        private const float SlotSize = 36f;
        private const int MaxSets = 8;

        private static readonly Color LockedVeil = new Color(0f, 0f, 0f, 0.6f);
        private static readonly Color ClosedVeil = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color LineOwned = new Color(0.95f, 0.95f, 0.95f, 0.95f);
        private static readonly Color LineMissing = new Color(0.45f, 0.45f, 0.45f, 0.8f);
        public static readonly Color PassiveBorder = new Color(0.95f, 0.8f, 0.4f);
        private static readonly Color ActiveSetColor = new Color(0.35f, 0.55f, 0.75f, 0.3f);
        private static readonly Texture2D SearchIcon = ContentFinder<Texture2D>.Get("UI/Widgets/Search", reportFailure: false);

        // Iconos provisionales de los stats (arte propio: UI/Stats/<defName>).
        private static readonly Dictionary<string, string> StatIcons = new Dictionary<string, string>
        {
            { "CVG_QiMax", "UI/Techniques/CVG_Tech_QiShield" },
            { "CVG_QiRegen", "UI/Techniques/CVG_Tech_SpringRejuvenation" },
            { "CVG_CultivationSpeed", "UI/Techniques/CVG_Tech_SwiftStep" },
            { "CVG_BreakthroughChance", "UI/Commands/CVG_Breakthrough" },
            { "CVG_CelestialDissonance", "UI/Techniques/CVG_Tech_ThornStorm" },
            { "CVG_TechniqueCost", "UI/Techniques/CVG_Tech_Fireball" },
            { "CVG_BloodQiMax", "UI/Techniques/CVG_Tech_MountainBody" },
            { "CVG_BodyTemperingSpeed", "UI/Techniques/CVG_Tech_EarthTremor" },
        };
        private static readonly Dictionary<StatDef, Texture2D> statIconCache = new Dictionary<StatDef, Texture2D>();
        private static readonly Dictionary<QiElementDef, Texture2D> elementIcons = new Dictionary<QiElementDef, Texture2D>();
        private static readonly Dictionary<TechniqueManualDef, Texture2D> gradientCache = new Dictionary<TechniqueManualDef, Texture2D>();
        private static readonly Dictionary<Color, Texture2D> barTextures = new Dictionary<Color, Texture2D>();

        private readonly Pawn pawn;
        private Vector2 setsScroll;
        private Vector2 pathsScroll;
        private float lastPathsHeight;
        private string search = "";
        private AbilityDef dragging;
        private readonly List<TabRecord> tabs = new List<TabRecord>();

        public override Vector2 InitialSize => new Vector2(Mathf.Min(1600f, UI.screenWidth - 40f), Mathf.Min(920f, UI.screenHeight - 40f));

        public Dialog_CultivationDevelopment(Pawn pawn)
        {
            this.pawn = pawn;
            CultivationGuide.Demonstrated(CultivationGuide.Techniques, KnowledgeAmount.Total);
            doCloseX = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
            draggable = true;
            forcePause = false;
            CompCultivator.For(pawn)?.Techniques.SyncOpenPaths();
            tabs.Add(new TabRecord("CVG_Dev_TabTechniques".Translate(), () => { }, true));
        }

        /// <summary>Solo se gasta en colonos propios (o en modo desarrollador).</summary>
        private bool CanEdit => (pawn.Faction?.IsPlayer ?? false) || Prefs.DevMode;

        private bool Matches(AbilityDef def) => search.NullOrEmpty() || def.label.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;

        public override void DoWindowContents(Rect inRect)
        {
            var comp = CompCultivator.For(pawn);
            if (comp == null || pawn.Destroyed)
            {
                Close();
                return;
            }
            var techniques = comp.Techniques;
            float leftWidth = Mathf.Clamp(inRect.width * 0.27f, 300f, 430f);
            var left = new Rect(inRect.x, inRect.y, leftWidth, inRect.height);
            var right = new Rect(left.xMax + Gap, inRect.y + 32f, inRect.width - leftWidth - Gap, inRect.height - 32f);

            DoLeft(left, comp, techniques);
            TabDrawer.DrawTabs(new Rect(right.x, right.y, right.width, 32f), tabs, 220f);
            DoPaths(right, comp, techniques);

            // Arrastrar: el icono sigue al ratón; al soltar fuera de un conjunto se cancela.
            if (dragging != null)
            {
                var mouse = Event.current.mousePosition;
                GUI.DrawTexture(new Rect(mouse.x - SlotSize / 2f, mouse.y - SlotSize / 2f, SlotSize, SlotSize), dragging.uiIcon);
                if (Event.current.rawType == EventType.MouseUp) dragging = null;
            }
        }

        // ================================================================ izquierda

        private void DoLeft(Rect rect, CompCultivator comp, TechniqueTracker techniques)
        {
            const float searchHeight = 30f;
            var top = new Rect(rect.x, rect.y, rect.width, rect.height - searchHeight - 6f);
            var listing = new Listing_Standard();
            listing.Begin(top);

            Text.Font = GameFont.Medium;
            listing.Label(pawn.LabelShortCap);
            var tracker = comp.Realm.Started ? comp.Realm : comp.ActiveTrackers.FirstOrDefault(t => t.Started);
            listing.Label(tracker != null
                ? "CVG_Dev_RealmLine".Translate(tracker.Realm.LabelCap, tracker.Stage)
                : "CVG_Dev_NoRealm".Translate());
            Text.Font = GameFont.Small;
            listing.Gap(6f);

            // Barra de progreso de la etapa (como la de experiencia de VPE) y cómo se gana.
            var bar = listing.GetRect(24f).ContractedBy(14f, 0f);
            if (tracker != null)
            {
                Widgets.FillableBar(bar, tracker.StagePercent, BarTex(tracker.Path.resourceColor), BaseContent.BlackTex, true);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(bar, tracker.Bottleneck ? "CVG_Dev_Bottleneck".Translate().ToString()
                    : tracker.Progress.ToString("F0") + " / " + tracker.Realm.progressPerStage.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;
            listing.Label(tracker?.Path == CVG_DefOf.CVG_Path_Body ? "CVG_Dev_ProgressHintBody".Translate() : "CVG_Dev_ProgressHint".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            listing.Gap(8f);

            listing.Label("CVG_Dev_PointsAvailable".Translate(techniques.Points));
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.8f, 0.8f, 0.8f);
            listing.Label("CVG_Dev_PointsHint".Translate());
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            if (Prefs.DevMode && Widgets.ButtonText(listing.GetRect(24f).LeftPart(0.3f), "Dev: +1"))
                techniques.AddPoints(1);
            listing.Gap(6f);

            // Stats con icono por fila (como las de VPE), con el desglose vanilla en el tooltip. En pantallas bajas, en
            // dos columnas para dejar sitio a los conjuntos.
            listing.Label("CVG_Dev_Stats".Translate());
            var stats = StatsFor(comp).ToList();
            bool twoColumns = rect.height < 760f;
            if (twoColumns)
            {
                for (int i = 0; i < stats.Count; i += 2)
                {
                    var line = listing.GetRect(26f);
                    StatRow(line.LeftHalf(), stats[i]);
                    if (i + 1 < stats.Count) StatRow(line.RightHalf(), stats[i + 1]);
                }
            }
            else foreach (var stat in stats) StatRow(listing.GetRect(26f), stat);
            listing.Gap(6f);

            // Raíz espiritual: los cinco elementos como los tipos de foco de VPE (los de la raíz, brillantes).
            listing.Label("CVG_Dev_Root".Translate(CultivationText.RootLabel(comp.Root)));
            var row = listing.GetRect(44f);
            float x = row.x;
            foreach (var element in DefDatabase<QiElementDef>.AllDefsListForReading)
            {
                var r = new Rect(x, row.y, 40f, 40f);
                bool has = TechniqueUtility.HasElement(comp, element);
                Widgets.DrawBoxSolid(r, new Color(0.1f, 0.1f, 0.1f, 0.6f));
                GUI.color = has ? Color.white : new Color(1f, 1f, 1f, 0.25f);
                var icon = ElementIcon(element);
                if (icon != null) GUI.DrawTexture(r.ContractedBy(3f), icon);
                GUI.color = has ? element.color : new Color(0.4f, 0.4f, 0.4f);
                Widgets.DrawBox(r, has ? 2 : 1);
                GUI.color = Color.white;
                TooltipHandler.TipRegion(r, element.LabelCap + "\n" + (has ? "CVG_Dev_ElementOwned" : "CVG_Dev_ElementMissing").Translate());
                x += 44f;
            }
            listing.Gap(8f);

            // Conjuntos de combate.
            listing.Label("CVG_Set_Title".Translate());
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.8f, 0.8f, 0.8f);
            listing.Label("CVG_Set_Desc".Translate(techniques.ActiveSlots));
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            // Los conjuntos ocupan el resto, calculado a mano: si el listado se pasa de alto abre otra columna.
            float used = listing.CurHeight;
            listing.End();
            var setsRect = new Rect(top.x, top.y + used, top.width, Mathf.Max(60f, top.height - used));
            DoSets(setsRect, techniques);

            // Buscador (resalta técnicas por nombre).
            var searchRect = new Rect(rect.x, rect.yMax - searchHeight, rect.width, searchHeight);
            if (SearchIcon != null) GUI.DrawTexture(new Rect(searchRect.x + 4f, searchRect.y + 5f, 20f, 20f), SearchIcon);
            search = Widgets.TextField(new Rect(searchRect.x + 30f, searchRect.y, searchRect.width - 30f, searchRect.height), search);
            TooltipHandler.TipRegion(searchRect, "CVG_Dev_SearchTip".Translate());
        }

        private static Texture2D BarTex(Color c)
        {
            if (!barTextures.TryGetValue(c, out var tex)) barTextures[c] = tex = SolidColorMaterials.NewSolidColorTexture(c);
            return tex;
        }

        private static IEnumerable<StatDef> StatsFor(CompCultivator comp)
        {
            if (comp.IsQiCultivator)
            {
                yield return CVG_DefOf.CVG_QiMax;
                yield return CVG_DefOf.CVG_QiRegen;
                yield return CVG_DefOf.CVG_CultivationSpeed;
                yield return CVG_DefOf.CVG_BreakthroughChance;
                yield return CVG_DefOf.CVG_CelestialDissonance;
                yield return CVG_DefOf.CVG_TechniqueCost;
            }
            if (comp.TrackerFor(CVG_DefOf.CVG_Path_Body)?.Started ?? false)
            {
                var bloodMax = DefDatabase<StatDef>.GetNamedSilentFail("CVG_BloodQiMax");
                if (bloodMax != null) yield return bloodMax;
                yield return CVG_DefOf.CVG_BodyTemperingSpeed;
            }
        }

        private void StatRow(Rect r, StatDef stat)
        {
            if (!statIconCache.TryGetValue(stat, out var icon))
            {
                icon = ContentFinder<Texture2D>.Get("UI/Stats/" + stat.defName, reportFailure: false);
                if (icon == null && StatIcons.TryGetValue(stat.defName, out var path)) icon = ContentFinder<Texture2D>.Get(path, reportFailure: false);
                statIconCache[stat] = icon;
            }
            if (icon != null) GUI.DrawTexture(new Rect(r.x + 4f, r.y + 2f, 22f, 22f), icon);
            float value = pawn.GetStatValue(stat);
            // El valor siempre visible a la derecha; el nombre se acorta si no cabe (completo en el tooltip).
            string valueText = stat.ValueToString(value);
            float valueWidth = Text.CalcSize(valueText).x + 6f;
            var labelRect = new Rect(r.x + 30f, r.y + 2f, r.width - 30f - valueWidth, 24f);
            Widgets.Label(labelRect, stat.LabelCap.Resolve().Truncate(labelRect.width));
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(new Rect(r.xMax - valueWidth - 4f, r.y + 2f, valueWidth, 24f), valueText);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.DrawHighlightIfMouseover(r);
            TooltipHandler.TipRegion(r, () => stat.LabelCap.AsTipTitle() + "\n" + stat.description + "\n\n"
                + stat.Worker.GetExplanationFull(StatRequest.For(pawn), ToStringNumberSense.Absolute, value), stat.GetHashCode());
        }

        private static Texture2D ElementIcon(QiElementDef element)
        {
            if (element == null) return null;
            if (!elementIcons.TryGetValue(element, out var tex))
            {
                string name = element.defName.Replace("CVG_Element_", "");
                tex = ContentFinder<Texture2D>.Get("UI/Icons/Genes/CVG_RootElement_" + name, reportFailure: false);
                elementIcons[element] = tex;
            }
            return tex;
        }

        private void DoSets(Rect rect, TechniqueTracker techniques)
        {
            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(4f, 6f);
            float blockHeight = 26f + SlotSize + 10f;
            var view = new Rect(0f, 0f, inner.width - 16f, techniques.Sets.Count * (blockHeight + 6f) + 46f);
            Widgets.BeginScrollView(inner, ref setsScroll, view);
            float y = 0f;
            for (int i = 0; i < techniques.Sets.Count; i++)
            {
                if (DrawSet(new Rect(0f, y, view.width, blockHeight), techniques, i)) break;
                y += blockHeight + 6f;
            }
            if (CanEdit && techniques.Sets.Count < MaxSets
                && Widgets.ButtonText(new Rect(4f, y + 4f, Mathf.Min(200f, view.width - 8f), 32f), "CVG_Set_Create".Translate()))
            {
                techniques.AddSet("CVG_Set_DefaultName".Translate(techniques.Sets.Count + 1));
                techniques.SetActiveSet(techniques.Sets.Count - 1);
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            Widgets.EndScrollView();
        }

        /// <summary>Dibuja un conjunto. Devuelve true si se borró (la lista cambió).</summary>
        private bool DrawSet(Rect block, TechniqueTracker techniques, int index)
        {
            var set = techniques.Sets[index];
            bool active = index == techniques.ActiveSetIndex;
            if (active) Widgets.DrawBoxSolid(block, ActiveSetColor);
            Widgets.DrawHighlightIfMouseover(block);

            // Cabecera: nombre (editable en el activo; clic en otro lo activa) y quitar.
            var nameRect = new Rect(block.x + 4f, block.y + 2f, block.width - 92f, 24f);
            if (active && CanEdit) set.name = Widgets.TextField(nameRect, set.Label);
            else
            {
                Widgets.Label(nameRect, set.Label.Truncate(nameRect.width));
                if (Widgets.ButtonInvisible(nameRect))
                {
                    techniques.SetActiveSet(index);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegion(nameRect, "CVG_Set_RowTip".Translate());
            }
            if (CanEdit && Widgets.ButtonText(new Rect(block.xMax - 84f, block.y + 2f, 80f, 22f), "CVG_Set_Remove".Translate()))
            {
                techniques.RemoveSet(set);
                return true;
            }

            // Ranuras: clic derecho quita; soltar una técnica arrastrada la agrega.
            float x = block.x + 4f;
            for (int i = 0; i < techniques.ActiveSlots; i++)
            {
                var slot = new Rect(x, block.y + 28f, SlotSize, SlotSize);
                GUI.DrawTexture(slot, Command.BGTexShrunk);
                if (i < set.techniques.Count)
                {
                    var def = set.techniques[i];
                    GUI.DrawTexture(slot.ContractedBy(2f), def.uiIcon);
                    TooltipHandler.TipRegion(slot, def.LabelCap + "\n\n" + "CVG_Set_SlotTip".Translate());
                    if (CanEdit && Mouse.IsOver(slot) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    {
                        techniques.SetInSet(set, def, false);
                        SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                        Event.current.Use();
                    }
                }
                x += SlotSize + 4f;
            }
            if (dragging != null && CanEdit && Mouse.IsOver(block))
            {
                Widgets.DrawBox(block, 2);
                if (Event.current.rawType == EventType.MouseUp)
                {
                    if (techniques.SetInSet(set, dragging, true)) SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                    else Messages.Message("CVG_Tab_NoFreeSlot".Translate(techniques.ActiveSlots), MessageTypeDefOf.RejectInput, historical: false);
                    dragging = null;
                }
            }
            return false;
        }

        // ================================================================ derecha: cartas de caminos

        private void DoPaths(Rect rect, CompCultivator comp, TechniqueTracker techniques)
        {
            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(4f);
            int perRow = Mathf.Clamp(Mathf.FloorToInt(inner.width / 200f), 2, 5);
            float cardWidth = (inner.width - 16f - (perRow + 1) * Gap) / perRow;
            float artHeight = cardWidth * 1.5f;
            var paths = DefDatabase<TechniqueManualDef>.AllDefsListForReading
                .OrderByDescending(techniques.KnowsManual)
                .ThenByDescending(m => techniques.CanOpenPath(m, out _, ignorePoints: true))
                .ThenBy(m => m.order).ToList();
            var view = new Rect(0f, 0f, inner.width - 16f, Mathf.Max(lastPathsHeight, inner.height - 1f));
            Widgets.BeginScrollView(inner, ref pathsScroll, view);
            var pos = new Vector2(Gap, Gap);
            int col = 0;
            foreach (var path in paths)
            {
                var card = new Rect(pos.x, pos.y, cardWidth, artHeight + CardLabelHeight);
                DrawCard(card, path, techniques);
                col++;
                if (col == perRow)
                {
                    col = 0;
                    pos.x = Gap;
                    pos.y += card.height + Gap;
                }
                else pos.x += cardWidth + Gap;
            }
            lastPathsHeight = pos.y + (col > 0 ? artHeight + CardLabelHeight + Gap : 0f);
            Widgets.EndScrollView();
        }

        private void DrawCard(Rect card, TechniqueManualDef path, TechniqueTracker techniques)
        {
            bool open = techniques.KnowsManual(path);
            var art = new Rect(card.x, card.y, card.width, card.height - CardLabelHeight);
            DrawCardArt(art, path);

            var label = new Rect(card.x, art.yMax, card.width, CardLabelHeight);
            Widgets.DrawBoxSolid(label, Widgets.WindowBGFillColor);
            Text.Anchor = TextAnchor.MiddleCenter;
            bool anyMatch = !search.NullOrEmpty() && path.Techniques.Any(Matches);
            GUI.color = anyMatch ? new Color(1f, 0.9f, 0.5f) : open ? Color.white : new Color(0.75f, 0.75f, 0.75f);
            // Nombres largos: fuente pequeña antes que cortarlos.
            if (Text.CalcSize(path.LabelCap).x > label.width - 6f) Text.Font = GameFont.Tiny;
            Widgets.Label(label, path.LabelCap.Truncate(label.width - 6f));
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.DrawBox(card);

            if (open)
            {
                DrawTree(art, path, techniques);
                return;
            }

            // Cerrado: arte oscurecido y "Desbloquear" (1 punto); si no se puede, botón apagado y el motivo en el tooltip.
            Widgets.DrawBoxSolid(art, ClosedVeil);
            float buttonWidth = Mathf.Min(art.width - 12f, 180f);
            var button = new Rect(art.center.x - buttonWidth / 2f, art.center.y - 16f, buttonWidth, 32f);
            bool canOpen = techniques.CanOpenPath(path, out string reason);
            if (canOpen && CanEdit)
            {
                if (Widgets.ButtonText(button, "CVG_Path_Unlock".Translate()))
                {
                    techniques.OpenPath(path);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    Messages.Message("CVG_Path_Opened".Translate(pawn.Named("PAWN"), path.LabelCap), pawn, MessageTypeDefOf.PositiveEvent, historical: false);
                }
            }
            else
            {
                GUI.color = Color.grey;
                Widgets.ButtonText(button, "CVG_Path_Locked".Translate(), active: false);
                GUI.color = Color.white;
            }
            TooltipHandler.TipRegion(card, new TipSignal(() => PathTooltip(path, canOpen ? null : reason), path.GetHashCode()));
        }

        /// <summary>Arte de la carta o, mientras falte, un degradado del color del camino con su glifo grande y tenue.</summary>
        private static void DrawCardArt(Rect art, TechniqueManualDef path)
        {
            var tex = path.CardTex;
            if (tex != null)
            {
                GUI.DrawTexture(art, tex, ScaleMode.ScaleAndCrop);
                return;
            }
            if (!gradientCache.TryGetValue(path, out var gradient))
            {
                gradient = new Texture2D(1, 64) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                var c = path.Color;
                for (int i = 0; i < 64; i++)
                {
                    float t = i / 63f; // 0 abajo, 1 arriba
                    gradient.SetPixel(0, i, Color.Lerp(new Color(c.r * 0.08f, c.g * 0.08f, c.b * 0.1f, 1f), new Color(c.r * 0.45f, c.g * 0.45f, c.b * 0.5f, 1f), t * t));
                }
                gradient.Apply();
                gradientCache[path] = gradient;
            }
            GUI.DrawTexture(art, gradient);
            float glyph = art.width * 0.8f;
            GUI.color = new Color(1f, 1f, 1f, 0.12f);
            GUI.DrawTexture(new Rect(art.center.x - glyph / 2f, art.center.y - glyph / 2f, glyph, glyph), path.Icon);
            GUI.color = Color.white;
        }

        private void DrawTree(Rect art, TechniqueManualDef path, TechniqueTracker techniques)
        {
            var byLevel = path.Techniques.GroupBy(t => TechniqueUtility.Ext(t).tier)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => TechniqueUtility.Ext(t).order).ToList());
            int levels = byLevel.Keys.DefaultIfEmpty(1).Max();
            float rowHeight = (art.height - 16f) / levels;
            var centers = new Dictionary<AbilityDef, Vector2>();
            foreach (var kv in byLevel)
            {
                // Nivel 1 abajo, como VPE.
                float cy = art.yMax - 8f - (kv.Key - 0.5f) * rowHeight;
                float spacing = Mathf.Min(NodeSpacing, (art.width - 8f) / kv.Value.Count);
                for (int i = 0; i < kv.Value.Count; i++)
                    centers[kv.Value[i]] = new Vector2(art.center.x + (i - (kv.Value.Count - 1) / 2f) * spacing, cy);
            }
            foreach (var def in path.Techniques)
            {
                var pre = TechniqueUtility.Ext(def).prerequisites;
                if (pre == null) continue;
                foreach (var p in pre)
                    if (centers.TryGetValue(p, out var from))
                        Widgets.DrawLine(from, centers[def], techniques.IsUnlocked(p) ? LineOwned : LineMissing, 2f);
            }
            foreach (var def in path.Techniques)
                DrawNode(centers[def], def, techniques);
        }

        private void DrawNode(Vector2 center, AbilityDef def, TechniqueTracker techniques)
        {
            var rect = new Rect(center.x - NodeSize / 2f, center.y - NodeSize / 2f, NodeSize, NodeSize);
            bool unlocked = techniques.IsUnlocked(def);
            bool canUnlock = !unlocked && techniques.CanUnlock(def, out _);
            bool passive = PassiveTechniqueUtility.IsPassive(def);

            if (canUnlock)
            {
                // Brillo que late (disponible).
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.realtimeSinceStartup * 4f);
                Widgets.DrawStrongHighlight(rect.ExpandedBy(6f + 3f * pulse));
            }
            GUI.color = Mouse.IsOver(rect) ? GenUI.MouseoverColor : Color.white;
            GUI.DrawTexture(rect, Command.BGTexShrunk);
            GUI.color = Color.white;
            GUI.DrawTexture(rect.ContractedBy(2f), def.uiIcon);
            if (!unlocked && !canUnlock) Widgets.DrawBoxSolid(rect, LockedVeil);
            if (!search.NullOrEmpty() && !Matches(def)) Widgets.DrawBoxSolid(rect, LockedVeil);
            GUI.color = passive ? PassiveBorder : unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            Widgets.DrawBox(rect, unlocked || passive ? 2 : 1);
            GUI.color = Color.white;
            if (unlocked && techniques.ActiveSet != null && techniques.ActiveSet.techniques.Contains(def))
                GUI.DrawTexture(new Rect(rect.xMax - 12f, rect.y - 4f, 16f, 16f), Widgets.CheckboxOnTex);

            TooltipHandler.TipRegion(rect, new TipSignal(() => NodeTooltip(def, techniques, canUnlock), def.GetHashCode()));

            if (!CanEdit) return;
            var ev = Event.current;
            if (canUnlock && Widgets.ButtonInvisible(rect))
            {
                techniques.Unlock(def);
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Messages.Message("CVG_Technique_Unlocked".Translate(pawn.Named("PAWN"), def.LabelCap), pawn, MessageTypeDefOf.PositiveEvent, historical: false);
            }
            else if (unlocked && !passive && Mouse.IsOver(rect) && ev.type == EventType.MouseDown && ev.button == 0)
            {
                // Arrastrar a un conjunto (VPE: arrastrar psycasts aprendidos a los psysets).
                dragging = def;
                SoundDefOf.Click.PlayOneShotOnCamera();
                ev.Use();
            }
        }

        private static string PathTooltip(TechniqueManualDef path, string lockedReason)
        {
            var sb = new StringBuilder();
            sb.AppendLine(path.LabelCap.AsTipTitle());
            sb.AppendLine(path.description);
            sb.AppendLine();
            sb.AppendLine("CVG_Dev_PathTechniques".Translate());
            foreach (var t in path.Techniques)
                sb.AppendLine("  - " + t.LabelCap.ToString() + (PassiveTechniqueUtility.IsPassive(t) ? " (" + "CVG_Dev_Passive".Translate().ToString() + ")" : ""));
            if (!lockedReason.NullOrEmpty())
            {
                sb.AppendLine();
                sb.Append(lockedReason.Colorize(ColorLibrary.RedReadable));
            }
            return sb.ToString().TrimEndNewlines();
        }

        private string NodeTooltip(AbilityDef def, TechniqueTracker techniques, bool canUnlock)
        {
            var ext = TechniqueUtility.Ext(def);
            bool passive = PassiveTechniqueUtility.IsPassive(def);
            var sb = new StringBuilder();
            sb.AppendLine(def.LabelCap.AsTipTitle());
            sb.AppendLine((passive ? "CVG_Technique_PassiveLine" : "CVG_Technique_ActiveLine").Translate()
                .Colorize(passive ? PassiveBorder : ColoredText.TipSectionTitleColor));
            sb.AppendLine(def.description);
            sb.AppendLine();
            var costComp = def.comps?.OfType<CompProperties_AbilityResourceCost>().FirstOrDefault();
            if (costComp != null)
                sb.AppendLine("CVG_Ability_ResourceCost".Translate((costComp.path ?? ext.manual.Path).resourceLabel.CapitalizeFirst(),
                    (costComp.BaseCost * TechniqueUtility.CostFactor(pawn, def)).ToString("0")));
            if (def.cooldownTicksRange.max > 0)
                sb.AppendLine("CVG_Technique_CooldownLine".Translate(
                    Mathf.RoundToInt(def.cooldownTicksRange.Average * TechniqueUtility.CooldownFactor(pawn, def)).ToStringTicksToPeriod()));
            if (ext.minRealm != null) sb.AppendLine("CVG_Technique_RealmLine".Translate(ext.minRealm.LabelCap));
            if (!ext.prerequisites.NullOrEmpty())
                sb.AppendLine("CVG_Technique_PrerequisitesLine".Translate(ext.prerequisites.Select(p => p.label).ToCommaListOr()));
            sb.AppendLine("CVG_Technique_PowerLine".Translate(TechniqueUtility.Power(pawn, def).ToStringPercent()));
            sb.AppendLine();
            if (techniques.IsUnlocked(def))
            {
                var mastery = techniques.Mastery(def);
                var next = mastery?.Next;
                if (mastery != null)
                    sb.AppendLine("CVG_Technique_MasteryLine".Translate(mastery.LabelCap.Colorize(mastery.color),
                        next == null ? "-" : techniques.MasteryXp(def).ToString("0") + " / " + next.xpRequired.ToString("0")));
                if (!passive) sb.Append("CVG_Dev_DragToSet".Translate().Colorize(ColoredText.SubtleGrayColor));
            }
            else if (canUnlock)
                sb.Append("CVG_Tab_ClickToUnlock".Translate(ext.unlockCost).Resolve().ToUpper().Colorize(ColoredText.TipSectionTitleColor));
            else if (!techniques.CanUnlock(def, out string reason) && reason != null)
                sb.Append(reason.Colorize(ColorLibrary.RedReadable));
            return sb.ToString().TrimEndNewlines();
        }
    }
}
