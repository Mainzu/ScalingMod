using System.Collections.Concurrent;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ScalingMod.ScalingModCode.Relics;

namespace ScalingMod.ScalingModCode.Patches;

/// <summary>
/// Multiplies the numbers on the player's relics and potions by (1 + x), the same way the RelicUpgrades mod scales
/// relics: by adjusting the DynamicVar values they read when triggering. Their tooltips show the scaled values too.
/// Gold and max HP vars are excluded because those gains are already scaled where they are received.
/// </summary>
[HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.BaseValue), MethodType.Getter)]
public static class RelicPotionScalingPatch
{
    private static readonly AccessTools.FieldRef<DynamicVar, AbstractModel?> VarOwner =
        AccessTools.FieldRefAccess<DynamicVar, AbstractModel?>("_owner");
    private static readonly AccessTools.FieldRef<RelicModel, Player?> RelicOwner =
        AccessTools.FieldRefAccess<RelicModel, Player?>("_owner");
    private static readonly AccessTools.FieldRef<PotionModel, Player?> PotionOwner =
        AccessTools.FieldRefAccess<PotionModel, Player?>("_owner");

    /// Relics: block, damage, buffs and debuffs.
    private static readonly HashSet<Type> RelicVarTypes = [typeof(DamageVar), typeof(BlockVar)];

    /// Potions: everything that is an amount of something.
    private static readonly HashSet<Type> PotionVarTypes =
    [
        typeof(DamageVar), typeof(BlockVar), typeof(HealVar), typeof(CardsVar), typeof(EnergyVar),
        typeof(ForgeVar), typeof(SummonVar), typeof(StarsVar), typeof(RepeatVar), typeof(OstyDamageVar),
    ];

    private static readonly ConcurrentDictionary<Type, bool> IsPowerVar = new();

    public static void Postfix(DynamicVar __instance, ref decimal __result)
    {
        if (__result == 0m) return;

        Player? player;
        HashSet<Type> scaledTypes;
        switch (VarOwner(__instance))
        {
            case RelicModel relic when relic is not ScalingRelic:
                player = RelicOwner(relic);
                scaledTypes = RelicVarTypes;
                break;
            case PotionModel potion:
                player = PotionOwner(potion);
                scaledTypes = PotionVarTypes;
                break;
            default:
                return;
        }

        Type varType = __instance.GetType();
        if (!scaledTypes.Contains(varType) && !IsPowerVar.GetOrAdd(varType,
                t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PowerVar<>)))
        {
            return;
        }

        ScalingRelic? scaling = player?.GetRelic<ScalingRelic>();
        if (scaling == null || scaling.Hundredths <= 0) return;

        __result = Math.Round(__result * ScalingSettings.RewardMultiplier(scaling.X), MidpointRounding.AwayFromZero);
    }
}
