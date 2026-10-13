using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;
using ScalingMod.ScalingModCode.Extensions;
using ScalingMod.ScalingModCode.Relics;

namespace ScalingMod.ScalingModCode.Rewards;

/// <summary>
/// Optional combat reward. Taking it raises the player's scaling factor x (granting <see cref="ScalingRelic"/> if needed).
/// </summary>
public sealed class ScalingReward : CustomReward
{
    [CustomEnum]
    public static RewardType Scaling;

    /// How much x this reward adds, in hundredths.
    public int Hundredths { get; }

    public ScalingReward(Player player, int hundredths) : base(player)
    {
        Hundredths = hundredths;
    }

    protected override RewardType RewardType => Scaling;

    public override LocString Description
    {
        get
        {
            LocString loc = GetLoc();
            loc.Add("amount", Hundredths / 100m);
            return loc;
        }
    }

    public override bool IsPopulated => true;

    protected override string IconPath => "scaling_relic.png".RelicImagePath();

    public override CreateRewardFromSave<CustomReward> DeserializeMethod => CreateFromSerializable;

    // The amount is stored in the vanilla GoldAmount field, like BaseLib's own custom rewards do.
    public override SerializableReward ToSerializable() => new()
    {
        RewardType = Scaling,
        GoldAmount = Hundredths,
    };

    public static ScalingReward CreateFromSerializable(SerializableReward save, Player player) => new(player, save.GoldAmount);

    public override void Populate()
    {
    }

    protected override async Task<bool> OnSelect()
    {
        ScalingRelic relic = Player.GetRelic<ScalingRelic>() ?? await RelicCmd.Obtain<ScalingRelic>(Player);
        await relic.Gain(Hundredths);
        MainFile.Logger.Info($"Player {Player.NetId} took Scaling, x is now {relic.X}");
        return true;
    }

    public override void MarkContentAsSeen()
    {
    }
}
