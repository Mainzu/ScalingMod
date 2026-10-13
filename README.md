# Scaling

A Slay the Spire 2 mod for runs that want more challenge. After every fight with rewards you can pick up **Scaling**, an optional reward that makes enemies grow exponentially stronger while you grow linearly. Only the most busted runs should reach a scaling of 2.

## What it does

### Earning Scaling

| Fight | Scaling per pickup |
| --- | --- |
| Normal | +0.01 |
| Elite | +0.05 |
| Boss | +0.25 |

Take no attack damage in a fight and two Scaling pickups are offered instead of one. Your scaling lives on the **Scaling** relic, whose tooltip shows your current multipliers.

### Enemies (exponential)

- HP, Block and defensive statuses (Plating, Artifact, Regen, …): **× e^scaling**
- Base attack damage and offensive statuses (Strength, debuffs they apply to you, …): **× 2^(scaling / 2)**

### You (linear)

- Gold, Max HP gains, relic effects and potion effects: **× (1 + scaling)**
- Card reward upgrade chance: **+scaling**. Above 100%, each full 100% is a guaranteed upgrade and the rest is the chance of one more.
- Every whole number of scaling: your relics repeat their pickup effects (shown one by one), and compatible cards can be upgraded one more time.

### Configuration

In the Mod Config menu you can set the starting scaling of new runs and how much each reward is worth. The dev console has a `scaling` command: `scaling add 0.5`, `scaling set 1`, `scaling get`.

### Languages

English, Simplified Chinese and Traditional Chinese. Other languages fall back to English.

## Installation

Subscribe on the Steam Workshop, along with [BaseLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127). In co-op, every player needs the mod with the same settings.

## About this mod

This mod was made with the help of an AI coding assistant (Claude). The Chinese translations (`zhs` Simplified, `zht` Traditional) were also written by AI. They follow the game's official terminology but haven't been reviewed by a native speaker. If a translation is wrong or reads unnaturally, please [open an issue](https://github.com/Mainzu/ScalingMod/issues) or leave a comment on the Steam Workshop page.

## License

Public domain, under [The Unlicense](LICENSE). Use, copy, modify or redistribute it however you like; no credit needed.

---

## Development

Generated from Alchyr's `Alchyr.Sts2.Templates` (Content template, v2.5.2), built on [BaseLib](https://alchyr.github.io/BaseLib-Wiki/).

### Requirements

- .NET 9 SDK (mods target `net9.0`)
- MegaDot 4.5.1 (Mega Crit's Godot fork), needed to export the `.pck` (images, localization). Copy `Directory.Build.props.example` to `Directory.Build.props` (machine-local, gitignored) and set `GodotPath`; the default is `C:/megadot/MegaDot_v4.5.1-stable_mono_win64.exe`.
- BaseLib installed in the game, version >= the `min_version` in `ScalingMod.json`.

### Layout

| Path | Purpose |
| --- | --- |
| `ScalingModCode/MainFile.cs` | Entry point (`[ModInitializer]`): applies Harmony patches, registers the config |
| `ScalingModCode/ScalingSettings.cs` | Scaling formulas |
| `ScalingModCode/ScalingConfig.cs` | Mod Config settings |
| `ScalingModCode/Relics/ScalingRelic.cs` | Stores scaling for the run; gold and upgrade-chance hooks, milestones |
| `ScalingModCode/Rewards/` | The Scaling reward and the model that offers it after fights |
| `ScalingModCode/Patches/` | Enemy scaling, relic/potion scaling, max HP, card upgrades, starting scaling |
| `ScalingModCode/Ui/MilestonePopup.cs` | Popups shown when scaling reaches a whole number |
| `ScalingModCode/ScalingConsoleCmd.cs` | The `scaling` dev console command |
| `ScalingMod/` | Godot resources packed into the `.pck`: `images/`, `localization/<lang>/*.json`, `mod_image.png` |
| `ScalingMod.json` | Mod manifest read by the game (BaseLib `min_version` is auto-synced on build) |

### Build

```bash
dotnet build      # compiles and copies .dll/.pdb/.json to <StS2>/mods/ScalingMod/
dotnet publish    # also exports ScalingMod.pck via MegaDot (needed whenever ScalingMod/ assets change)
```

The game path is auto-detected from Steam; override `Sts2Path` in `Directory.Build.props` if needed. Close the game before building, since it locks the DLL.

Game code lives in `<StS2>/data_sts2_windows_x86_64/sts2.dll`. Decompile it (ILSpy / dotPeek / Rider) to find things to patch.

### Translations

Each language is a folder under `ScalingMod/localization/` (`eng`, `zhs`, `zht`) with the same keys. To add one, copy `eng` to the game's language code (e.g. `jpn`, `kor`) and translate the values; keep the `{placeholders}` and `[color]` tags.

### Publishing to Steam Workshop

Uses Mega Crit's official uploader, [megacrit/sts2-mod-uploader](https://github.com/megacrit/sts2-mod-uploader). The `workshop/` folder is its workspace (Godot ignores it via `.gdignore`).

| File | Purpose |
| --- | --- |
| `workshop/workshop.json` | Title, description (Steam BBCode), visibility, change note, tags, dependencies (BaseLib = `3737335127`) |
| `workshop/image.png` | Workshop thumbnail, must be under 1 MB |
| `workshop/content/` | Files to upload; generated by `prepare.ps1`, not committed |
| `workshop/mod_id.txt` | Written by the uploader on first upload; commit it so updates go to the same item |

1. Bump `version` in `ScalingMod.json` and set `changeNote` in `workshop.json`.
2. Close the game, then stage the files:
   ```
   pwsh workshop/prepare.ps1
   ```
3. With Steam running, upload with the uploader (`ModUploader-win-x64.zip` from its releases page):
   ```
   ModUploader.exe upload -w <path to this repo>/workshop
   ```
4. The first upload is `"visibility": "private"`. Check the page on Steam (you may need to accept the Workshop legal agreement there), then change it to `"public"` and upload again, or switch it on the website.

If you're subscribed to your own item while `mods/ScalingMod` also exists locally, the game loads the local copy and disables the Workshop one when the versions are equal. Delete the local folder to test the Workshop version.
