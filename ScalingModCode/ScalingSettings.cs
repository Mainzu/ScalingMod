using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using ScalingMod.ScalingModCode.Relics;

namespace ScalingMod.ScalingModCode;

/// <summary>
/// Tuning values for Scaling. The scaling factor x is tracked in hundredths (an int) to avoid rounding drift;
/// see <see cref="ScalingRelic.Hundredths"/>.
/// </summary>
public static class ScalingSettings
{
    /// How much x (in hundredths) one Scaling pickup adds, by the room it was earned in. Set in <see cref="ScalingConfig"/>.
    public static int GainFor(RoomType roomType) => ScalingConfig.ToHundredths(roomType switch
    {
        RoomType.Elite => ScalingConfig.EliteFightReward,
        RoomType.Boss => ScalingConfig.BossFightReward,
        _ => ScalingConfig.NormalFightReward,
    });

    /// Enemy HP, Block and survival statuses.
    public static decimal EnemyDefenseMultiplier(decimal x) => (decimal)Math.Exp((double)x);

    /// Enemy damage and offensive statuses.
    public static decimal EnemyOffenseMultiplier(decimal x) => (decimal)Math.Pow(2, 0.5 * (double)x);

    /// Player rewards: gold, max HP gain, relic and potion effects.
    public static decimal RewardMultiplier(decimal x) => 1m + x;

    /// Combined x across all players; in co-op every player's Scaling strengthens the shared enemies.
    public static decimal TotalX(IRunState runState) =>
        runState.Players.Sum(p => p.GetRelic<ScalingRelic>()?.X ?? 0m);
}
