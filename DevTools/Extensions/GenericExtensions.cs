using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using NULL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DevTools.Extensions;

public static class GenericExtensions
{
    /// <summary>
    /// Set Alpha for Image.
    /// </summary>
    /// <param name="img">Image to change.</param>
    /// <param name="a">Target alpha.</param>
    /// <returns>New color with changed alpha.</returns>
    public static Color SetAlpha(this Image img, float a)
    {
        var c = img.color;
        c.a = a;
        img.color = c;
        return c;
    }
    ///<summary>
    /// Returns a array of file names from the <paramref name="directory"/>.
    ///</summary>
    ///<param name="directory"></param>
    ///<returns>Array of file names.</returns>
    static IEnumerable<string> GetFiles(string directory)
    {
        foreach (var s in Directory.GetFiles(Path.Combine(BasePlugin.ModPath, directory)))
            yield return Path.GetFileNameWithoutExtension(s);
    }
    ///<summary>
    /// Returns a array of file names from the Modded/your.mod.folder folder.
    ///</summary>
    ///<param name="directory"></param>
    ///<returns>Array of file names.</returns>
    public static string[] GetAllFiles(string directory) => [.. GetFiles(directory)];

    /// <summary>
    /// Removes all elements that satisfy the predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="predicate"></param>
    /// <returns>Collection with deleted items.</returns>
    public static IEnumerable<T> RemoveAllAndReturn<T>(this IEnumerable<T> values, Predicate<T> predicate)
    {
        using (var enumerator = values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (!predicate(enumerator.Current))
                    yield return enumerator.Current;
            }
        }
    }
    /// <summary>
    /// Replaces all elements that satisfy the predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="predicate"></param>
    /// <param name="replacement"></param>
    /// <returns>Collection with replaced elements.</returns>
    public static IEnumerable<T> ReplaceAllAndReturn<T>(this IEnumerable<T> values, Predicate<T> predicate, T replacement)
    {
        using (var enumerator = values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    yield return replacement;

                yield return enumerator.Current;
            }
        }
    }

    /// <summary>
    /// Extension to find the index of an element inside the collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="val"></param>
    /// <returns>The index of the element or -1 if it hasn't been found</returns>
    public static int IndexAt<T>(this IEnumerable<T> values, T val)
    {
        int index = 0;
        using (IEnumerator<T> enumerator = values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (ReferenceEquals(enumerator.Current, val) || Equals(val, enumerator.Current))
                    return index;

                index++;
            }
        }
        return -1;
    }
    /// <summary>
    /// Extension to find the index of the last element inside the collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="val"></param>
    /// <returns>The index of the element or -1 if it hasn't been found</returns>
    public static int LastIndexAt<T>(this IEnumerable<T> values, T val)
    {
        int curIndex = -1;
        int index = 0;
        using (IEnumerator<T> enumerator = values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (ReferenceEquals(enumerator.Current, val) || Equals(val, enumerator.Current))
                    curIndex = index;

                index++;
            }
        }
        return curIndex;
    }
    /// <summary>
    /// Extension to find the index of an element inside the collection based on the passed conditions
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="func"></param>
    /// <returns>The index of the element or -1 if it hasn't been found</returns>
    public static int IndexAt<T>(this IEnumerable<T> values, Predicate<T> func)
    {
        int index = 0;
        using (IEnumerator<T> enumerator = values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (func(enumerator.Current))
                    return index;

                index++;
            }
        }
        return -1;
    }
    /// <summary>
    /// Extension to find the index of the last element inside the collection based on the passed conditions
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="func"></param>
    /// <returns>The index of the element or -1 if it hasn't been found</returns>
    public static int LastIndexAt<T>(this IEnumerable<T> values, Predicate<T> func)
    {
        int curIndex = -1;
        int index = 0;
        using (IEnumerator<T> enumerator = values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (func(enumerator.Current))
                    curIndex = index;

                index++;
            }
        }
        return curIndex;
    }
    /// <summary>
    /// Finds the element in a list with the specified name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="name"></param>
    /// <returns>The element with specified name</returns>
    public static T ElementWithName<T>(this List<T> list, string name) => list.Find(x => x.ToString().Equals(name));
    /// <summary>
    /// Finds the element in a list containg the specified name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="name"></param>
    /// <returns>The element containing the specified name</returns>
    public static T ElementContainingName<T>(this List<T> list, string name) => list.Find(x => x.ToString().Contains(name));
    /// <summary>
    /// Finds the elements in a list with the specified name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="name"></param>
    /// <returns>List of elements with specified name</returns>
    public static List<T> ElementsWithName<T>(this List<T> list, string name) => list.FindAll(x => x.ToString().Equals(name));
    /// <summary>
    /// Finds the elementss in a list containg the specified name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="name"></param>
    /// <returns>List of elements containg the specified name</returns>
    public static List<T> ElementsContainingName<T>(this List<T> list, string name) => list.FindAll(x => x.ToString().Contains(name));
    /// <summary>
    /// Replaces an item at <paramref name="index"/> of a List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void Replace<T>(this IList<T> values, int index, T value)
    {
        if (index < 0 || index >= values.Count || values.Count == 0)
            throw new ArgumentOutOfRangeException($"The index: {index} is out of the list range (Length: {values.Count})");

        values.RemoveAt(index);
        values.Insert(index, value);
    }
    /// <summary>
    /// Does specific action using <paramref name="func"/> set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="func"></param>
    /// <returns>Modifyed collection</returns>
    public static IEnumerable<T> DoAndReturn<T>(this IEnumerable<T> values, Func<T, T> func)
    {
        using (var enumerator = values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                yield return func(enumerator.Current);
            }
        }
    }

    /// <summary>
    /// Extension to remove an item from a collection based on the <paramref name="val"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="val"></param>
    /// <returns>A collection without the item provided</returns>
    /// <exception cref="NullReferenceException"></exception>
    public static IEnumerable<T> RemoveIn<T>(this IEnumerable<T> values, T val) => values.Where(x => !ReferenceEquals(x, val) && !Equals(val, x));
    /// <summary>
    /// Extension to remove an item at <paramref name="index"/> from a collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="index"></param>
    /// <returns>A collection without the item provided</returns>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IEnumerable<T> RemoveInAt<T>(this IEnumerable<T> values, int index)
    {
        int numeration = 0;
        using (var enumerator = values.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (numeration++ != index)
                    yield return enumerator.Current;
            }
        }
    }
    /// <summary>
    /// Returns the next value from the enumeration for the given value.
    /// </summary>
    /// <typeparam name="T">Enumeration type.</typeparam>
    /// <param name="src">The current value of the enumeration.</param>
    /// <returns>The next value in the enumeration, or the first value if the current value is the last.</returns>
    /// <exception cref="ArgumentException">Thrown if type T is not an enumeration.</exception>
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(string.Format("Argumnent {0} is not an Enum", typeof(T).FullName));
        }

        T[] array = (T[])Enum.GetValues(src.GetType());
        int num = Array.IndexOf<T>(array, src) + 1;

        if (array.Length != num)
        {
            return array[num];
        }

        return array[0];
    }
    ///<summary>
    /// Swaps the elements at the specified positions in the list.
    ///</summary>
    ///<typeparam name="T">The type of elements in the list.</typeparam>
    ///<param name="list">The list in which to perform the swap.</param>
    ///<param name="i">The index of the first element to swap.</param>
    ///<param name="j">The index of the second element to swap.</param>
    public static void Swap<T>(this List<T> list, int i, int j) => (list[j], list[i]) = (list[i], list[j]); // Thanks, compiler!

    /// <summary>
    /// Finds all elements in the list that match the specified predicate and removes them.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list.</typeparam>
    /// <param name="lst">The list to search and modify.</param>
    /// <param name="match">The predicate to match elements against.</param>
    /// <returns>A list of elements that were removed.</returns>
    public static List<T> FindAndRemove<T>(this List<T> lst, Predicate<T> match)
    {
        List<T> result = lst.FindAll(match);
        lst.RemoveAll(match);
        return result;
    }
    /// <summary>
    /// Returns a random element from the given list.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list.</typeparam>
    /// <param name="array">The list to select a random element from.</param>
    /// <returns>A randomly selected element from the list.</returns>
    public static T GetRandom<T>(this IList<T> array) => array[UnityEngine.Random.Range(0, array.Count)];
    /// <summary>
    /// Parses the specified string to the corresponding enum value.
    /// </summary>
    /// <typeparam name="T">The enum type to parse to.</typeparam>
    /// <param name="value">The string representation of the enum value.</param>
    /// <returns>The parsed enum value.</returns>
    public static T ParseEnum<T>(this string value) => (T)Enum.Parse(typeof(T), value, true);
    /// <summary>
    /// Loads all base assets from the mod folder of the specified type.
    /// </summary>
    /// <typeparam name="T">The asset type to load.</typeparam>
    /// <param name="man">AssetManager where assets will be loaded</param>
    public static void LoadAll<T>(this AssetManager man) where T : UnityEngine.Object
    {
        var t = typeof(T);
        //  if (t == typeof(AudioClip))
        //      Utils.GetAllFilesFromFolder("AudioClip").Do(x => man.Add(x.Split('\\').Last(), ContentManager.CreateAudio(x)));
        if (t == typeof(Sprite))
            Utils.GetAllFilesFromFolder("Texture2D").Do(x => man.Add(x.Split('\\').Last(), ContentManager.CreateSprite(x)));
        if (t == typeof(SoundObject))
            Utils.GetAllFilesFromFolder("AudioClip").Do(x => man.Add(x.Split('\\').Last(), ContentManager.CreateSoundObject(x, "", type: SoundType.Voice)));
    }
    /// <summary>
    /// Loads all base assets from the mod folder.
    /// </summary>
    /// <param name="man">AssetManager where assets will be loaded</param>
    public static void LoadAll(this AssetManager man)
    {
        man.LoadAll<Sprite>();
        man.LoadAll<SoundObject>();
    }
    /*
    public static void TargetPosition(this GameObject obj, Vector3 position, float speed)
    {
        float maxDistanceDelta = speed * Time.deltaTime;
        Vector3 vector = position;
        obj.transform.position = Vector3.MoveTowards(obj.transform.position, vector, maxDistanceDelta);
        if (Vector3.Distance(position, vector) < 0.001f)
            vector *= -1f;
    }*/
    /// <summary>
    /// Converts the specified string to the corresponding enum value using custom logic.
    /// </summary>
    /// <typeparam name="T">The enum type to convert to.</typeparam>
    /// <param name="text">The string representation of the enum value.</param>
    /// <returns>The converted enum value.</returns>
    public static T ToEnum<T>(this string text) where T : Enum => EnumExtensions.ExtendEnum<T>(text);
#pragma warning disable IDE0060
    /// <summary>
    /// Returns a random enum value of the specified type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum type instance (not used).</param>
    /// <returns>A randomly selected value from the enum.</returns>
    public static T GetRandom<T>(this T value) where T : Enum
    {
        var array = Enum.GetValues(typeof(T));
        List<T> values = [];
        foreach (T en in array)
            values.Add(en);
        return values.GetRandom();
    }
#pragma warning restore
    /// <summary>
    /// Determines whether the list contains any elements that match the specified predicate.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list.</typeparam>
    /// <param name="list">The list to search.</param>
    /// <param name="predicate">The predicate to match elements against.</param>
    /// <returns>True if any elements match the predicate; otherwise, false.</returns>
    public static bool Contains<T>(this IList<T> list, Predicate<T> predicate)
    {
        foreach (var b in list)
        {
            if (predicate(b))
                return true;
        }
        return false;
    }
    /// <summary>
    /// Removes all child objects containing the specified name.
    /// </summary>
    /// <param name="t">Target transform</param>
    /// <param name="names">Names of childs transforms.</param>
    public static void RemoveChildsContainingNames(this Transform t, IList<string> names)
    {
        var e = t.transform.GetEnumerator();
        while (e.MoveNext())
        {
            foreach (var name in names)
                if (((Transform)e.Current).ToString().Contains(name))
                    UnityEngine.Object.Destroy(((Transform)e.Current).gameObject);
        }
    }
    /// <summary>
    /// Add text under the sprite menu button.
    /// </summary>
    /// <param name="but">Target button.</param>
    /// <param name="text">Text to add.</param>
    /// <param name="size">Size of the text.</param>
    /// <param name="color">Color of the text.</param>
    /// <param name="offset">The text offset from the middle of the button.</param>
    /// <param name="fontStyle">Font style of the text.</param>
    /// <returns></returns>
    public static StandardMenuButton AddText(this StandardMenuButton but, string text, float size, Color? color = null, Vector2? offset = null, FontStyles fontStyle = FontStyles.Normal)
    {
        var txt = new GameObject("CustomButText").AddComponent<TextMeshProUGUI>();
        txt.transform.SetParent(but.transform, false);
        txt.fontSize = size;
        txt.color = color ?? Color.black;
        txt.text = text;
        txt.transform.localPosition += (Vector3)(offset ?? Vector3.zero);
        return but;
    }
    /// <summary>
    /// Tries to add a key-value pair to the dictionary. 
    /// If the key already exists, logs a warning instead of adding the value.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    /// <param name="dict">The dictionary to add the key-value pair to.</param>
    /// <param name="key">The key to add to the dictionary.</param>
    /// <param name="val">The value associated with the key.</param>
    public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue val)
    {
        if (!dict.ContainsKey(key))
            dict.Add(key, val);
        else
            Debug.LogWarning("The dictionary already contains the key: " + key);
    }

    /// <summary>
    /// Counts the number of occurrences of a specified substring within a given input string.
    /// </summary>
    /// <param name="input">The string to search within.</param>
    /// <param name="substring">The substring to count occurrences of.</param>
    /// <returns>The number of times the substring occurs in the input string.</returns>
    public static int CountSubs(this string input, string substring)
    {
        int count = 0;
        int index = 0;

        while ((index = input.IndexOf(substring, index)) != -1)
        {
            count++;
            index += substring.Length;
        }
        return count;
    }
    /// <summary>
    /// Adds a component if it does not exist. If there is, returns an existing one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static T TryAddComponent<T>(this GameObject obj) where T : Component
    {
        if (obj == null)
            throw new NullReferenceException("Yo, man! We don't accept objects equal to null! Bye!");

        if (obj.GetComponent<T>() is null)
            return obj.AddComponent<T>();

        return obj.GetComponent<T>();
    }

    public static string ArrayToString<T>(this IList<T> list)
    {
        string res = string.Empty;
        using var en = list.GetEnumerator();
        while (en.MoveNext())
            res += en.Current.ToString() + "\n";

        return res;
    }
}
