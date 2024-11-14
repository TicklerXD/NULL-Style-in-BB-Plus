using DevTools;
using DevTools.Extensions;
using HarmonyLib;
using MTM101BaldAPI;
using NULL.Manager.CompatibilityModule;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static NULL.Manager.ModManager;
using static UnityEngine.Object;

namespace NULL.ModPatches;

[ConditionalPatchAlways]
[HarmonyPatch(typeof(MainModeButtonController), "OnEnable")]
internal class MenuPatcher
{
    internal static GameObject styleSelect;
    static void Finalizer(MainModeButtonController __instance)
    {
        var pickMode = FindObjectOfType<MainModeButtonController>(true);

        var desc = __instance.gameObject.transform.Find("ModeText");
        if (desc.GetComponent<DescriptionFix>() is null) // Very dump solution, YEAH, YEAH, LMAO!
            desc.gameObject.AddComponent<DescriptionFix>();

        var butPre = __instance.gameObject.transform.Find("Free");

        if (__instance.gameObject.transform.Find("MainNull") != null) return;

        StandardMenuButton but = Object.Instantiate(butPre.gameObject).GetComponent<StandardMenuButton>();
        but.transform.parent = butPre.transform.parent;
        but.transform.localScale = butPre.transform.localScale;
        but.OnPress = new UnityEvent();
        but.OnHighlight = new UnityEvent();
        but.transform.localPosition = new Vector3(-.057f, -24.85f);
        but.transform.SetSiblingIndex(1);
        but.name = __instance.mainNew.activeSelf ? "MainNull" : "MainNullContinue";
        var menuUI = but.gameObject.AddComponent<ContentManager.MenuUI>();
        menuUI.localizedText = "But_NullMode";
        but.OnHighlight.AddListener(() => desc.gameObject.GetComponent<TextLocalizer>().GetLocalizedText("Men_NullDesc"));
        but.transitionOnPress = true;
        but.OnPress.AddListener(() =>
        {
            GeneratePickMode();
            pickMode.gameObject.SetActive(false);
            styleSelect.SetActive(true);
        });
    }
    internal static void GeneratePickMode()
    {
        if (styleSelect != null) return;

        var pickMode = Instantiate(m.Get<GameObject>("PickPre"));
        var nullBut = ContentManager.CreateMenuButton(pickMode, "NullStory", Vector3.left * 100f, Vector2.one * 256, true, m.Get<Sprite>("ull"), () =>
        {
            if (ImprovedLvlLoaderCompat.CustomLevelCheck(true))
                return;

            Destroy(styleSelect);
            NullStyle = true;
            ExtraVariables.LoadGame(customScene: nullScenes[0]);

        }).AddText("NULL STYLE", 24, offset: new Vector3(25, -135));
        var glitchBut = ContentManager.CreateMenuButton(pickMode, "GlitchStory", Vector2.right * 100f, Vector2.one * 220, true, m.Get<Sprite>("BaldloonRed"), () =>
        {
            if (ImprovedLvlLoaderCompat.CustomLevelCheck(true))
                return;

            Destroy(styleSelect);
            GlitchStyle = true;
            ExtraVariables.LoadGame(customScene: nullScenes[0]);
                       
        }).AddText("GLITCH STYLE", 24, offset: new Vector3(25, -135));


        void AddHighEventToButton(StandardMenuButton but)
        {
            but.eventOnHigh = true;
            but.OnHighlight = new();
            but.OnHighlight.AddListener(() =>
            {
                but.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                but.GetComponentInChildren<Image>().SetAlpha(.69f);
            });
            but.OffHighlight = new();
            but.OffHighlight.AddListener(() =>
            {
                but.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
                but.GetComponentInChildren<Image>().SetAlpha(1);
            });
        }

        AddHighEventToButton(nullBut);
        AddHighEventToButton(glitchBut);

        var backBut = pickMode.transform.Find("BackButton").GetComponent<StandardMenuButton>();
        backBut.OnPress = new();
        backBut.OnPress.i().AddListener(() =>
        {
            FindObjectOfType<MainModeButtonController>(true).gameObject.SetActive(true);
            styleSelect.SetActive(false);
        });

        styleSelect = pickMode;
        styleSelect.SetActive(false);
    }

    class DescriptionFix : MonoBehaviour // Very dump solution, YEAH, YEAH, LMAO!
    {
        void Start()
        {
            gameObject.transform.position -= Vector3.up * 120;
            gameObject.transform.localScale -= new Vector3(.25f, .25f, .25f);
        }
    }
}

