using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using NULL.Content;
using NULL.NPCs;
using PlusLevelLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DevTools.Extensions;
using PixelInternalAPI.Extensions;
namespace NULL.Manager;

internal static partial class ModManager
{
    internal static List<SceneObject> nullScenes = [];
    internal static Dictionary<string, CustomLevelObject> nullLevels = [];
    internal static void LoadScenes() // Called AFTER all LevelObjects have been created by other mods
    {
        #region Local variables
        void Register_Internal(CustomLevelObject ld, bool withNpcs = true)
        {
            try
            {
                var chalk = ld.potentialItems.First(x => x.selection.itemType == Items.ChalkEraser).selection;

                Items[] types = [Items.Apple, Items.AlarmClock, Items.Boots, Items.DetentionKey, Items.Nametag];

                foreach (var item in ld.potentialItems)
                {
                    if (types.Contains(item.selection.itemType))
                    {
                        item.selection = chalk;
                        item.weight = 120;
                    }
                    if (item.selection.itemType == Items.ChalkEraser)
                    {
                        item.weight = 120;
                    }
                }

                ld.randomEvents?.RemoveAll(x => x?.selection.Type == RandomEventType.Snap);
            }
            catch {}
            if (!withNpcs)
            {
                ld.additionalNPCs = 0;
                ld.potentialNPCs = []; // No other npcs
                ld.forcedNpcs = [];
            }
            ld.potentialBaldis = [new() { selection = m.Get<NullNPC>(!ld.name.Contains("GLITCH") ? "NULL" : "NULLGLITCH"), weight = 100 }];
        } // Basically, does the same thing as GeneratorManagement.Register

        SceneObject[] objs = Resources.FindObjectsOfTypeAll<SceneObject>();
        Dictionary<LevelObject, CustomLevelObject>[] oldToNewMapping = new Dictionary<LevelObject, CustomLevelObject>[4];
        for (int i = 0; i < oldToNewMapping.Length; i++)
            oldToNewMapping[i] = [];

        #endregion

        foreach (SceneObject obj in objs)
        {
            if (obj.levelObject is not CustomLevelObject)
                continue;

            var scene = CustomLevelLoader.CreateEmptySceneObject();
            Dictionary<string, CustomLevelObject> m_nullLevels = [];
            if (obj.manager.GetType() == typeof(MainGameManager))
            {
                #region Creating level object for null (with npcs)
                CustomLevelObject nullMain = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(obj.levelObject);
                nullMain.name = "NULL_" + obj.levelObject.name;
                Register_Internal(nullMain);
                oldToNewMapping[0].Add((CustomLevelObject)obj.levelObject, nullMain);
                nullMain.MarkAsNeverUnload();
                m_nullLevels.Add(nullMain.name, nullMain);
                #endregion

                #region Creating level object for null (without npcs)
                CustomLevelObject nullMain_NoNpcs = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(nullMain);
                nullMain_NoNpcs.name = "NULL_" + obj.levelObject.name + "_NoNpcs";
                Register_Internal(nullMain_NoNpcs, false);
                oldToNewMapping[1].Add((CustomLevelObject)obj.levelObject, nullMain_NoNpcs);
                nullMain_NoNpcs.MarkAsNeverUnload();
                m_nullLevels.Add(nullMain_NoNpcs.name, nullMain_NoNpcs);
                #endregion

                #region Creating level object for glitch (with npcs)
                CustomLevelObject glitchMain = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(nullMain);
                glitchMain.name = "GLITCH_" + obj.levelObject.name;
                Register_Internal(glitchMain);
                oldToNewMapping[2].Add((CustomLevelObject)obj.levelObject, nullMain);
                glitchMain.MarkAsNeverUnload();
                m_nullLevels.Add(glitchMain.name, glitchMain);
                #endregion

                #region Creating level object for glitch (without npcs)
                CustomLevelObject glitchMain_NoNpcs = ScriptableObjectHelpers.CloneScriptableObject<LevelObject, CustomLevelObject>(nullMain_NoNpcs);
                glitchMain_NoNpcs.name = "GLITCH_" + obj.levelObject.name + "_NoNpcs";
                Register_Internal(glitchMain_NoNpcs, false);
                oldToNewMapping[3].Add((CustomLevelObject)obj.levelObject, glitchMain_NoNpcs);
                glitchMain_NoNpcs.MarkAsNeverUnload();
                m_nullLevels.Add(glitchMain_NoNpcs.name, glitchMain_NoNpcs);
                #endregion

                #region Set scene params

                scene.manager = m.Get<NullPlusManager>("NullPlusMan");
                scene.levelNo = obj.levelNo;
                scene.levelObject = nullMain;

                if (System.Text.RegularExpressions.Regex.IsMatch(obj.levelTitle, @"^F\d$"))
                {
                    scene.name = "NULL_" + obj.levelTitle;
                    scene.levelTitle = "N" + obj.levelTitle.Substring(1);
                }

                scene.shopItems = [.. obj.shopItems.ReplaceAllAndReturn(x => x.selection.itemType == Items.Apple, new WeightedItemObject() { selection = PixelInternalAPI.Extensions.GenericExtensions.FindResourceObjectByName<ItemObject>("ChalkEraser"), weight = 100 })];
                scene.totalShopItems = obj.totalShopItems;
                scene.mapPrice = obj.mapPrice;
                #endregion

                #region Set level objects params
                foreach (var kp in m_nullLevels)
                {
                    kp.Value.shopItems = scene.shopItems;
                    kp.Value.totalShopItems = scene.totalShopItems;
                    kp.Value.mapPrice = scene.mapPrice;
                }
                #endregion

                scene.MarkAsNeverUnload();
                nullLevels.AddRange(m_nullLevels);
                nullScenes.Add(scene);
            }
        }

        #region Creating ending scene
        #region Creating NullPlusFinaleManager
        GameObject newObject = new();
        newObject.SetActive(false);
        var finaleMan = newObject.AddComponent<NullPlusFinaleManager>();
        finaleMan.spawnNpcsOnInit = false;
        finaleMan.spawnImmediately = false;
        finaleMan.ReflectionSetVariable("destroyOnLoad", true);
        finaleMan.gameObject.name = "NullPlusFinaleManager";
        finaleMan.gameObject.ConvertToPrefab(true);
        #endregion

        LevelAsset levelAsset = ScriptableObject.CreateInstance<LevelAsset>();
        levelAsset.spawnPoint = new(65, 5, 25);
        levelAsset.spawnDirection = Direction.North;
        levelAsset.levelSize = new(10, 10);

        #region Load tiles
        var types = new int[,]
        {
            { 12, 8, 10, 10, 10, 10, 8, 9 },
            { 4, 1, 14, 8, 8, 9, 4, 1 },
            { 4, 0, 9, 6, 0, 1, 4, 1 },
            { 14, 10, 10, 10, 2, 3, 4, 1 },
            { 4, 0, 0, 0, 8, 8, 0, 1 },
            { 4, 0, 0, 0, 0, 0, 0, 1 },
            { 6, 2, 2, 2, 2, 2, 2, 3 }
        };

        var tilesList = new List<CellData>();

        for (int x = 3; x <= 9; x++)
        {
            for (int z = 2; z <= 9; z++)
            {
                int type = 0;
                int roomId = 1;
                switch (x)
                {
                    case 3:
                        type = types[x - 3, z - 2]; break;
                    case 4:
                        type = types[x - 3, z - 2];
                        roomId = (z >= 5 && z <= 7) ? 0 : 1; break;
                    case 5:
                        type = types[x - 3, z - 2];
                        roomId = (z >= 5 && z <= 7) ? 0 : 1; break;
                    case 6:
                        type = types[x - 3, z - 2];
                        roomId = (z == 6 || z == 7) ? 0 : 1; break;
                    case 7:
                        type = types[x - 3, z - 2]; break;
                    case 8:
                        type = types[x - 3, z - 2]; break;
                    case 9:
                        type = types[x - 3, z - 2]; break;

                }
                tilesList.Add(new() { pos = new(x, z), type = type, roomId = roomId });
            }
        }
        levelAsset.tile = [.. tilesList];
        #endregion

        #region Creation rooms
        static T Find<T>(string name) where T : Object => PixelInternalAPI.Extensions.GenericExtensions.FindResourceObjectByName<T>(name);

        #region Create Office
        var doorMats = Find<StandardDoorMats>("ClassDoorSet");
        RoomData data = new()
        {
            name = "Office",
            category = RoomCategory.Office,
            type = RoomType.Room,
            doorMats = doorMats,
            florTex = m.Get<Sprite>("BasicRealCarpet").texture,
            wallTex = m.Get<Sprite>("BasicRealWall").texture,
            ceilTex = m.Get<Sprite>("BasicRealCeiling").texture,
            activity = new ActivityData()
            {
                prefab = PixelInternalAPI.Extensions.GenericExtensions.FindResourceObject<NoActivity>(),
                position = new Vector3(41.5f, 5, 77.5f)
            },
            hasActivity = true
        };

        #region Add basic objects
        data.basicObjects.Add(new()
        {
            position = new(45, 0, 77.5f),
            prefab = Find<Transform>("BigDesk")
        });

        data.basicObjects.Add(new()
        {
            position = new(53, 0, 73.5f),
            prefab = Find<Transform>("BigDesk"),
            rotation = Quaternion.Euler(0, 90, 0)
        });
        data.basicObjects.Add(new()
        {
            position = new(55, 0, 65),
            prefab = Find<Transform>("CeilingFan")

        });
        data.basicObjects.Add(new()
        {
            position = new(58.09f, 4.05f, 52.01f),
            prefab = Find<Transform>("Decor_Papers")
        });
        data.basicObjects.Add(new()
        {
            position = new(55, 0, 52),
            prefab = Find<Transform>("FilingCabinet_Short"),
            rotation = Quaternion.Euler(0, 270, 0)
        });
        data.basicObjects.Add(new()
        {
            position = new(45, 3.75f, 77.5f),
            prefab = Find<Transform>("MyComputer"),
            rotation = Quaternion.Euler(0, 270, 0)
        });
        data.basicObjects.Add(new()
        {
            position = new(44.28f, 0, 74.05f),
            prefab = Find<Transform>("Chair_Test"),
        });

        #endregion

        levelAsset.rooms.Add(data);
        #endregion

        #region Create Empty room
        var black = Find<Texture2D>("BlackTexture");
        data = new()
        {
            name = "Empty",
            category = RoomCategory.Null,
            type = RoomType.Hall,
            doorMats = doorMats,
            florTex = black,
            wallTex = black,
            ceilTex = black
        };
        levelAsset.rooms.Add(data);
        #endregion

        #region Create Supplies room
        data = new()
        {
            name = "Supplies",
            category = RoomCategory.Special,
            type = RoomType.Room,
            doorMats = doorMats,
            florTex = Find<Texture2D>("Carpet"),
            wallTex = m.Get<Sprite>("BasicRealWall").texture,
            ceilTex = Find<Texture2D>("PlasticTable"),
            lightPre = MTM101BaldiDevAPI.roomAssetMeta.Get("Room_ReflexOffice_0").value.lightPre
        };
        levelAsset.rooms.Add(data);
        #endregion
        #endregion

        #region Creation doors
        var door = Find<StandardDoor>("ClassDoor_Standard");
        levelAsset.doors.Add(new(1, door, new(6, 5), Direction.North));
        levelAsset.doors.Add(new(2, door, new(4, 4), Direction.North));
        #endregion

        #region Add lighting
        levelAsset.lights.Add(new()
        {
            position = new(5, 6),
            strength = 8
        });
        #endregion

        #region Creation posters
        var officeWindow = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficeWindow").texture, []);
        levelAsset.posters.Add(new()
        {
            poster = officeWindow,
            position = new(4, 5),
            direction = Direction.West
        });
        levelAsset.posters.Add(new()
        {
            poster = officeWindow,
            position = new(6, 7),
            direction = Direction.North
        });
        levelAsset.posters.Add(new()
        {
            poster = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficeWhiteboard").texture, []),
            position = new(4, 7),
            direction = Direction.West
        });
        levelAsset.posters.Add(new()
        {
            poster = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficeRulesPoster").texture, []),
            position = new(6, 7),
            direction = Direction.East
        });
        levelAsset.posters.Add(new()
        {
            poster = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficePlusPoster").texture, []),
            position = new(6, 6),
            direction = Direction.East
        });
        levelAsset.posters.Add(new()
        {
            poster = ObjectCreators.CreatePosterObject(m.Get<Sprite>("MyOfficeToys").texture, []),
            position = new(5, 5),
            direction = Direction.South
        });
        #endregion

        levelAsset.name = "NULL";

        var endingScene = CustomLevelLoader.CreateEmptySceneObject();
        endingScene.levelAsset = levelAsset;
        endingScene.levelTitle = endingScene.name = "NULL";
        endingScene.manager = finaleMan;
        endingScene.MarkAsNeverUnload();
        #endregion

        nullScenes.Add(endingScene); // Must be the last one so that it will be the next level for the last floor

        #region Adding next level for each SceneObject
        for (int i = 0; i < nullScenes.Count - 1; i++)
            nullScenes[i].nextLevel = nullScenes[i + 1];
        #endregion

        #region Adding previous levels for level objects
        foreach (var dictionary in oldToNewMapping)
        {
            foreach (var kvp in dictionary)
            {
                kvp.Value.previousLevels = new LevelObject[kvp.Key.previousLevels.Length];
                for (int i = 0; i < kvp.Key.previousLevels.Length; i++)
                {
                    kvp.Value.previousLevels[i] = dictionary[kvp.Key.previousLevels[i]];
                }
            }
        }
        #endregion
    }
}
