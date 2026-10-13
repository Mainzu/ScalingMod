using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ScalingMod.ScalingModCode.Rewards;

/// <summary>
/// Always-active model that adds Scaling to every combat's rewards: one pickup, or two if the player took no
/// attack damage during the fight.
/// </summary>
public sealed class ScalingRewardOffer : CustomSingletonModel
{
    private readonly HashSet<Player> _hitByAttack = new();

    public ScalingRewardOffer() : base(HookType.Run)
    {
    }

    public override Task BeforeCombatStart()
    {
        _hitByAttack.Clear();
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target.Player != null && result.UnblockedDamage > 0 && props.IsPoweredAttack())
        {
            _hitByAttack.Add(target.Player);
        }
        return Task.CompletedTask;
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (room is not CombatRoom) return false;
        // The final boss has no rewards screen worth scaling for.
        if (room.RoomType == RoomType.Boss && player.RunState.CurrentActIndex >= player.RunState.Acts.Count - 1) return false;

        int gain = ScalingSettings.GainFor(room.RoomType);
        if (gain <= 0) return false;
        int count = _hitByAttack.Contains(player) ? 1 : 2;
        for (int i = 0; i < count; i++)
        {
            rewards.Add(new ScalingReward(player, gain));
        }
        return true;
    }
}
