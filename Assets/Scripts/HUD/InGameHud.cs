using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameHud : MonoBehaviour, IAnimatedHud
{
    [SerializeField] private RectTransform heartTransform;
    private Vector2 defaultHeartPos;
    private Vector2 defaultHeartSize;
    private bool heartAnimated;

    [SerializeField] private RectTransform pauseButton;
    private Vector2 defaultPauseButtonPos;
    private Vector2 defaultPauseButtonSize;

    [SerializeField] private GameObject ballonTexts;

    private bool hidden;
    public bool Hidden => hidden;

    private void Awake()
    {
        defaultHeartPos = heartTransform.anchoredPosition;
        defaultHeartSize = heartTransform.sizeDelta;

        defaultPauseButtonPos = pauseButton.anchoredPosition;
        defaultPauseButtonSize = pauseButton.sizeDelta;
    }

    private void Start()
    {
        HudManager.ToggleAllChilds(transform, false);
        ToggleAnim(false, force: true);
    }

    public void ToggleAnim(bool enable, bool force = false)
    {
        ToggleAnim(enable, null, force);
    }

    public void ToggleAnim(bool enable, Action actionAfterAnim, bool force = false)
    {
        if (!GameManager.Instance.HudManager.IsAnimated || force)
        {
            hidden = !enable;

            List<HudManager.RectTransformAnimated> anims;

            if (enable)
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = heartTransform, position = defaultHeartPos, size = defaultHeartSize },
                    new HudManager.RectTransformAnimated() { rectTransform = pauseButton, position = defaultPauseButtonPos, size = defaultPauseButtonSize }
                };
            }
            else
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = heartTransform, position = new(defaultHeartPos.x, 0), size = new(defaultHeartSize.x, 0) },
                    new HudManager.RectTransformAnimated() { rectTransform = pauseButton, position = new(defaultPauseButtonPos.x, 0), size = new(defaultPauseButtonSize.x, 0) }
                };
            }

            GameManager.Instance.HudManager.AnimateRectTransforms(transform, anims, enable, actionAfterAnim, force);
        }
    }

    public void UpdateHealth(bool onlyText = false)
    {
        var healthText = heartTransform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (onlyText)
        {
            healthText.text = GameManager.Instance.PlayerManager.Health.ToString();
            return;
        }
        else if (!heartAnimated)
        {
            heartAnimated = true;

            var size = 10f;
            LeanTween.size(heartTransform, new Vector2(defaultHeartSize.x + size, defaultHeartSize.y + size), 0.35f).setEase(LeanTweenType.easeInOutCirc).setLoopPingPong(1).setOnComplete(() =>
            {
                heartAnimated = false;

                healthText.text = GameManager.Instance.PlayerManager.Health.ToString();

                if (GameManager.Instance.PlayerManager.Health <= 0)
                {
                    GameManager.Instance.HudManager.ShowGameOver();
                    ToggleBallonsText(false);

                    //GameManager.Instance.HudManager.ShowMenuHud();
                }
            });
        }
    }

    public void ToggleBallonsText(bool enable)
    {
        if (ballonTexts.activeSelf != enable)
            ballonTexts.SetActive(enable);
    }
}
