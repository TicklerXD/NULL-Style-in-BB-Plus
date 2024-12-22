using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using NULL.Manager;
using System.IO;

namespace NULL;

[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("mtm101.rulerp.baldiplus.levelloader", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("pixelguy.pixelmodding.baldiplus.bbextracontent", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("mtm101.rulerp.baldiplus.leveleditor", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("tickler.asau.baldiplus.improvedlevelloader", BepInDependency.DependencyFlags.SoftDependency)]

[BepInPlugin(ModInfo.ID, ModInfo.NAME, ModInfo.VERSION)]
public class BasePlugin : BaseUnityPlugin
{
    internal readonly static Harmony harmony = new(ModInfo.ID);
    static string _modPath;
    public static string ModPath { get => _modPath; }
    internal static ConfigEntry<bool> characters;
    internal static ConfigEntry<bool> darkAtmosphere;
    internal static ConfigEntry<int> nullHealth;

    private void Awake()
    {
        ModManager.plug = this;
        _modPath = AssetLoader.GetModPath(this);
        characters = Config.Bind("Null Style settings", "Enable another characters", false, "Setting this \"true\" will enable other characters on the floor except Null/Red Baldloon");
        darkAtmosphere = Config.Bind("Null Style settings", "Enable the dark atmosphere", false, "Setting this \"true\" will enable the dark atmosphere, which makes the level darker and more creepy");
        nullHealth = Config.Bind("Null Style settings", "Health", 10, "Setting a custom amount of null's health");
        harmony.PatchAllConditionals();
        LoadingEvents.RegisterOnAssetsLoaded(Info, ModManager.LoadContent(), false);
        LoadingEvents.RegisterOnAssetsLoaded(Info, () => ModManager.TryRunMethod(ModManager.LoadScenes), true);
        AssetLoader.LocalizationFromFile(Path.Combine(ModPath, "Language", "English", "SubtitlesEn.json"), Language.English);
    }
    public static void RePatch()
    {
        harmony.UnpatchSelf();
        harmony.PatchAllConditionals();
    }
}

static class ModInfo
{
    public const string ID = "tickler.asau.baldiplus.null";
    public const string NAME = "NULL";
    public const string VERSION = "1.2.5";
}