using MTM101BaldAPI.SaveSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Object;

namespace DevTools;

public static class ExtraVariables
{
    public static EnvironmentController ec => Singleton<BaseGameManager>.Instance.Ec;
    public static PlayerManager pm => Singleton<CoreGameManager>.Instance.GetPlayer(0);
    public static MovementModifier stopMod;
    public static TimeScaleModifier stopNpcs;
    public static Canvas canvas;
    public static bool freezeElevators;
    public static CoreGameManager Core { get => Singleton<CoreGameManager>.Instance; }
    public static bool AllNotebooks { get => Singleton<BaseGameManager>.Instance.allNotebooksFound; }
    public static int CurrentFloor { get => Singleton<CoreGameManager>.Instance.sceneObject.levelNo; }
    public static IntVector2 LevelCenter { get => new IntVector2(ec.levelSize.x / 2, ec.levelSize.z / 2); }
    public static List<Cell> AllCellsInHall { 
        get
        {
            var res = ec.mainHall.AllTilesNoGarbage(false, false);
            return res.Count > 0 ? res : ec.AllTilesNoGarbage(false, false);
        }
    }
    public static Cell RandomCell { get => ec.RandomCell(false, false, false); }
    public static Cell RandomCellFromHallway
    {
        get
        {
            var a = AllCellsInHall;
            return a[Random.Range(0, a.Count)];
        }
    }
    public static bool PlayerInElevator { get => FindObjectOfType<ElevatorScreen>(); }

    public static void Reset() // ALWAYS! Always damn sure when you add a new static variable, assign it a value in the Reset() method!
    {
     //   ec = Singleton<BaseGameManager>.Instance.Ec;
      //  pm = Singleton<CoreGameManager>.Instance.GetPlayer(0);
        stopMod = new(Vector3.zero, 0f);
        stopNpcs = new() { npcTimeScale = 0f, environmentTimeScale = 1f };
        freezeElevators = false;
    }

    public static void ClearEffects()
    {
        try
        {
            foreach (var fog in ec.fogs) ec.RemoveFog(fog);
            foreach (var gum in FindObjectsOfType<Gum>()) gum.Cut();
        }
        catch { }
        Singleton<CoreGameManager>.Instance.GetPlayer(0).Am.moveMods.RemoveAll(x => x.Equals(stopMod));
        Shader.SetGlobalFloat("_VertexGlitchIntensity", 0f);
        Shader.SetGlobalFloat("_TileVertexGlitchIntensity", 0f);
        Shader.SetGlobalInt("_ColorGlitching", 0);
        Shader.SetGlobalInt("_SpriteColorGlitching", 0);
    }

    public static void StopAllEvents()
    {
        for (int i = 0; i < ec.CurrentEventTypes.Count; i++)
            ec.GetEvent(ec.CurrentEventTypes[i]).remainingTime = 0f;
    }

    public static void RemoveAllItems()
    {
        for (int k = 0; k < 5; k++) Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.RemoveItem(k);
    }

    public static void ForceCloseAllElevators()
    {
        var bgm = Singleton<BaseGameManager>.Instance;
        foreach (var elevator in ec.elevators)
        {
            elevator.Close();
            elevator.Door.Shut();
            bgm.elevatorsClosed++;
            bgm.elevatorsToClose--;
        }
    }

    public static void StartEvent(RandomEventType type, System.Random rng = null)
    {
        var RandomEvents = Resources.FindObjectsOfTypeAll<RandomEvent>();
        var randomEvent = Instantiate(RandomEvents.OfType<RandomEvent>().FirstOrDefault(x => x.Type == type));
        var controlledRNG = FindObjectOfType<LevelBuilder>().controlledRNG;
        randomEvent.Initialize(ec, controlledRNG);
        randomEvent.SetEventTime(controlledRNG);
        randomEvent.AfterUpdateSetup(rng ?? new System.Random());
        randomEvent.Begin();
    }

    public static void GameOver() // Using this will restart the level without being caught by Baldi
    {
        if (Core.lives < 1 && Core.extraLives < 1)
        {
            Singleton<GlobalCam>.Instance.SetListener(true);
            Singleton<CoreGameManager>.Instance.ReturnToMenu();
            return;
        }

        if (Core.lives > 0)
            Core.lives--;
        else
            Core.extraLives--;

        Singleton<BaseGameManager>.Instance.RestartLevel();
    }
    public static void PausePlayer(bool val) // For cutscenes
    {
        Singleton<CoreGameManager>.Instance.disablePause = val;
        if (val)
        {
            pm.Am.moveMods.Add(stopMod);
            pm.itm.enabled = false;
            return;
        }
        pm.Am.moveMods.RemoveAll(x => x.Equals(stopMod));
    }

    public static SceneObject LoadGame(bool setSave = true, bool ignoreSaveFile = false, SceneObject customScene = null) // Loads a modified save or starts a new game
    {
        Debug.Log("CustomScene: " + customScene.name);
        bool saveAvailable = !ignoreSaveFile && Singleton<ModdedFileManager>.Instance.saveData.saveAvailable;
        GameLoader gameLoader = gameLoader = Resources.FindObjectsOfTypeAll<GameLoader>().First();

        gameLoader.gameObject.SetActive(true);
        gameLoader.cgmPre = PixelInternalAPI.Extensions.GenericExtensions.FindResourceObject<CoreGameManager>();
        var scene = customScene ?? Singleton<ModdedFileManager>.Instance.saveData.level;

        if (!saveAvailable)
        {
            gameLoader.CheckSeed();
            gameLoader.Initialize(2);
            gameLoader.SetMode(0);
        }
        else
        {
            Singleton<ModdedFileManager>.Instance.CreateSavedGameCoreManager(gameLoader);
            gameLoader.SetMode(0);
            Singleton<CursorManager>.Instance.LockCursor();
        }

        ElevatorScreen elevatorScreen = (from x in SceneManager.GetActiveScene().GetRootGameObjects()
                                         where x.name == "ElevatorScreen"
                                         select x).First().GetComponent<ElevatorScreen>();

        gameLoader.AssignElevatorScreen(elevatorScreen);
        elevatorScreen.gameObject.SetActive(true);
        gameLoader.LoadLevel(scene);
        elevatorScreen.Initialize();
        gameLoader.SetSave(setSave);

        if (saveAvailable)
            Singleton<ModdedFileManager>.Instance.DeleteSavedGame();

        return scene;
    }

    public static void HideHuds(bool val)
    {
        var huds = Singleton<CoreGameManager>.Instance.huds;
        foreach (var hud in huds)
            hud?.Hide(val);
    }
}
