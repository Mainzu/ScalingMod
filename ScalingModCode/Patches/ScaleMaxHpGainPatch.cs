using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using ScalingMod.ScalingModCode.Relics;

namespace ScalingMod.ScalingModCode.Patches;

/// <summary>
/// Multiplies the player's max HP gains by (1 + x). The game has no hook for this, so CreatureCmd.GainMaxHp is patched.
/// </summary>
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainMaxHp))]
public static class ScaleMaxHpGainPatch
{
    public static void Prefix(Creature creature, ref decimal amount)
    {
        ScalingRelic? relic = creature.Player?.GetRelic<ScalingRelic>();
        if (relic == null || relic.Hundredths <= 0) return;

        amount = Math.Round(amount * ScalingSettings.RewardMultiplier(relic.X));
    }
}
