using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace ScalingMod.ScalingModCode.Ui;

/// <summary>
/// Popups shown when scaling reaches a whole number: one announcing the milestone, then one per relic whose pickup
/// effect is about to repeat. Uses the game's own confirmation popup so it matches the UI.
/// </summary>
public static class MilestonePopup
{
    private const string Table = "gameplay_ui";

    public static Task ShowMilestone(int level, int relicCount)
    {
        LocString title = Loc("MILESTONE.title");
        title.Add("level", level);

        LocString body = Loc(relicCount > 0 ? "MILESTONE.body" : "MILESTONE.bodyNoRelics");
        body.Add("limit", 1 + level);
        body.Add("relics", relicCount);

        return Show(title, body, Loc("MILESTONE.continue"));
    }

    public static Task ShowRelic(RelicModel relic, int index, int count)
    {
        LocString title = Loc("MILESTONE_RELIC.title");
        title.Add("relic", relic.Title.GetFormattedText());
        // Not "index": SmartFormat reserves it for list positions and resolves it to -1 here.
        title.Add("position", index);
        title.Add("total", count);

        LocString body = Loc("MILESTONE_RELIC.body");
        body.Add("icon", relic.IconPath);
        body.Add("description", relic.DynamicDescription.GetFormattedText());

        return Show(title, body, Loc("MILESTONE_RELIC.trigger"));
    }

    private static async Task Show(LocString title, LocString body, LocString button)
    {
        NModalContainer? container = NModalContainer.Instance;
        if (container == null) return;

        // Confirming a popup resumes our code inside the button handler, before the popup clears the modal container,
        // and the container refuses new modals while one is open. Wait for it to be free first.
        for (int frame = 0; container.OpenModal != null && frame < 120; frame++)
        {
            await container.ToSignal(container.GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        NGenericPopup? popup = NGenericPopup.Create();
        if (popup == null) return;

        container.Add(popup);
        if (container.OpenModal != popup)
        {
            MainFile.Logger.Warn("Could not show Scaling popup: another modal is open");
            popup.QueueFree();
            return;
        }

        Task confirmed = popup.WaitForConfirmation(body, title, null, button);
        PopIn(popup.GetNodeOrNull<Control>("VerticalPopup"));
        await confirmed;
    }

    /// Small scale-and-fade entrance so each step reads as a new beat.
    private static void PopIn(Control? panel)
    {
        if (panel == null) return;
        panel.PivotOffset = panel.Size / 2;
        panel.Scale = new Vector2(0.75f, 0.75f);
        panel.Modulate = new Color(1, 1, 1, 0);

        Tween tween = panel.CreateTween().SetParallel();
        tween.TweenProperty(panel, "scale", Vector2.One, 0.35).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(panel, "modulate:a", 1f, 0.2);
    }

    private static LocString Loc(string key) => new(Table, $"{MainFile.ModId.ToUpperInvariant()}-{key}");
}
