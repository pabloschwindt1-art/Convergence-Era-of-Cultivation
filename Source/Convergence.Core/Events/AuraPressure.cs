using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Convergence
{
    /// <summary>
    /// Presión de aura (DEFENSA §4): cuando un asalto trae un cultivador de un reino superior al de todos los colonos, el jugador recibe una carta
    /// especial que lo avisa y dice cuánto más alto es. No cambia el asalto: el límite de reino de los enemigos ya lo aplica <c>scaleEnemyRealms</c>.
    /// </summary>
    public static class AuraPressure
    {
        /// <summary>Si entre los hostiles de <paramref name="faction"/> en el mapa hay alguien de reino superior al mejor de la colonia, envía la carta. Devuelve si la envió.</summary>
        public static bool TryNotify(Map map, Faction faction)
        {
            if (map == null || faction == null || !GameComponent_Convergence.Active) return false;
            int colony = CultivatorPawnKindUtility.ColonyMaxRealm();
            Pawn top = null;
            int topLevel = 0;
            foreach (var p in map.mapPawns.AllPawnsSpawned)
            {
                if (p.Faction != faction || !p.HostileTo(Faction.OfPlayer)) continue;
                int level = CompCultivator.For(p)?.RealmLevel ?? 0;
                if (level > topLevel) { topLevel = level; top = p; }
            }
            if (top == null || topLevel <= colony) return false;
            var realm = CompCultivator.For(top).Realm.Realm;
            Find.LetterStack.ReceiveLetter("CVG_AuraPressure_Label".Translate(), "CVG_AuraPressure_Text".Translate(top.LabelShortCap, realm.LabelCap, faction.Name), LetterDefOf.ThreatBig, top);
            return true;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecuteWorker")]
    public static class Patch_IncidentWorker_Raid_AuraPressure
    {
        public static void Postfix(IncidentParms parms, bool __result)
        {
            if (__result && parms.target is Map map && map.IsPlayerHome) AuraPressure.TryNotify(map, parms.faction);
        }
    }
}
