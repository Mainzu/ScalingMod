using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using ScalingMod.ScalingModCode.Relics;

namespace ScalingMod.ScalingModCode.Patches;

/// <summary>
/// Gives every player the Scaling relic at the configured starting x when a new run is created.
/// </summary>
[HarmonyPatch(typeof(RunState), nameof(RunState.CreateForNewRun))]
public static class StartingScalingPatch
{
    public static void Postfix(IReadOnlyList<Player> players)
    {
        int start = ScalingConfig.ToHundredths(ScalingConfig.StartingScaling);
        if (start <= 0) return;

        foreach (Player player in players)
        {
            if (player.GetRelic<ScalingRelic>() != null) continue;
            ScalingRelic relic = (ScalingRelic)ModelDb.Relic<ScalingRelic>().ToMutable();
            relic.Hundredths = start;
            player.AddRelicInternal(relic, silent: true);
        }
    }
}
