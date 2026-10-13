using System.Globalization;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using ScalingMod.ScalingModCode.Relics;

namespace ScalingMod.ScalingModCode;

/// <summary>
/// Dev console command. The game picks up AbstractConsoleCmd subclasses in mods automatically.
/// </summary>
public class ScalingConsoleCmd : AbstractConsoleCmd
{
    private static readonly string[] Actions = ["add", "set", "get"];

    public override string CmdName => "scaling";

    public override string Args => "[add|set|get] <amount:decimal>";

    public override string Description =>
        "View or change your scaling. 'add' works like taking Scaling pickups (milestones trigger), 'set' changes it silently.";

    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (issuingPlayer == null || !RunManager.Instance.IsInProgress)
        {
            return new CmdResult(success: false, "A run does not appear to be in progress");
        }

        ScalingRelic? relic = issuingPlayer.GetRelic<ScalingRelic>();
        string action = args.Length > 0 ? args[0].ToLowerInvariant() : "get";
        if (action == "get")
        {
            return new CmdResult(success: true, $"Scaling is {relic?.X ?? 0m:0.00}");
        }
        if (action is not ("add" or "set"))
        {
            return new CmdResult(success: false, $"Unknown action '{args[0]}'. Use add, set or get.");
        }
        if (args.Length < 2 || !decimal.TryParse(args[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
        {
            return new CmdResult(success: false, "Second argument must be a number, e.g. 'scaling add 0.25'.");
        }

        int hundredths = (int)Math.Round(amount * 100m);
        int current = relic?.Hundredths ?? 0;
        int target = Math.Max(0, action == "add" ? current + hundredths : hundredths);
        return new CmdResult(Apply(issuingPlayer, action, target), success: true, $"Scaling set to {target / 100m:0.00}");
    }

    private static async Task Apply(Player player, string action, int target)
    {
        ScalingRelic relic = player.GetRelic<ScalingRelic>() ?? await RelicCmd.Obtain<ScalingRelic>(player);
        if (action == "add" && target > relic.Hundredths)
        {
            await relic.Gain(target - relic.Hundredths);
        }
        else
        {
            relic.Hundredths = target;
        }
    }

    public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
    {
        if (args.Length <= 1)
        {
            return CompleteArgument(Actions, [], args.Length == 1 ? args[0] : "");
        }
        return base.GetArgumentCompletions(player, args);
    }
}
