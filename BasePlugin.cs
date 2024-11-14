using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using NULL.Manager;

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
    private void Awake()
    {
        ModManager.plug = this; 
        _modPath = AssetLoader.GetModPath(this);
        harmony.PatchAllConditionals();
        LoadingEvents.RegisterOnAssetsLoaded(Info, ModManager.LoadContent(), false);
        LoadingEvents.RegisterOnAssetsLoaded(Info, () => ModManager.TryRunMethod(ModManager.LoadScenes), true);
        CustomOptionsCore.OnMenuInitialize += OptionsManager.OnMenu;
        CustomOptionsCore.OnMenuClose += OptionsManager.Save;
        ModdedSaveSystem.AddSaveLoadAction(this, (bool isSave, string allocatedPath) =>
        {
            if (isSave)
            {
                OptionsManager.SaveOptions();
                return;
            }
            OptionsManager.LoadOptions();
        });       
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
    public const string VERSION = "1.2.4";
}