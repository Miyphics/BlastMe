using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHud : MonoBehaviour, IAnimatedHud
{
    [SerializeField] private ShopHud shopPanel;
    public ShopHud ShopPanel => shopPanel;
    [SerializeField] private SettingsHud settingsPanel;
    public SettingsHud SettingsPanel => settingsPanel;
    [SerializeField] private ScoresHud scoresHud;
    public ScoresHud ScoresHud => scoresHud;
    [Space, SerializeField] private RectTransform rightButtonsPanel;
    private Vector2 defaultRightButtonsPos;
    private Vector2 defaultRightButtonsSize;
    [SerializeField] private RectTransform linesPanel;
    private Vector2 defaultLinesPanelPos;
    private Vector2 defaultLinesPanelSize;
    [SerializeField] private RectTransform playButton;
    private Vector2 defaultPlayButtonSize;

    private void Awake()
    {
        defaultRightButtonsPos = rightButtonsPanel.anchoredPosition;
        defaultRightButtonsSize = rightButtonsPanel.sizeDelta;
        defaultPlayButtonSize = playButton.sizeDelta;

        defaultLinesPanelPos = linesPanel.anchoredPosition;
        defaultLinesPanelSize = linesPanel.sizeDelta;
    }

    public void ToggleAnim(bool enable, bool force = false)
    {
        ToggleAnim(enable, null, force);
    }

    public void ToggleAnim(bool enable, Action actionAfterAnim, bool force = false)
    {
        if (!GameManager.Instance.HudManager.IsAnimated || force)
        {
            var playButComp = playButton.GetComponent<AnimatedButton>();

            List<HudManager.RectTransformAnimated> anims;

            if (enable)
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = rightButtonsPanel, position = defaultRightButtonsPos, size = defaultRightButtonsSize },
                    new HudManager.RectTransformAnimated() { rectTransform = playButton, position = playButton.anchoredPosition, size = defaultPlayButtonSize },
                    new HudManager.RectTransformAnimated() { rectTransform = linesPanel, position = defaultLinesPanelPos, size = defaultLinesPanelSize },

                };

                actionAfterAnim += () =>
                {
                    playButComp.ShowObjectOnButton();
                };
            }
            else
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = rightButtonsPanel, position = new(defaultRightButtonsPos.x, 0), size = new(defaultPlayButtonSize.x, 0) },
                    new HudManager.RectTransformAnimated() { rectTransform = playButton, position = playButton.anchoredPosition, size = new(0, defaultPlayButtonSize.y) },
                    new HudManager.RectTransformAnimated() { rectTransform = linesPanel, position = new(defaultLinesPanelPos.x, 26), size = new(0, 0) },
                };

                playButComp.HideObjectOnButton();
            }

            GameManager.Instance.HudManager.AnimateRectTransforms(transform, anims, enable, actionAfterAnim, force);
        }
    }

    public void OpenSettings()
    {
        ToggleAnim(false, () => settingsPanel.ToggleAnim(true));
        GameManager.Instance.HudManager.HideScoreText();
    }

    public void CloseSettings()
    {
        if (!GameManager.Instance.HudManager.ConfirmationWindow.Hidden)
        {
            GameManager.Instance.HudManager.ConfirmationWindow.ToggleAnim(false, true);
        }

        settingsPanel.ToggleAnim(false, () =>
        {
            ToggleAnim(true);

            if (GameManager.Instance.HudManager.scoreTextShowed)
                GameManager.Instance.HudManager.ShowScoreText();
        }, true);
    }

    public void OpenShop()
    {
        ToggleAnim(false, () => shopPanel.ToggleAnim(true));
        GameManager.Instance.HudManager.HideScoreText();
    }

    public void CloseShop()
    {

        if (!GameManager.Instance.HudManager.ConfirmationWindow.Hidden)
        {
            GameManager.Instance.HudManager.ConfirmationWindow.ToggleAnim(false, true);
        }

        shopPanel.ToggleAnim(false, () =>
        {
            ToggleAnim(true);

            if (GameManager.Instance.HudManager.scoreTextShowed)
                GameManager.Instance.HudManager.ShowScoreText();
        }, true);
    }

    public void OpenScores()
    {
        ToggleAnim(false, () => scoresHud.ToggleAnim(true));
        GameManager.Instance.HudManager.HideScoreText();
    }

    public void CloseScores()
    {
        if (!GameManager.Instance.HudManager.ConfirmationWindow.Hidden)
        {
            GameManager.Instance.HudManager.ConfirmationWindow.ToggleAnim(false, true);
        }

        scoresHud.ToggleAnim(false, () =>
        {
            ToggleAnim(true);

            if (GameManager.Instance.HudManager.scoreTextShowed)
                GameManager.Instance.HudManager.ShowScoreText();
        }, true);
    }

    public void UpdateCountOfLines()
    {
        switch (GameManager.Instance.PlayerManager.NumberOfLines)
        {
            case 1:
                linesPanel.GetChild(0).GetComponent<RadioButton>().IsChecked = true;
                linesPanel.GetChild(1).GetComponent<RadioButton>().IsChecked = false;
                linesPanel.GetChild(2).GetComponent<RadioButton>().IsChecked = false;
                break;

            case 3:
                linesPanel.GetChild(0).GetComponent<RadioButton>().IsChecked = false;
                linesPanel.GetChild(1).GetComponent<RadioButton>().IsChecked = false;
                linesPanel.GetChild(2).GetComponent<RadioButton>().IsChecked = true;
                break;

            case 2:
            default:
                linesPanel.GetChild(0).GetComponent<RadioButton>().IsChecked = false;
                linesPanel.GetChild(1).GetComponent<RadioButton>().IsChecked = true;
                linesPanel.GetChild(2).GetComponent<RadioButton>().IsChecked = false;
                break;
        }
    }
}
