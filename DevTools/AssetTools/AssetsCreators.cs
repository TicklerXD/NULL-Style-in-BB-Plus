using MTM101BaldAPI;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static MTM101BaldAPI.AssetTools.AssetLoader;
using static NULL.BasePlugin;
using static NULL.Manager.ModManager;
using static PixelInternalAPI.Extensions.GenericExtensions;
using static UnityEngine.Object;

namespace DevTools;

public partial class ContentManager
{
    /// <summary>
    /// Just creates a sprite. An analog of SpriteFromMod from MTM101API.
    /// </summary>
    /// <param name="name"> The name of the file located in the Texture2D folder. </param>
    /// <param name="pixelsPerUnits"></param>
    /// <returns>Sprite with <paramref name="name"/></returns>
    public static Sprite CreateSprite(string name, float pixelsPerUnits = 100f) => SpriteFromTexture2D(TextureFromFile(Path.Combine(ModPath, "Texture2D", name + ".png")), pixelsPerUnits);
    /// <summary>
    /// Just creates a AudioClip. An analog of AudioClipFromMod from MTM101API, lol.
    /// </summary>
    /// <param name="name"> The name of the file located in the AudioClip folder.</param>
    /// <returns>AudioClip with <paramref name="name"/></returns>
    public static AudioClip CreateAudio(string name) => AudioClipFromFile(Path.Combine(ModPath, "AudioClip", name + ".wav"));
    /// <summary>
    /// Creates a SoundObject from AudioClip.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="subtitle">Key of captions from JSON file from Language folder.</param>
    /// <param name="type">Type of the sound.</param>
    /// <param name="color">Color of captions.</param>
    /// <param name="sublength">Duration of captions.</param>
    /// <returns></returns>
    public static SoundObject CreateSoundObject(AudioClip clip, string subtitle = "", SoundType type = SoundType.Effect, Color? color = null, float sublength = -1f)
    {
        var soundObject = ScriptableObject.CreateInstance<SoundObject>();
        soundObject.name = clip.name;
        soundObject.soundClip = clip;
        soundObject.subDuration = ((sublength == -1f) ? (clip.length + 1f) : sublength);
        soundObject.soundType = type;
        soundObject.soundKey = subtitle;
        soundObject.color = (color ?? Color.white);
        soundObject.subtitle = (subtitle != "");
        return soundObject;
    }
    /// <summary>
    /// Creates a SoundObject from name.
    /// </summary>
    /// <param name="name">The name of the file in the AudioClip folder.</param>
    /// <param name="subtitle">Key of captions from JSON file from Language folder.</param>
    /// <param name="type">Type of the sound.</param>
    /// <param name="color">Color of captions.</param>
    /// <param name="sublength">Duration of captions.</param>
    /// <returns></returns>
    public static SoundObject CreateSoundObject(string name, string subtitle = "", SoundType type = SoundType.Effect, Color? color = null, float sublength = -1f) =>
        CreateSoundObject(CreateAudio(name), subtitle, type, color, sublength);
    /// <summary>
    /// Creates a looping sound.
    /// </summary>
    /// <param name="clips">All clips are played, but the last one is looped.</param>
    /// <returns></returns>
    public static LoopingSoundObject CreateLoopingSoundObject(params string[] clips)
    {
        var loop = ScriptableObject.CreateInstance<LoopingSoundObject>();
        var list = new List<AudioClip>();
        foreach (var clip in clips) list.Add(CreateAudio(clip));
        loop.clips = list.ToArray();
        loop.mixer = FindResourceObjectByName<AudioMixerGroup>("Master");
        return loop;
    }
    /// <summary>
    /// Creates a custom game canvas.
    /// </summary>
    /// <param name="name">The name of the canvas.</param>
    /// <returns></returns>
    public static Canvas CreateGameCanvas(string name = "BaseCanvas")
    {
        var canvas = Instantiate(FindResourceObjectByName<GameObject>("GumOverlay"));
        Destroy(canvas.GetComponentInChildren<Image>());
        canvas.SetActive(false);
        canvas.name = name;
        canvas.GetComponent<Canvas>().worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(0).canvasCam;
        canvas.ConvertToPrefab(true);
        return canvas.GetComponent<Canvas>();
    }

    /// <summary>
    /// Creates a custom game mode selection button with text.
    /// </summary>
    /// <param name="pickMode">Parent pick mode.</param>
    /// <param name="pos">Position of the button.</param>
    /// <param name="size">Size of the button.</param>
    /// <param name="text">Text on the button.</param>
    /// <param name="OnPress">The action that is performed on click.</param>
    /// <param name="OnHighlight">The action performed on hover.</param>
    /// <param name="OffHighlight">The action that is performed on unhover.</param>
    /// <returns></returns>
    public static StandardMenuButton CreateMenuButton(GameObject pickMode, Vector2 pos, Vector2 size, string text, Action OnPress, Action OnHighlight = null, Action OffHighlight = null)
    {
        var butPre = m.Get<StandardMenuButton>("PickButPre");
        StandardMenuButton but = Instantiate(butPre.gameObject).GetComponent<StandardMenuButton>();
        but.transform.SetParent(pickMode.transform, true);
        but.transform.localScale = size;
        but.OnPress = new();
        but.OnHighlight = new();
        but.OffHighlight = new();
        but.transform.localPosition = pos;
        but.transform.SetSiblingIndex(1);
        but.name = "CustomBut_" + text;
        var ui = but.gameObject.AddComponent<MenuUI>();
        ui.localizedText = text;

        if (OnHighlight != null && OffHighlight != null) but.eventOnHigh = true;
        but.OnHighlight.AddListener(() => OnHighlight());
        but.OffHighlight.AddListener(() => OffHighlight());
        but.OnPress.AddListener(() => OnPress());

        return but;
    }
    /// <summary>
    /// Creates a custom game mode selection button with image.
    /// </summary>
    /// <param name="pickMode">Parent pick mode.</param>
    /// <param name="pos">Position of the button.</param>
    /// <param name="size">Size of the button.</param>
    /// <param name="isRectTrasnform">Should use RectTransform or simple Transform?</param>
    /// <param name="img">Sprite for the button.</param>
    /// <param name="OnPress">The action that is performed on click.</param>
    /// <param name="OnHighlight">The action performed on hover.</param>
    /// <param name="OffHighlight">The action that is performed on unhover.</param>
    /// <returns></returns>
    public static StandardMenuButton CreateMenuButton(GameObject pickMode, string name, Vector2 pos, Vector2 size, bool isRectTrasnform, Sprite img, Action OnPress, Action OnHighlight = null, Action OffHighlight = null)
    {
        var sprBut = new GameObject("CustomBut_" + name).AddComponent<Image>();
        sprBut.sprite = img;
        sprBut.SetNativeSize();

        var but = sprBut.gameObject.ConvertToButton<StandardMenuButton>();
        but.OffHighlight = new();
        but.transform.SetParent(pickMode.transform, true);
        but.eventOnHigh = OnHighlight != null ? true : false;
        if (but.eventOnHigh && OnHighlight is not null && OffHighlight is not null)
        {
            but.OnHighlight.AddListener(() => OnHighlight());
            but.OffHighlight.AddListener(() => OffHighlight());
        }
        but.OnPress.AddListener(() => OnPress());
        if (!isRectTrasnform)
        {
            but.transform.localScale = size;
            but.transform.localPosition = pos;
        }
        else
        {
            but.transform.localScale = Vector3.one;
            but.transform.localPosition = Vector3.zero;
            var rectT = but.GetComponent<RectTransform>();
            rectT.anchoredPosition = pos;
            rectT.sizeDelta = size;
        }

        but.transform.SetSiblingIndex(1);
        but.name = "CustomBut_" + name;
        return but;
    }
    public static StandardMenuButton CreateMenuButton(GameObject pickMode, string name, Vector3 anchoredPos, Vector2 sizeDelta, Sprite img, Action OnPress, Action OnHighlight = null, Action OffHighlight = null)
    {
        var b = CreateMenuButton(pickMode, name, Vector2.zero, Vector2.one, img, OnPress, OnHighlight, OffHighlight);
        var rectT = b.GetComponent<RectTransform>();
        rectT.anchoredPosition = anchoredPos;
        rectT.sizeDelta = sizeDelta;
        return b;
    }
    public static StandardMenuButton CreateMenuButton(GameObject pickMode, Vector3 anchoredPos, Vector2 sizeDelta, string text, Action OnPress, Action OnHighlight = null, Action OffHighlight = null)
    {
        var b = CreateMenuButton(pickMode, Vector2.zero, Vector2.one, text, OnPress, OnHighlight, OffHighlight);
        var rectT = b.GetComponent<RectTransform>();
        rectT.anchoredPosition = anchoredPos;
        rectT.sizeDelta = sizeDelta;
        return b;
    }
    public class MenuUI : MonoBehaviour // Component for properly displaying localized text on menu buttons
    {
    public string localizedText = "Text";
    void Update() => gameObject.GetComponent<TextLocalizer>()?.GetLocalizedText(localizedText);
}
}
