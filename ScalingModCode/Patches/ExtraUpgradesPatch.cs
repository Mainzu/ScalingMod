using System.Collections.Concurrent;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using ScalingMod.ScalingModCode.Relics;

namespace ScalingMod.ScalingModCode.Patches;

/// <summary>
/// Each whole number of x lets compatible cards be upgraded one more time.
/// Repeating a card's OnUpgrade stacks naturally (DynamicVar.UpgradeValueBy adds, cost bottoms out at 0, keyword
/// changes are idempotent) and saves replay OnUpgrade once per level, so only the cap needs raising.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.MaxUpgradeLevel), MethodType.Getter)]
public static class ExtraUpgradesPatch
{
    private static readonly AccessTools.FieldRef<CardModel, Player?> OwnerRef =
        AccessTools.FieldRefAccess<CardModel, Player?>("_owner");

    private static readonly ConcurrentDictionary<Type, bool> Compatible = new();

    /// Set while a card is rebuilt from a save, before its owner's relics are guaranteed to be loaded.
    [ThreadStatic] private static bool _loadingCard;

    public static void Postfix(CardModel __instance, ref int __result)
    {
        if (__result != 1 || !IsCompatible(__instance.GetType())) return;
        if (_loadingCard)
        {
            __result = int.MaxValue;
            return;
        }

        ScalingRelic? relic = OwnerRef(__instance)?.GetRelic<ScalingRelic>();
        if (relic != null) __result += relic.Hundredths / 100;
    }

    /// A card is compatible if its upgrade raises at least one number, so repeating it keeps doing something.
    /// Cards whose upgrade only changes cost or keywords are left alone.
    public static bool IsCompatible(Type cardType) => Compatible.GetOrAdd(cardType, type =>
    {
        MethodInfo? onUpgrade = AccessTools.DeclaredMethod(type, "OnUpgrade");
        if (onUpgrade == null) return false;
        try
        {
            return PatchProcessor.GetOriginalInstructions(onUpgrade).Any(i =>
                i.operand is MethodInfo { Name: nameof(DynamicVar.UpgradeValueBy) });
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Could not inspect {type.Name}.OnUpgrade: {e.Message}");
            return false;
        }
    });

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
    public static class LoadingCardPatch
    {
        public static void Prefix() => _loadingCard = true;

        public static void Finalizer() => _loadingCard = false;
    }
}
