﻿using BBTimes.CustomContent.Misc;
using BBTimes.ModPatches;
using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using NULL.Content;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NULL.Manager.CompatibilityModule;

[CompatPatchBBTimes]
[HarmonyPatch]
internal class BBTimesCompat
{
    internal static float _fixedAnger;
    internal static Coroutine angerCoroutine;
    public class CompatPatchBBTimes : ConditionalPatchNULL { public override bool ShouldPatch() => base.ShouldPatch() && Plugins.IsTimes; }

    [HarmonyPatch(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), [typeof(IEnumerator)])]
    [HarmonyPostfix]
    static void GetAngerCoroutine(IEnumerator routine, Coroutine __result)
    {
        if (routine.ToString().Contains("InfiniteAnger"))
            angerCoroutine = __result;
    }

    [HarmonyPatch(typeof(BossManager), nameof(BossManager.StartBossIntro))]
    [HarmonyPrefix]
    static void OnStartBossIntro(BossManager __instance)
    {
        var n = __instance.nullNpc;
        try
        {
            n.StopCoroutine(angerCoroutine); // Stop infinite anger coroutine
        }
        catch { }
        n.SetAnger(_fixedAnger);
        Singleton<MusicManager>.Instance.StopFile();
       // Singleton<MusicManager>.Instance.StopAllCoroutines();
    }
    [HarmonyPatch(typeof(NullPlusManager), nameof(NullPlusManager.ElevatorClosed))]
    [HarmonyPostfix]
    static void StoreAngerBeforeRage(NullPlusManager __instance, int ___elevatorsClosed, int ___elevatorsToClose)
    {
        if (___elevatorsClosed >= 3 && ___elevatorsToClose == 0)
            _fixedAnger = __instance.nullNpc.anger;
    }

    [HarmonyPatch(typeof(MainGameManagerPatches), nameof(MainGameManagerPatches.REDAnimation))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> REDAnimation_Enable(IEnumerable<CodeInstruction> instructs)
    {        
        var list = new List<CodeInstruction>(instructs);
        bool skipInstructions = false;

        var m = new CodeMatcher(list)
            .MatchForward(false,
            new TextCodeMatch(OpCodes.Ldarg_1),
            new TextCodeMatch(OpCodes.Callvirt, "get_name"),
            new TextCodeMatch(OpCodes.Ldstr, "Lvl3"))
            .RemoveInstructions(4)
            .Insert(Transpilers.EmitDelegate(() => Singleton<CoreGameManager>.Instance.sceneObject.name.EndsWith("F3")));

        list = new List<CodeInstruction>(m.InstructionEnumeration());

        for (int i=0;i<list.Count;i++)
        {
            if (list[i].opcode == OpCodes.Ldsfld && list[i].OperandIs("endGameAnimation"))
            {
                if (list[i+ 1].opcode.ToString().ToLower().Contains("brfalse") &&
                list[i + 2].opcode == OpCodes.Ldarg_1)
                {
                    skipInstructions = true;
                }
            }
            if (skipInstructions && list[i].opcode == OpCodes.Ret)
            {
                skipInstructions = false;
                continue;
            }
            if (!skipInstructions)
            {
                yield return list[i];
            }
        }
    }

    [HarmonyPatch(typeof(FocusedStudent), nameof(FocusedStudent.Disturbed))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> NullReactionToDistrubingStudent(IEnumerable<CodeInstruction> i) => new CodeMatcher(i)
        .MatchForward(true,
        new TextCodeMatch(OpCodes.Ldarg_0),
        new TextCodeMatch(OpCodes.Ldc_I4_1),
        new TextCodeMatch(OpCodes.Stfld, "shaking"),
        new TextCodeMatch(OpCodes.Ldarg_0))
        .InsertAndAdvance(Transpilers.EmitDelegate(() =>
        {
            if (OptionsManager.Characters || NullPlusManager.instance.nullNpc.isGlitch || ModManager.GlitchStyle) return;

            var n = NullPlusManager.instance.nullNpc;
            var aud = n.AudMan;
            aud.FlushQueue(true);
            aud.audioDevice.clip = ModManager.m.Get<SoundObject>("Null_PreBoss_Start").soundClip;
            aud.audioDevice.time = 7.25f;
            aud.audioDevice.Play();
            n.GetAngry(69f);
            n.slideMode = true;
            ExtraVariables.ec.MakeNoise(ExtraVariables.pm.transform.position, 69);
        }))
        .InstructionEnumeration();
}