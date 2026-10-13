using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using ScalingMod.ScalingModCode.Ui;
using static ScalingMod.ScalingModCode.ScalingSettings;

namespace ScalingMod.ScalingModCode.Relics;

/// <summary>
/// Holds the player's scaling factor x for the run and applies its effects.
/// Most effects live in the Patches folder; this relic handles the ones the game exposes as relic hooks.
/// </summary>
[Pool(typeof(EventRelicPool))]
public sealed class ScalingRelic : ScalingModRelic
{
    private int _hundredths;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => true;

    /// Shown in hundredths: a counter of 125 means a scaling of 1.25. The tooltip shows the exact value.
    public override int DisplayAmount => Hundredths;

    [SavedProperty]
    public int Hundredths
    {
        get => _hundredths;
        set
        {
            AssertMutable();
            _hundredths = value;
            UpdateDescriptionVars();
            InvokeDisplayAmountChanged();
        }
    }

    public decimal X => Hundredths / 100m;

    protected override IEnumerable<DynamicVar> CanonicalVars => DescriptionValues(0)
        .Select(v => (DynamicVar)new StringVar(v.Name, v.Value));

    // Text lives in ScalingMod/localization/<lang>/relics.json. Not the in-code Localization property:
    // BaseLib writes that over the loaded table, which would replace every translation with English.

    private static IEnumerable<(string Name, string Value)> DescriptionValues(int hundredths)
    {
        decimal x = hundredths / 100m;
        yield return ("Scaling", $"{x:0.00}");
        yield return ("EnemyDefense", $"×{EnemyDefenseMultiplier(x):0.00}");
        yield return ("EnemyOffense", $"×{EnemyOffenseMultiplier(x):0.00}");
        yield return ("Rewards", $"×{RewardMultiplier(x):0.00}");
        yield return ("UpgradeChance", $"+{x * 100m:0}%");
        yield return ("ExtraUpgrades", $"+{hundredths / 100}");
        yield return ("NextMilestone", $"{hundredths / 100 + 1}.00");
    }

    private void UpdateDescriptionVars()
    {
        foreach ((string name, string value) in DescriptionValues(Hundredths))
        {
            if (DynamicVars[name] is StringVar var) var.StringValue = value;
        }
    }

    // Only obtainable through the Scaling reward, never from random relic drops.
    public override bool IsAllowed(IRunState runState) => false;

    /// Adds to x and, for every whole number crossed, repeats the pickup effects of the player's other relics.
    public async Task Gain(int hundredths)
    {
        int before = Hundredths / 100;
        Hundredths += hundredths;
        Flash();
        // Popups only on the machine of the player who took the reward; other clients just apply the effects.
        bool showPopups = LocalContext.IsMe(Owner) && !TestMode.IsOn;
        for (int level = before + 1; level <= Hundredths / 100; level++)
        {
            List<RelicModel> relics = Owner.Relics.Where(r => r != this && r.HasUponPickupEffect).ToList();
            if (showPopups) await MilestonePopup.ShowMilestone(level, relics.Count);

            for (int i = 0; i < relics.Count; i++)
            {
                if (showPopups) await MilestonePopup.ShowRelic(relics[i], i + 1, relics.Count);
                relics[i].Flash();
                await relics[i].AfterObtained();
            }
        }
    }

    public override decimal ModifyGoldGained(Player player, decimal amount)
    {
        if (player != Owner) return amount;
        return amount * RewardMultiplier(X);
    }

    public override decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
    {
        if (player != Owner) return odds;
        return odds + X;
    }
}
