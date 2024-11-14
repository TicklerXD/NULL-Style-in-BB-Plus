using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using NULL.Content;
using NULL.Manager;
using System.Collections;
using UnityEngine;
using static DevTools.ExtraVariables;


namespace NULL.ModPatches;

[ConditionalPatchNULL]
[HarmonyPatch(typeof(CoreGameManager))]
internal class CoreGamePatcher
{
    [HarmonyPatch("ReturnToMenu")]
    [HarmonyPrefix]
    static void ReturnToMenu_Fix()
    {
        ModManager.NullStyle = false;
        Reset();
        BossManager.Instance?.RemoveAllProjectiles();
        Singleton<MusicManager>.Instance.KillMidi();
        try { Object.Destroy(Singleton<CoreGameManager>.Instance.GetHud(0).BaldiTv.gameObject.GetComponent<CustomComponents.NullTV>()); }
        catch {}
    }
    [HarmonyPatch("Quit")]
    [HarmonyPrefix]
    static bool TryQuit()
    {
        var nullNpc = NullPlusManager.instance.nullNpc;

        if (!(nullNpc != null && !nullNpc.Hidden && !nullNpc.IsPaused() && !nullNpc.isGlitch 
            && Singleton<BaseGameManager>.Instance.foundNotebooks >= 2 && Random.Range(0, 10).Equals(0))) return true; // 10% chance Null won't let you leave, lol

#if CHEAT
        if (Singleton<PlayerFileManager>.Instance.fileName == "Test") return true;
#endif

        Core.SetLives(0);
        nullNpc.transform.position = pm.transform.position + pm.transform.forward;
        Core.Pause(false);
        return false;
    }

    [HarmonyPatch("EndGame")]
    [HarmonyPrefix]
    private static bool EndGame_Prefix(CoreGameManager __instance, Transform player, Baldi baldi)
    {
        IEnumerator EpicEndSequence()
        {
            try { Object.Destroy(canvas.gameObject); }
            catch { }
            __instance.GetHud(0).Hide(true);
            __instance.audMan.Play("NullEnd");

            float time = 0f;
            float glitchRate = 0.5f;
            Shader.SetGlobalInt("_ColorGlitching", 1);
            if (Singleton<PlayerFileManager>.Instance.reduceFlashing)
            {
                Shader.SetGlobalInt("_ColorGlitchVal", Random.Range(0, 4096));
            }
            yield return null;
            while (time <= 5f)
            {
                time += Time.unscaledDeltaTime * 0.5f;
                Shader.SetGlobalFloat("_VertexGlitchSeed", Random.Range(0f, 1000f));
                Shader.SetGlobalFloat("_TileVertexGlitchSeed", Random.Range(0f, 1000f));
                Singleton<InputManager>.Instance.Rumble(time / 5f, 0.05f);
                if (!Singleton<PlayerFileManager>.Instance.reduceFlashing)
                {
                    glitchRate -= Time.unscaledDeltaTime;
                    Shader.SetGlobalFloat("_VertexGlitchIntensity", Mathf.Pow(time, 2.2f));
                    Shader.SetGlobalFloat("_TileVertexGlitchIntensity", Mathf.Pow(time, 2.2f));
                    Shader.SetGlobalFloat("_ColorGlitchPercent", time * 0.05f);
                    if (glitchRate <= 0f)
                    {
                        Shader.SetGlobalInt("_ColorGlitchVal", Random.Range(0, 4096));
                        Singleton<InputManager>.Instance.SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
                        glitchRate = 0.55f - time * 0.1f;
                    }
                }
                else
                {
                    Shader.SetGlobalFloat("_ColorGlitchPercent", time * 0.25f);
                    Shader.SetGlobalFloat("_VertexGlitchIntensity", time * 2f);
                    Shader.SetGlobalFloat("_TileVertexGlitchIntensity", time * 2f);
                }
                yield return null;
            }
            GameOver();
            ClearEffects();
        }
        if (baldi.gameObject.name.Contains("NULL"))
        {
            Time.timeScale = 0f;
            Singleton<MusicManager>.Instance.StopMidi();
            __instance.disablePause = true;
            __instance.GetCamera(0).UpdateTargets(baldi.transform, 0);
            __instance.GetCamera(0).offestPos = (player.position - baldi.transform.position).normalized * 2f + Vector3.up * (baldi.gameObject.name.Contains("GLITCH") ? 1 : 1.25f);
            __instance.GetCamera(0).SetControllable(false);
            __instance.GetCamera(0).matchTargetRotation = false;
            __instance.audMan.volumeModifier = 0.6f;
            __instance.StartCoroutine(EpicEndSequence());
            Singleton<InputManager>.Instance.Rumble(1f, 2f);
            return false;
        }
        return true;
    }
}
