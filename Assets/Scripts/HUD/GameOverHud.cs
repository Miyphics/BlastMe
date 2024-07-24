using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverHud : MonoBehaviour, IAnimatedHud
{
    [SerializeField] private RectTransform gameOverText;
    private Vector2 defaultGOTextSize;

    private void Awake()
    {
        defaultGOTextSize = gameOverText.sizeDelta;
    }

    private void Start()
    {
        HudManager.ToggleAllChilds(transform, false);
        ToggleAnim(false, true);
    }

    public void ToggleAnim(bool enable, bool force = false)
    {
        ToggleAnim(enable, null, force);
    }

    public void ToggleAnim(bool enable, Action actionAfterAnim, bool force = false)
    {
        if (!GameManager.Instance.HudManager.IsAnimated || force)
        {
            List<HudManager.RectTransformAnimated> anims;

            if (enable)
            {
                StartCoroutine(ToggleOffIE());

                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = gameOverText, position = gameOverText.anchoredPosition, size = defaultGOTextSize },
                };
            }
            else
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = gameOverText, position = gameOverText.anchoredPosition, size = new(0, 0) },
                };
            }

            GameManager.Instance.HudManager.AnimateRectTransforms(transform, anims, enable, actionAfterAnim, force, 0.7f);
        }
    }

    private IEnumerator ToggleOffIE()
    {
        yield return new WaitForSeconds(3f);
        GameManager.Instance.HudManager.InGameHud.ToggleAnim(false, true);
        ToggleAnim(false, () =>
        {
            GameManager.Instance.HudManager.ShowMainMenu(() =>
            {
                GameManager.Instance.PlayerManager.RestartGame();
            });

            if (GameManager.Instance.PlayerManager.CanShowAds)
            {
                GameManager.Instance.PlayerManager.ShowAds();
            }

        }, true);
    }
}
