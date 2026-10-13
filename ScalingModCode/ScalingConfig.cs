using BaseLib.Config;

namespace ScalingMod.ScalingModCode;

/// <summary>
/// In-game settings (Mod Config menu). Values are x, converted to hundredths where used.
/// </summary>
[ConfigHoverTipsByDefault]
public class ScalingConfig : SimpleModConfig
{
    [ConfigSection("StartSection")]
    [ConfigSlider(0.0, 3.0, 0.01, Format = "{0:0.00}")]
    public static double StartingScaling { get; set; } = 0.0;

    [ConfigSection("RewardSection")]
    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double NormalFightReward { get; set; } = 0.01;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double EliteFightReward { get; set; } = 0.05;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double BossFightReward { get; set; } = 0.25;

    public static int ToHundredths(double x) => (int)Math.Round(x * 100.0);
}
