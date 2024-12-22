﻿using DevTools.Extensions;
using MidiPlayerTK;
using NULL.Manager;
using NULL.NPCs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DevTools.ExtraVariables;


namespace NULL.Content;

public class NullPlusManager : MainGameManager
{
    public static NullPlusManager instance;
    [SerializeField] public NullNPC nullNpc;
    float glitchVal = 0f;
    BossManager Bm => BossManager.Instance;
    internal static LoopingSoundObject darkAmbience;
    public override void Initialize()
    {
        BasePlugin.RePatch(); // I don't know why I should to do another fucking repatch if it has already been done when setting NullStyle = true in EditorCompat.LoadNullLevel
        Reset();
        instance = this;
        if (BossManager.Instance == null)
            new GameObject("BossManager").AddComponent<BossManager>();

        base.Initialize();
        ec.SpawnNPCs();
        ec.StartEventTimers();

        if (BasePlugin.darkAtmosphere.Value)
        {
            foreach (var cell in ec.AllCells()) cell.SetLight(false); // When using ___ec.Set All Lights(false); swing doors still remain bright
            Shader.SetGlobalColor("_SkyboxColor", Color.black);
        }
        ec.standardDarkLevel = new Color(0.35f, 0.35f, 0.35f);
        // Core.GetHud(0).Hide(false);
        HideHuds(false);
        ec.MakeNoise(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position, 127);
        Bm?.RemoveAllProjectiles();
        Singleton<CoreGameManager>.Instance.GetHud(0).BaldiTv.gameObject.TryAddComponent<CustomComponents.NullTV>();
    }

    public override void BeginPlay()
    {
        base.BeginPlay();
        PixelInternalAPI.Classes.GlobalAudioListenerModifier.Reset();

        if (BasePlugin.darkAtmosphere.Value)
            Singleton<MusicManager>.Instance.QueueFile(darkAmbience, true);

        Singleton<MusicManager>.Instance.KillMidi();
    }

    public override void VirtualUpdate()
    {
        base.VirtualUpdate();
        if (Bm == null || ec == null || nullNpc == null) return;

        if (Bm.BossActive)
        {
            if (!Singleton<MusicManager>.Instance.MidiPlaying && Bm.holdBeat && !Bm.bossTransitionWaiting && !Core.Paused) Bm.StartBossFight();
            nullNpc.Hear(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position, 127, false);
            Singleton<MusicManager>.Instance.MidiPlayer.MPTK_ChannelVolumeSet(9, Mathf.Clamp(1f - (Vector3.Distance(nullNpc.transform.position, Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position) - 75f) / 150f, 0f, 1f));
        }
    }
    public override void CollectNotebook(Notebook notebook)
    {
        base.CollectNotebook(notebook);
        if (BasePlugin.darkAtmosphere.Value)
            ec.MakeNoise(notebook.transform.position, 69);
    }
    public override void LoadNextLevel()
    {
        if (Core.sceneObject.nextLevel is null)
            Core.ReturnToMenu();
        else
            base.LoadNextLevel();
    }

    public override void ElevatorClosed(Elevator elevator)
    {
        base.ElevatorClosed(elevator);

        var list = new List<Elevator>();
        list.AddRange(ec.elevators);
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].IsOpen)
            {
                list.RemoveAt(i);
                i--;
            }
        }

        if (elevatorsClosed >= 3 && elevatorsToClose == 0)
        {
            nullNpc.behaviorStateMachine.ChangeState(new NullNPC_Preboss(nullNpc, list[Random.Range(0, list.Count)]));
            freezeElevators = true;
        }
    }
#pragma warning disable IDE0051
    new void OnEnable() => MusicManager.OnMidiEvent += MidiEvent;
    new void OnDisable() => MusicManager.OnMidiEvent -= MidiEvent;

#pragma warning restore

    void MidiEvent(MPTKEvent midiEvent)
    {
        if (Bm == null) return;

        if (Bm.BossActive && !Bm.holdBeat && midiEvent.Command == MPTKCommand.MetaEvent && midiEvent.Meta == MPTKMeta.TextEvent)
        {          
            if (glitchVal <= 0f) StartCoroutine(UnGlitch());
            glitchVal = 1f;
            Shader.SetGlobalFloat("_VertexGlitchSeed", Random.Range(0f, 1000f));
            Shader.SetGlobalFloat("_VertexGlitchIntensity", glitchVal * 3f);
            Shader.SetGlobalFloat("_TileVertexGlitchSeed", Random.Range(0f, 1000f));
            Shader.SetGlobalFloat("_TileVertexGlitchIntensity", glitchVal * 3f);
        }
    }
    IEnumerator UnGlitch()
    {
        yield return null;
        while (glitchVal > 0f)
        {
            glitchVal -= Time.deltaTime * 4f;
            Shader.SetGlobalFloat("_VertexGlitchIntensity", glitchVal * 3f);
            Shader.SetGlobalFloat("_TileVertexGlitchIntensity", glitchVal * 3f);
            yield return null;
        }
        glitchVal = 0f;
        Shader.SetGlobalFloat("_VertexGlitchIntensity", 0f);
        Shader.SetGlobalFloat("_TileVertexGlitchIntensity", 0f);
        yield break;
    }

    public static IEnumerator AngerGlitch(float wait)
    {
        float glitchRate = 0.5f;
        while (wait > 0f)
        {
            wait -= Time.deltaTime;
            yield return null;
        }
        wait = 0f;
        Shader.SetGlobalInt("_ColorGlitching", 1);
        Shader.SetGlobalInt("_SpriteColorGlitching", 1);
        while (wait < 3f)
        {
            wait += Time.deltaTime / (ModManager.GlitchStyle ? 2 : 1);
            Shader.SetGlobalFloat("_VertexGlitchSeed", Random.Range(0f, 1000f));
            Shader.SetGlobalFloat("_TileVertexGlitchSeed", Random.Range(0f, 1000f));
            Singleton<InputManager>.Instance.Rumble(wait / 6f, 0.05f);
            if (!Singleton<PlayerFileManager>.Instance.reduceFlashing)
            {
                glitchRate -= Time.unscaledDeltaTime;
                Shader.SetGlobalFloat("_VertexGlitchIntensity", Mathf.Pow(wait, 2f));
                Shader.SetGlobalFloat("_TileVertexGlitchIntensity", Mathf.Pow(wait, 2f));
                Shader.SetGlobalFloat("_ColorGlitchPercent", wait * 0.05f);
                Shader.SetGlobalFloat("_SpriteColorGlitchPercent", wait * 0.05f);
                if (glitchRate <= 0f)
                {
                    Shader.SetGlobalInt("_ColorGlitchVal", Random.Range(0, 4096));
                    Shader.SetGlobalInt("_SpriteColorGlitchVal", Random.Range(0, 4096));
                    Singleton<InputManager>.Instance.SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
                    glitchRate = 0.55f - wait * 0.1f;
                }
            }
            else
            {
                Shader.SetGlobalFloat("_ColorGlitchPercent", wait * 0.25f);
                Shader.SetGlobalFloat("_SpriteColorGlitchPercent", wait * 0.25f);
                Shader.SetGlobalFloat("_VertexGlitchIntensity", wait * 2f);
                Shader.SetGlobalFloat("_TileVertexGlitchIntensity", wait * 2f);
            }
            yield return null;
        }
        Shader.SetGlobalFloat("_VertexGlitchIntensity", 0f);
        Shader.SetGlobalFloat("_TileVertexGlitchIntensity", 0f);
        Shader.SetGlobalInt("_ColorGlitching", 0);
        Shader.SetGlobalInt("_SpriteColorGlitching", 0);
    }
}