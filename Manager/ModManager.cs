using BepInEx.Bootstrap;
using DevTools;
using DevTools.Extensions;
using DevTools.Patches;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Reflection;
using NULL.Content;
using NULL.CustomComponents;
using NULL.Manager.CompatibilityModule;
using NULL.NPCs;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using static MTM101BaldAPI.AssetTools.AssetLoader;
using static NULL.BasePlugin;
using static PixelInternalAPI.Extensions.GenericExtensions;
using static UnityEngine.Object;

namespace NULL.Manager;

internal static partial class ModManager
{
    public static BasePlugin plug;
   
    static bool _nullStyle;
    static bool _glitchStyle;
    public static AssetManager m = new();
    public static bool NullStyle
    {
        get => _nullStyle;
        set
        {
            _nullStyle = value;

            if (!_glitchStyle)
                RePatch();

            if (!value) 
                GlitchStyle = false;

           // TryRunMethodIfModInstalled("pixelguy.pixelmodding.baldiplus.bbextracontent", () => BBTimes.Plugin.BooleanStorage.endGameMusic = !value);
        }
    }
    public static bool GlitchStyle
    {
        get => _nullStyle && _glitchStyle;
        set
        {
            _glitchStyle = value;
            if (!NullStyle && value)
                NullStyle = value;

            RePatch();
        }
    }
    internal static IEnumerator LoadContent()
    {
        bool e = Plugins.IsEditor;
        yield return e ? 4 : 3;
        yield return "Loading assets...";
        TryRunMethod(LoadAssets);
        yield return "Creating NPCs...";
        TryRunMethod(CreateNPCs);
        yield return "Loading captions...";
        TryRunMethod(LoadCaptions);
        if (e)
        {
            yield return "Loading editor assets...";
            TryRunMethod(EditorCompat.AddEditorAssets);
        }
    }

    static void LoadAssets()
    {
        m.LoadAll(); // Loads all sprites and SoundObjects from the mod folder

        #region Load MIDIs
        foreach (var midi in Utils.GetAllFilesFromFolder("MidiDB"))
            MidiFromMod(midi, plug, ModPath, "MidiDB", midi + ".mid");
        #endregion

        #region Create ambience
        var ambience = ScriptableObject.CreateInstance<LoopingSoundObject>();
        ambience.clips = [m.Get<SoundObject>("unknown_ambience").soundClip];
        ambience.mixer = FindResourceObjectByName<AudioMixerGroup>("Master");
        NullPlusManager.darkAmbience = ambience;
        #endregion

        #region Load projectiles
        #region Banana
        string prefix = "Projectile_";
        var pr = new GameObject(prefix + "Banana");
        var spriteBase = Instantiate(Utils.FindResourceObjectWithName<GameObject>("Decor_Banana"));
        spriteBase.transform.SetParent(pr.transform, false);
        spriteBase.transform.localScale += new Vector3(1.4f, 1.4f);

        var collider = pr.AddComponent<SphereCollider>();
        collider.radius = 4f;
        collider.isTrigger = true;
        pr.AddComponent<NullProjectile>();

        pr.ConvertToPrefab(true);
        m.Add(pr.name, pr.GetComponent<NullProjectile>());
        BossManager.projectiles.Add(pr.GetComponent<NullProjectile>());
        #endregion

        #region Plant
        pr = Instantiate(Utils.FindResourceObjectWithName<GameObject>("Plant"));

        pr.name = prefix + "Plant";
        collider = pr.AddComponent<SphereCollider>();
        collider.radius = 4f;
        collider.isTrigger = true;
        pr.transform.localScale -= Vector3.one * .125f;
        pr.AddComponent<NullProjectile>();
        pr.ConvertToPrefab(true);
        m.Add(pr.name, pr.GetComponent<NullProjectile>());
        BossManager.projectiles.Add(pr.GetComponent<NullProjectile>());
        #endregion

        #region Chair
        pr = new GameObject(prefix + "Chair");
        spriteBase = Instantiate(Utils.FindResourceObjectWithName<GameObject>("Chair_Test"));

        spriteBase.transform.SetParent(pr.transform, false);
        spriteBase.transform.localScale += Vector3.one * .5f;
        spriteBase.transform.localPosition = pr.transform.localPosition;
        spriteBase.transform.localRotation = pr.transform.localRotation;

        Destroy(spriteBase.GetComponent<BoxCollider>());
        collider = pr.AddComponent<SphereCollider>();
        collider.radius = 4;
        collider.isTrigger = true;
        pr.AddComponent<NullProjectile>();
        pr.ConvertToPrefab(true);
        m.Add(pr.name, pr.GetComponent<NullProjectile>());
        BossManager.projectiles.Add(pr.GetComponent<NullProjectile>());
        #endregion
        #endregion

        #region Setup managers
        GameObject obj = new();
        obj.SetActive(false);
        var mainGameManager = obj.AddComponent<NullPlusManager>();
        GameObject ambient = Instantiate(FindResourceObject<MainGameManager>().transform.Find("Ambience").gameObject, mainGameManager.transform);
        mainGameManager.elevatorScreenPre = FindResourceObject<ElevatorScreen>();
        mainGameManager.pitstop = FindResourceObject<MainGameManager>().pitstop;  //FindResourceObjectByName<SceneObject>("Pitstop");
        mainGameManager.ReflectionSetVariable("ambience", ambient.GetComponent<Ambience>());
        mainGameManager.spawnNpcsOnInit = false;
        mainGameManager.spawnImmediately = false;
        mainGameManager.beginPlayImmediately = false;
        mainGameManager.ReflectionSetVariable("destroyOnLoad", true);
        mainGameManager.gameObject.name = "NullPlusManager";
        mainGameManager.gameObject.ConvertToPrefab(true);
        m.Add("NullPlusMan", mainGameManager);
        #endregion

        #region Pick mode creation
        var pick = Instantiate(FindResourceObjectByName<GameObject>("PickChallenge"));
        pick.name = "PickPre";
        pick.transform.RemoveChildsContainingNames(["Speedy", "Stealthy", "Grapple", "ModeText"]);
        pick.gameObject.ConvertToPrefab(true);
        m.Add("PickPre", pick);
        #endregion

        // Setup null's curves
        var b = Utils.FindResourceObjectContainingName<Baldi>("Baldi");
        NullNPC.slapCurve = b.slapCurve;
        NullNPC.speedCurve = b.speedCurve;
    }
    public static void DisableSounds(bool val, params string[] sounds)
    {
        foreach (var sound in sounds)
        {
            var s = sound.ToString();
            if (val) AudioManagerPatcher.disabledSounds.Add(s);
            else if (AudioManagerPatcher.disabledSounds.Contains(s)) 
                AudioManagerPatcher.disabledSounds.Remove(s);
        }
    }
    
    public static bool ModInstalled(string mod) => Chainloader.PluginInfos.ContainsKey(mod);

    public static void TryRunMethod(Action actionToRun, bool causeCrashIfFail = true)
    {
        try
        {
            actionToRun();
        }
        catch (Exception e)
        {
            Debug.LogWarning("------ Error caught during an action ------");
            Debug.LogException(e);

            if (causeCrashIfFail)
                MTM101BaldiDevAPI.CauseCrash(plug.Info, e);
        }
    }
    public static bool TryRunMethodIfModInstalled(string guid, Action act)
    {
        if (ModInstalled(guid))
        {
            TryRunMethod(act);
            return true;
        }
        return false;
    }
}
