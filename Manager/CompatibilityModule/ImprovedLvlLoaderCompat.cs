using PlusLevelFormat;
using System.IO;
using System.Linq;

namespace NULL.Manager.CompatibilityModule;

internal class ImprovedLvlLoaderCompat
{
    public static bool CustomLevelCheck(bool glitchStyle = false)
    {
        bool CustomLevelCheck_Internal()
        {
            var lvlPath = ImprovedCustomLevelsLoader.LoadLevelFromAnyModes.customLevelPath;
            if (!string.IsNullOrEmpty(lvlPath))
            {
                using var br = new BinaryReader(File.OpenRead(lvlPath));
                var lvl = br.ReadLevel();
                if (!lvl.npcSpawns.Any(x => x.type == (!glitchStyle ? "NULL" : "NULLGLITCH")))
                {
                    PixelInternalAPI.ResourceManager.RaisePopup(ModManager.plug.Info, $"An error occurred when loading the level:\r\n\r\nMissing NULL" + (glitchStyle ? "GLITCH" : string.Empty) + " character at the level.");
                    return true;
                }
            }
            return false;
        }
        return Plugins.IsImprovedLoader && CustomLevelCheck_Internal();
        
    }
}
