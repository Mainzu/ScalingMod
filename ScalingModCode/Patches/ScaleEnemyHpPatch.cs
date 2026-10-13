using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace ScalingMod.ScalingModCode.Patches;

/// <summary>
/// Scales enemy max HP by e^x everywhere the game applies its own multiplayer HP scaling.
/// Creature.ScaleHpForMultiplayer is what monsters call when they set HP after spawning (Test Subject revives,
/// Tough Egg hatching, Decimillipede segments re-rolling HP), so patching it covers those too.
/// It is skipped at spawn in single player, so spawning is handled separately.
/// </summary>
public static class ScaleEnemyHpPatch
{
    private static decimal Scale(decimal hp)
    {
        RunState? runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null) return hp;

        decimal x = ScalingSettings.TotalX(runState);
        if (x <= 0m) return hp;
        return Math.Max(1m, Math.Round(hp * ScalingSettings.EnemyDefenseMultiplier(x)));
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.ScaleHpForMultiplayer))]
    public static class ScaledHp
    {
        public static void Postfix(ref decimal __result) => __result = Scale(__result);
    }

    /// At spawn, the game only calls ScaleHpForMultiplayer when there is more than one player.
    [HarmonyPatch(typeof(Creature), nameof(Creature.ScaleMonsterHpForMultiplayer))]
    public static class SinglePlayerSpawn
    {
        public static void Postfix(Creature __instance, int playerCount)
        {
            if (playerCount > 1) return;

            decimal maxHp = Scale(__instance.MaxHp);
            __instance.SetMaxHpInternal(maxHp);
            __instance.SetCurrentHpInternal(maxHp);
        }
    }
}
