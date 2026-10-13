using Godot;

namespace ScalingMod.ScalingModCode.Extensions;

//Utilities to get asset paths.
public static class StringExtensions
{
    private const string FallbackRelic = "scaling_relic.png";

    public static string RelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", FallbackRelic);
    }

    public static string BigRelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "big", FallbackRelic);
    }
}
