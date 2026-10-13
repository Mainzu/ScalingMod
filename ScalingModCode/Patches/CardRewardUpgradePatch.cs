using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace ScalingMod.ScalingModCode.Patches;

/// <summary>
/// Card reward upgrade chances above 100%: every full 100% is a guaranteed upgrade, and the remainder is the chance of
/// one more. The vanilla roll already performs the first upgrade whenever the chance is at least 100%.
/// Upgrades stop at the card's upgrade cap (see <see cref="ExtraUpgradesPatch"/>).
/// </summary>
public static class CardRewardUpgradePatch
{
    /// Final upgrade chance of the roll in progress, after every modifier has been applied.
    [ThreadStatic] private static decimal? _finalOdds;

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyCardRewardUpgradeOdds))]
    public static class CaptureOdds
    {
        public static void Postfix(decimal __result) => _finalOdds = __result;
    }

    [HarmonyPatch(typeof(CardFactory), "RollForUpgrade",
        [typeof(Player), typeof(CardModel), typeof(decimal), typeof(Rng)])]
    public static class ExtraUpgrades
    {
        public static void Prefix() => _finalOdds = null;

        public static void Postfix(CardModel card, Rng rng)
        {
            decimal odds = _finalOdds ?? 0m;
            _finalOdds = null;
            if (odds <= 1m) return;

            int guaranteed = (int)Math.Floor(odds);
            decimal remainder = odds - guaranteed;
            int extra = guaranteed - 1;
            if (remainder > 0m && (decimal)rng.NextFloat() < remainder) extra++;

            for (int i = 0; i < extra && card.IsUpgradable; i++)
            {
                CardCmd.Upgrade(card);
            }
        }
    }
}
