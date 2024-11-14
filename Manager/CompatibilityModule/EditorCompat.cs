using BaldiLevelEditor;
using DevTools.Extensions;
using HarmonyLib;
using MTM101BaldAPI;
using NULL.Content;
using NULL.NPCs;
using PlusLevelFormat;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static BaldiLevelEditor.BaldiLevelEditorPlugin;

namespace NULL.Manager.CompatibilityModule;

[ConditionalPatchMod("mtm101.rulerp.baldiplus.leveleditor")]
[HarmonyPatch]
internal class EditorCompat
{
    internal static void AddEditorAssets()
    {
        var m = ModManager.m;
        for (int i = 0; i < 2; i++)
        {
            var p = (i == 1 ? "GLITCH" : string.Empty);
            var obj = StripAllScripts(m.Get<NullNPC>("NULL" + p).gameObject, true);

            var spr = obj.transform.Find("SpriteBase/Sprite");
            if (i == 0)
                spr.localScale += Vector3.one * .5f;
            else
                spr.localScale *= 2;

            characterObjects.Add("NULL" + p, obj);
            Instance.assetMan.Add("UI/NPC_NULL" + p, m.Get<Sprite>("EditorNpc_NULL" + p));
        }
    }
    [HarmonyPatch(typeof(PlusLevelEditor), "Initialize")]
    [HarmonyPostfix]
    static void OnEditorInitialize(PlusLevelEditor __instance)
    {
        __instance.toolCats.Find(x => x.name == "characters").tools.AddRange([new NpcTool("NULL"), new NpcTool("NULLGLITCH")]);
        Shader.SetGlobalFloat("_VertexGlitchIntensity", 0f);
        Shader.SetGlobalFloat("_TileVertexGlitchIntensity", 0f);
        Shader.SetGlobalInt("_ColorGlitching", 0);
        Shader.SetGlobalInt("_SpriteColorGlitching", 0);
    }


    [HarmonyPatch(typeof(PlusLevelEditor), nameof(PlusLevelEditor.LoadTempPlay))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> LoadNullLevel(IEnumerable<CodeInstruction> i) =>
            new CodeMatcher(i).MatchForward(false,
                new TextCodeMatch(OpCodes.Ldloc_0),
                new TextCodeMatch(OpCodes.Ldsfld, "mainGameManager"),
                new TextCodeMatch(OpCodes.Stfld, "manager"))
            .Advance(1)
            .SetInstruction(Transpilers.EmitDelegate(() =>
            {
                if (PlusLevelEditor.Instance.tempPlayLevel.npcSpawns.Any(x => x.type.Contains("NULL")))
                    return ModManager.m.Get<NullPlusManager>("NullPlusMan");
                return mainGameManager;
            }))
            .End()
            .Advance(-1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlusLevelEditor), "tempPlayLevel")))
            .Insert(Transpilers.EmitDelegate((Level level) =>
            {
                bool HasNPC(string npc) => level.npcSpawns.Any(x => x.type == npc);
                if (HasNPC("NULL"))
                    ModManager.NullStyle = true;
                else if (HasNPC("NULLGLITCH"))
                    ModManager.GlitchStyle = true;
            }))
            .InstructionEnumeration();
}
