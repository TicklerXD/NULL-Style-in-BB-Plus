using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DevTools;

public class AssetBundleLoader
{
    public static List<Object> assets = []; // Asset memory
    public static string modPath = string.Empty;

    /// <summary>
    /// Loads all assets in memory from AssetBundle located in the Modded/your.mod.folder/AssetBundle folder with the .assets extension
    /// </summary>
    /// <param name="name"></param>
    public static void LoadAssets(string name)
    {
        try
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "AssetBunlde", name + ".assets"));
            Object[] sequence = assetBundle.LoadAllAssets();
            sequence.Do(assets.Add);
        }
        catch { 
            if (string.IsNullOrEmpty(modPath)) 
                Debug.LogError("ModPath does not exist");
        }
    }
    /// <summary>
    /// Loads all assets in memory from all AssetBundles located in the Modded/your.mod.folder/AssetBundle folder with the .assets extension
    /// </summary>
    public static void LoadAssets()
    {
        string[] allFilesFromFolder = Utils.GetAllFilesFromFolder(Path.Combine(modPath, "AssetBunlde"));
        foreach (string name in allFilesFromFolder)
        {
            LoadAssets(name);
        }
    }
    /// <summary>
    /// Returns an asset from the loaded ones by its name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns>An asset with the specified name</returns>
    public static T GetAsset<T>(string name) where T : Object => (T)assets.Find((Object x) => x.name.Equals(name));

    /// <summary>
    /// Allows you to get the original prefab or immediately make it active and get its copy
    /// </summary>
    /// <param name="name"></param>
    /// <param name="enable"></param>
    /// <returns>The original prefab or its enabled copy</returns>
    public static GameObject GetPrefab(string name, bool enable = true)
    {
        GameObject gameObject = (GameObject)assets.Find((Object x) => x.name.Equals(name));
        if (!enable)
            return gameObject;

        return Object.Instantiate(gameObject);
    }
    /// <summary>
    /// Returns the original prefab or an enabled copy that contains the specified name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="enable"></param>
    /// <returns>The original prefab or its enabled copy containing specified name</returns>
    public static GameObject GetPrefabContainingName(string name, bool enable = true)
    {
        GameObject gameObject = (GameObject)assets.Find((Object x) => x.name.Contains(name));
        if (!enable)
            return gameObject;

        return Object.Instantiate(gameObject);
    }

    /// <summary>
    /// Clears asset memory
    /// </summary>
    public static void Clear() => assets.Clear();
}