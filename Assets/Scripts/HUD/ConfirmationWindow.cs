using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfirmationWindow : MonoBehaviour, IAnimatedHud
{
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private AnimatedButton buttonConfirm;
    [SerializeField] private AnimatedButton buttonCancel;

    private RectTransform mainWindow;
    private Vector2 defaultWindowPos;
    private Vector2 defaultWindowSize;

    public bool Hidden { get; private set; }

    private void Awake()
    {
        buttonConfirm.onClick.AddListener(() => ToggleAnim(false, true));
        buttonCancel.onClick.AddListener(() => ToggleAnim(false, true));

        mainWindow = transform.GetChild(0).GetComponent<RectTransform>();
        defaultWindowPos = mainWindow.anchoredPosition;
        defaultWindowSize = mainWindow.sizeDelta;
    }

    private void Start()
    {
        HudManager.ToggleAllChilds(transform, false);
        ToggleAnim(false, true);
    }

    public void SetInfo(string mainText, Action confirmAction)
    {
        confirmationText.text = mainText;
        SetConfirmAction(confirmAction);
        ToggleAnim(true);
    }

    public void SetInfo(string mainText, Action confirmAction, bool buttonOk)
    {
        SetInfo(mainText, confirmAction);

        if (buttonOk)
        {
            buttonCancel.gameObject.SetActive(false);
            SetConfirmButtonText(LocalizationManager.Instance.GetLocalizedText("Ok"));
        }
    }

    public void SetInfo(string mainText, Action confirmAction, Action cancelAction)
    {
        SetCancelAction(cancelAction);
        SetInfo(mainText, confirmAction);
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

            Hidden = !enable;

            if (enable)
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = mainWindow, position = mainWindow.anchoredPosition, size = defaultWindowSize },
                };
            }
            else
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = mainWindow, position = mainWindow.anchoredPosition, size = new(defaultWindowSize.x, -750) },
                };

                actionAfterAnim += () =>
                {
                    SetConfirmButtonText(LocalizationManager.Instance.GetLocalizedText("Yes"));
                    SetCancelButtonText(LocalizationManager.Instance.GetLocalizedText("No"));
                };
            }

            GameManager.Instance.HudManager.AnimateRectTransforms(transform, anims, enable, actionAfterAnim, force);
        }
    }

    public void SetConfirmAction(Action action)
    {
        buttonConfirm.onClick.RemoveAllListeners();
        buttonConfirm.onClick.AddListener(() => ToggleAnim(false, true));

        if (action != null)
            buttonConfirm.onClick.AddListener(() => action.Invoke());
    }

    public void SetCancelAction(Action action)
    {
        buttonCancel.onClick.RemoveAllListeners();
        buttonCancel.onClick.AddListener(() => ToggleAnim(false, true));

        if (action != null)
            buttonCancel.onClick.AddListener(() => action.Invoke());
    }

    public void SetConfirmButtonText(string text)
    {
        buttonConfirm.ObjectOnButton.GetComponent<TextMeshProUGUI>().text = text;
        if (!buttonConfirm.gameObject.activeSelf)
            buttonConfirm.gameObject.SetActive(true);
    }

    public void SetCancelButtonText(string text)
    {
        buttonCancel.ObjectOnButton.GetComponent<TextMeshProUGUI>().text = text;
        if (!buttonCancel.gameObject.activeSelf)
            buttonCancel.gameObject.SetActive(true);
    }
}
