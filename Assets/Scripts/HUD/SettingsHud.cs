using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsHud : MonoBehaviour, IAnimatedHud
{
    [SerializeField] private Checkbox musicCheckBox;
    [SerializeField] private Checkbox batterySavingCheckBox;
    [SerializeField] private GameObject musicVolumePanel;
    [SerializeField] private GameObject resetButtonObject;
    [SerializeField] private GameObject backToMenuObject;

    private Action closeAction;

    private Color defaultBackgroundColor;
    private Vector2 defaultSettingsPanelPos;
    private Vector2 defaultSettingsPanelSize;

    private void Awake()
    {
        defaultBackgroundColor = transform.GetChild(0).GetComponent<Image>().color;

        var settingsPanel = musicCheckBox.transform.parent.parent.GetComponent<RectTransform>();
        defaultSettingsPanelPos = settingsPanel.anchoredPosition;
        defaultSettingsPanelSize = settingsPanel.sizeDelta;

        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            batterySavingCheckBox.transform.parent.gameObject.SetActive(false);
        }
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

            var background = transform.GetChild(0).GetComponent<RectTransform>();
            var defCol = defaultBackgroundColor;
            var settingsPanel = musicCheckBox.transform.parent.parent.GetComponent<RectTransform>();
            var animSpeed = GameManager.Instance.HudManager.HudAnimSpeed;

            LeanTween.cancel(background);

            if (enable)
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = settingsPanel, position = defaultSettingsPanelPos, size = defaultSettingsPanelSize },
                };

                musicCheckBox.IsChecked = GameManager.Instance.MusicManager.CanPlayMusic;
                batterySavingCheckBox.IsChecked = GameManager.Instance.BatterySaving;
                SetMusicVolumeSliderValue(GameManager.Instance.MusicManager.GetVolume());

                // If called from pause
                if (closeAction != null)
                {
                    if (resetButtonObject.activeSelf)
                        resetButtonObject.SetActive(false);

                    if (!backToMenuObject.activeSelf)
                        backToMenuObject.SetActive(true);
                }
                else
                {
                    if (!resetButtonObject.activeSelf)
                        resetButtonObject.SetActive(true);

                    if (backToMenuObject.activeSelf)
                        backToMenuObject.SetActive(false);
                }

                LeanTween.color(background, defCol, animSpeed).setIgnoreTimeScale(true);
            }
            else
            {
                closeAction = null;

                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = settingsPanel, position = new(defaultSettingsPanelPos.x, 170), size = new(defaultSettingsPanelSize.x, -620) },
                };

                LeanTween.color(background, new Color(defCol.r, defCol.g, defCol.b, 0), animSpeed).setIgnoreTimeScale(true);
            }

            GameManager.Instance.HudManager.AnimateRectTransforms(transform, anims, enable, actionAfterAnim, force);
        }
    }

    public void ToggleMusic()
    {
        GameManager.Instance.MusicManager.CanPlayMusic = musicCheckBox.IsChecked;
        GameManager.Instance.PlayerManager.DeferredSaveSettings();
        ToggleMusicVolumePanel(musicCheckBox.IsChecked);
    }

    public void ToggleMusic(bool enable)
    {
        GameManager.Instance.MusicManager.CanPlayMusic = enable;
        musicCheckBox.IsChecked = enable;
        ToggleMusicVolumePanel(musicCheckBox.IsChecked);
    }

    public void SetMusicVolime(float volume)
    {
        GameManager.Instance.MusicManager.SetVolume(volume);
        GameManager.Instance.PlayerManager.DeferredSaveSettings();
    }

    public void SetMusicVolumeSliderValue(float value)
    {
        musicVolumePanel.transform.Find("Slider").GetComponent<Slider>().value = value;
    }

    public void ToggleMusicVolumePanel(bool enable)
    {
        if (musicVolumePanel.activeSelf != enable)
        {
            musicVolumePanel.SetActive(enable);
        }
    }

    public void ToggleBatterySaving()
    {
        GameManager.Instance.BatterySaving = batterySavingCheckBox.IsChecked;
        GameManager.Instance.PlayerManager.DeferredSaveSettings();
    }

    public void ToggleBatterySaving(bool enable)
    {
        GameManager.Instance.BatterySaving = enable;
        batterySavingCheckBox.IsChecked = enable;
    }

    public void ResetProgress()
    {
        if (closeAction != null)
            return;

        GameManager.Instance.HudManager.ConfirmationWindow.SetInfo(LocalizationManager.Instance.GetLocalizedText("info.resetProgress"), () =>
        {
            PlayerData data = new();
            SaveManager.SaveData(data, true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    public void ClosePanel()
    {
        if (closeAction == null)
        {
            GameManager.Instance.HudManager.MenuHud.CloseSettings();
            return;
        }

        closeAction.Invoke();
    }

    public void SetCloseAction(Action action)
    {
        closeAction = action;
    }

    public void GoToMenu()
    {
        if (GameManager.Instance.HudManager.ConfirmationWindow.Hidden)
        {
            GameManager.Instance.HudManager.ConfirmationWindow.SetInfo(LocalizationManager.Instance.GetLocalizedText("info.goToMenu"), () =>
            {
                GameManager.Instance.PlayerManager.TogglePause(false, true);
            });
        }
    }
}
