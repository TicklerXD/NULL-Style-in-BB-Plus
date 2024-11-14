using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using NULL.Content;
using NULL.Manager;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using static DevTools.ExtraVariables;

namespace NULL.ModPatches.Fixes;

[ConditionalPatchNULL]
[HarmonyPatch]
internal class MinorFixes
{
    [HarmonyPatch(typeof(Baldi), "Praise")]
    [HarmonyPrefix]
    static bool NoPraise() => false;

    [HarmonyPatch(typeof(ElevatorScreen), nameof(ElevatorScreen.UpdateFloorDisplay))]
    [HarmonyPostfix]
    static void SetNormalSizeOfFloorText(TMP_Text ___floorText)
    {
         if (Core.sceneObject.levelTitle.Length < 4) return;

        ___floorText.fontSize -= 4;
        ___floorText.autoSizeTextContainer = true;
        ___floorText.rectTransform.anchoredPosition -= Vector2.up * 2;
    }

    [HarmonyPatch(typeof(EnvironmentController), nameof(EnvironmentController.Awake))]
    [HarmonyPostfix]
    static void AngerNullOnSpawn(EnvironmentController __instance)
    {
        __instance.angerOnSpawn = true;
        __instance.npcSpawnBufferRadius = OptionsManager.Characters ? 50 : 40;
    }

    [HarmonyPatch(typeof(ITM_PrincipalWhistle), "Use")]
    [HarmonyPostfix]
    static void NullWhistleReaction() => NullPlusManager.instance.nullNpc.GetAngry(169f);

    [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.CloseMap))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> CloseMapFix(IEnumerable<CodeInstruction> i) => new CodeMatcher(i)
        .MatchForward(true,
        new TextCodeMatch(OpCodes.Ldarg_0),
        new TextCodeMatch(OpCodes.Ldc_I4_0),
        new TextCodeMatch(OpCodes.Call, "GetHud"),
        new TextCodeMatch(OpCodes.Ldc_I4_0),
        new TextCodeMatch(OpCodes.Callvirt, "Hide"))
        .Advance(-1)
        .SetInstruction(Transpilers.EmitDelegate(() => BossManager.Instance != null && (BossManager.Instance.BossActive || BossManager.Instance.bossTransitionWaiting) ? 1 : 0))
        .InstructionEnumeration();

    [ConditionalPatchNULL]
    [HarmonyPatch(typeof(BaldiTV))]
    internal class BaldiTVFixes
    {
        // Yeah, that fucking transpiler patch that broke the mod for some people

        /*[HarmonyPatch(typeof(BaldiTV), nameof(BaldiTV.AnnounceEvent))]
        [HarmonyPatch(typeof(BaldiTV), nameof(BaldiTV.Speak))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> NoEventsAnnounces(IEnumerable<CodeInstruction> i) =>
        new CodeMatcher(i)
        .MatchForward(false,
        new TextCodeMatch(OpCodes.Ldc_R4, "0,25"),
        new TextCodeMatch(OpCodes.Call, "Static"),
        new TextCodeMatch(OpCodes.Call, "QueueEnumerator"),
        new TextCodeMatch(OpCodes.Ldarg_0))
        .Set(OpCodes.Ldc_R4, 5f)
        .Advance(3)
        .RemoveInstructions(10)
        .InstructionEnumeration();*/

        [HarmonyPatch(typeof(BaldiTV), nameof(BaldiTV.AnnounceEvent))]
        [HarmonyPatch(typeof(BaldiTV), nameof(BaldiTV.Speak))]
        [HarmonyPrefix]
        static bool NoEventsAnnounces(BaldiTV __instance, bool ___busy)
        {
            if (!___busy)
            {
                __instance.QueueEnumerator(__instance.Exclamation(2.5f));
            }
            __instance.QueueEnumerator(__instance.Static(5f));
            return false;
        }

        [HarmonyPatch(nameof(BaldiTV.Static), MethodType.Enumerator)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SetNullOnTV(IEnumerable<CodeInstruction> i) =>
            new CodeMatcher(i)
            .MatchForward(true,
            new TextCodeMatch(OpCodes.Ldloc_1),
            new TextCodeMatch(OpCodes.Ldfld, "staticObject"),
            new TextCodeMatch(OpCodes.Ldc_I4_1),
            new TextCodeMatch(OpCodes.Callvirt, "SetActive"))
         .Advance(1)
        .Insert(
           new CodeInstruction(OpCodes.Ldloc_1),
           new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BaldiTV), nameof(BaldiTV.baldiImage))),
           new CodeInstruction(OpCodes.Ldc_I4_1),
           new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(Behaviour), nameof(Behaviour.enabled))))
        .InstructionEnumeration();
    }
}
