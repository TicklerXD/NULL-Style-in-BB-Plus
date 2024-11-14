using HarmonyLib;
using MTM101BaldAPI;
using NULL;
using NULL.Manager;
using PlusLevelFormat;
using PlusLevelLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace DevTools;

public static class Utils
{
    /// <summary>
    /// Just create a random color.
    /// </summary>
    /// <returns>Random color.</returns>
    public static Color GetRandomColor() => new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    /// <summary>
    /// Returns the names (not the path, but the names!) of files from a folder and all its subfolders
    /// </summary>
    /// <param name="folder"></param>
    /// <returns>Array with names of files</returns>
    public static string[] GetAllFilesFromFolder(string folder)
    {
        var files = new List<string>();

        foreach (var file in Directory.GetFiles(Path.Combine(BasePlugin.ModPath, folder)))
        {
            files.Add(Path.GetFileNameWithoutExtension(file));
        }

        foreach (var subfolder in Directory.GetDirectories(Path.Combine(BasePlugin.ModPath, folder)))
        {
            var relativeSubfolder = Path.Combine(folder, new DirectoryInfo(subfolder).Name);
            var subfolderFiles = GetAllFilesFromFolder(relativeSubfolder);
            foreach (var subfile in subfolderFiles)
            {
                files.Add(Path.Combine(new DirectoryInfo(relativeSubfolder).Name, subfile));
            }
        }
        return [.. files];
    }
    
    ///<summary>
    /// Loads a custom SceneObject from a file
    ///</summary>
    ///<param name="lvl"> Filename</param>
    ///<param name="folder"> Your folder for custom levels</param>
    ///<returns>The <typeparam name="SceneObject"/> object.</returns>
    public static SceneObject LoadCustomSceneObject(string lvl, string title = "WIP", int lvlNo = -1, Cubemap skybox = null, string folder = "ExtraLevels")
    {
        var path = Path.Combine(BasePlugin.ModPath, folder, lvl + ".cbld");
        try
        {
            BinaryReader binaryReader = new(File.OpenRead(path));
            var scene = CustomLevelLoader.LoadLevel(binaryReader.ReadLevel());
            scene.manager = FindResourceObject<BaseGameManager>();
            scene.levelTitle = title;
            scene.levelNo = lvlNo;
            scene.name = title;
            scene.skybox = skybox ?? FindResourceObjectWithName<Cubemap>("Cubemap_DayStandard");
            return scene;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Loading custom level failed.\nCheck that the file path is correct: {path}");
            Debug.LogException(e);
        }

        return null;
    }
    ///<summary>
    /// Gets an object of specified type by its name, if not found, attempts to get it from the assets from AssetManager.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    ///<param name="name"></param>
    ///<returns>The <typeparam name="T"/> object containg name, if not found returns null.</returns>
    public static T GetObject<T>(string name) where T : Object => FindResourceObjectContainingName<T>(name) ?? ModManager.m.Get<T>(name);
    ///<summary>
    /// Gets all objects from resources and AssetManager of specified type.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    ///<returns>Array with <typeparam name="T"/> objects, if not found returns null.</returns>
    public static T[] GetObjects<T>() where T : Object => [.. FindResourceObjects<T>().Union(ModManager.m.GetAll<T>())];
    /// <summary>
    /// Find an object from the Resources by the <paramref name="name"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns>An <typeparamref name="T"/> object from resources</returns>
    public static T FindResourceObjectContainingName<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().First(x => x.name.ToLower().Contains(name.ToLower()));
    /// <summary>
    /// Find an object from the Resources by the exact <paramref name="name"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns>An <typeparamref name="T"/> object from resources</returns>
    public static T FindResourceObjectWithName<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().First(x => x.name.ToLower() == name.ToLower());
    /// <summary>
    /// Find an array of objects from the Resources by the <paramref name="name"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns>An array of <typeparamref name="T"/> object from resources</returns>
    public static T[] FindResourceObjectsContainingName<T>(string name) where T : Object => Resources.FindObjectsOfTypeAll<T>().Where(x => x.name.ToLower().Contains(name.ToLower())).ToArray();
    /// <summary>
    /// Find an object from the resources
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Desired <typeparamref name="T"/> Object</returns>
    public static T FindResourceObject<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>()[0];
    /// <summary>
    /// Find objects from the resources
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>Desired <typeparamref name="T"/> Objects</returns>
    public static T[] FindResourceObjects<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>();
}
