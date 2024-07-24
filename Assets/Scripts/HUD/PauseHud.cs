using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseHud : MonoBehaviour, IAnimatedHud
{
    [SerializeField] private RectTransform unpauseButton;
    private Vector2 defaultUnpauseButtonPos;
    private Vector2 defaultUnpauseButtonSize;

    private Color defaultBackgroundColor;

    private void Awake()
    {
        defaultBackgroundColor = transform.GetChild(0).GetComponent<Image>().color;

        defaultUnpauseButtonPos = unpauseButton.anchoredPosition;
        defaultUnpauseButtonSize = unpauseButton.sizeDelta;
    }

    private void Start()
    {
        HudManager.ToggleAllChilds(transform, false);
        ToggleAnim(false);
    }

    public void ToggleAnim(bool enable, bool force = true)
    {
        ToggleAnim(enable, null, force);
    }

    public void ToggleAnim(bool enable, Action actionAfterAnim, bool force = true)
    {
        var bg = transform.GetChild(0).GetComponent<RectTransform>();
        var animSpeed = GameManager.Instance.HudManager.HudAnimSpeed;

        List<HudManager.RectTransformAnimated> anims;

        LeanTween.cancel(bg);

        if (enable)
        {
            anims = new()
            {
                new HudManager.RectTransformAnimated() { rectTransform = unpauseButton, position = defaultUnpauseButtonPos, size = defaultUnpauseButtonSize },
            };

            LeanTween.color(bg, defaultBackgroundColor, animSpeed).setIgnoreTimeScale(true);
        }
        else
        {
            anims = new()
            {
                new HudManager.RectTransformAnimated() { rectTransform = unpauseButton, position = new(defaultUnpauseButtonPos.x, 0), size = new(defaultUnpauseButtonSize.x, 0) },
            };

            LeanTween.color(bg, new(defaultBackgroundColor.r, defaultBackgroundColor.g, defaultBackgroundColor.b, 0), animSpeed).setIgnoreTimeScale(true);
        }

        GameManager.Instance.HudManager.AnimateRectTransforms(transform, anims, enable, actionAfterAnim, force);
    }
}
