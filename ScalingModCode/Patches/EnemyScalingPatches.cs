using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using static ScalingMod.ScalingModCode.ScalingSettings;

namespace ScalingMod.ScalingModCode.Patches;

/// <summary>
/// Scales enemy combat stats at their source, so each effect is scaled exactly once:
/// base move damage before Strength etc. is added, move block (not block granted by buffs, which are scaled
/// through the buff amount instead), and status amounts as they are applied.
/// Enemy HP is scaled in <see cref="ScaleEnemyHpPatch"/>.
/// </summary>
public static class EnemyScalingPatches
{
    /// Survival statuses on enemies, scaled by e^x. Starts from the powers the game itself scales in multiplayer
    /// (ShouldScaleInMultiplayer), plus damage caps and powers that reduce damage taken by their owner.
    private static readonly HashSet<Type> DefensivePowers =
    [
        typeof(HardToKillPower), typeof(ColossusPower), typeof(CoveredPower), typeof(GuardedPower),
        typeof(InterceptPower), typeof(SoarPower), typeof(TankPower),
    ];

    /// Offensive statuses on enemies, scaled by 2^(x/2). Powers that add to their owner's damage.
    /// Generators like Ritual are left out: the Strength they grant is scaled when applied.
    private static readonly HashSet<Type> OffensivePowers =
    [
        typeof(StrengthPower), typeof(VigorPower), typeof(AccuracyPower), typeof(DoubleDamagePower),
        typeof(LethalityPower), typeof(TrackingPower), typeof(ThornsPower),
    ];

    private static bool IsDefensive(PowerModel power) => power.ShouldScaleInMultiplayer || DefensivePowers.Contains(power.GetType());

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
    public static class BaseDamage
    {
        public static void Prefix(IRunState runState, Creature? dealer, ref decimal damage, ValueProp props)
        {
            if (dealer?.Side != CombatSide.Enemy || !props.HasFlag(ValueProp.Move)) return;
            decimal x = TotalX(runState);
            if (x > 0m) damage *= EnemyOffenseMultiplier(x);
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyBlock))]
    public static class MoveBlock
    {
        public static void Prefix(ICombatState combatState, Creature target, ref decimal block, ValueProp props)
        {
            if (target.Side != CombatSide.Enemy || !props.IsPoweredCardOrMonsterMoveBlock()) return;
            decimal x = TotalX(combatState.RunState);
            if (x > 0m) block *= EnemyDefenseMultiplier(x);
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyPowerAmountReceived))]
    public static class StatusAmount
    {
        public static void Postfix(ICombatState combatState, PowerModel canonicalPower, Creature target, Creature? giver, ref decimal __result)
        {
            decimal x = TotalX(combatState.RunState);
            if (x <= 0m) return;

            decimal multiplier = MultiplierFor(canonicalPower, target, giver, __result);
            if (multiplier == 1m) return;
            __result = Math.Round(__result * multiplier, MidpointRounding.AwayFromZero);

            decimal MultiplierFor(PowerModel power, Creature to, Creature? from, decimal amount)
            {
                // Enemies gaining buffs. Negative amounts (e.g. the player removing Strength) are left alone.
                if (to.Side == CombatSide.Enemy && amount > 0m)
                {
                    if (IsDefensive(power)) return EnemyDefenseMultiplier(x);
                    if (OffensivePowers.Contains(power.GetType())) return EnemyOffenseMultiplier(x);
                    return 1m;
                }
                // Enemies weakening players: debuffs, or negative buffs such as Strength loss.
                if (to.Side == CombatSide.Player && from?.Side == CombatSide.Enemy &&
                    (power.Type == PowerType.Debuff || amount < 0m))
                {
                    return EnemyOffenseMultiplier(x);
                }
                return 1m;
            }
        }
    }
}
