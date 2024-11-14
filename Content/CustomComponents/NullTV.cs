using DevTools.Extensions;
using NULL.Manager;
using UnityEngine;

namespace NULL.CustomComponents;

[RequireComponent(typeof(BaldiTV))]
public class NullTV : MonoBehaviour
{
    [SerializeField] Sprite balSpr; // Orig Baldi sprite
    [SerializeField] Sprite nullSpr;
    [SerializeField] BaldiTV tv;
#pragma warning disable IDE0051
    void Start()
    {
        tv = GetComponent<BaldiTV>();
        balSpr = tv.baldiImage.sprite;
        nullSpr = ModManager.m.Get<Sprite>((ModManager.GlitchStyle ? "Glitch" : "Null") + "TV");
        Reset(true);
    }
    void Reset(bool nullStyle = false) // Reset Baldi image or Null image
    {
        if (tv == null)
            tv = GetComponent<BaldiTV>();
        if (!nullStyle)
        {
            tv.ResetScreen();
            tv.baldiImage.SetAlpha(1);
            tv.baldiImage.sprite = balSpr;
            tv.staticImage.rectTransform.SetSiblingIndex(tv.baldiImage.rectTransform.GetSiblingIndex());
            return;
        }
        tv.baldiImage.sprite = nullSpr;
        tv.baldiImage.SetAlpha(0.5f);
        tv.baldiImage.rectTransform.SetSiblingIndex(tv.staticImage.rectTransform.GetSiblingIndex());
    }
    void OnDestroy() => Reset();
    void OnDisable() => Reset();
    void LateUpdate()
    {
        if (nullSpr && tv.baldiImage.sprite != nullSpr)
            tv.baldiImage.sprite = nullSpr;
    }
#pragma warning restore
}
