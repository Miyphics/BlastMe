using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoresHud : MonoBehaviour, IAnimatedHud
{
    [SerializeField] private RectTransform verticalPanel;
    [SerializeField] private RectTransform scoresPanel;
    [SerializeField] private TextMeshProUGUI currentCategoryText;

    private Vector2 defaultVerticalPanelPos;
    private Vector2 defaultVerticalPanelSize;
    private Color defaultBackgroundColor;
    private int currentNumberOfLines = 2;
    public int CurrentNumberOfLines { get { return currentNumberOfLines; }
        set
        {
            currentNumberOfLines = value;
            if (currentCategoryText != null)
            {
                currentCategoryText.text = currentNumberOfLines.ToString() + " ";
                if (currentNumberOfLines > 1)
                {
                    currentCategoryText.text += LocalizationManager.Instance.GetLocalizedText("lines");
                }
                else
                {
                    currentCategoryText.text += LocalizationManager.Instance.GetLocalizedText("line");
                }
            }
        }
    }

    private void Awake()
    {
        defaultBackgroundColor = transform.GetChild(0).GetComponent<Image>().color;

        defaultVerticalPanelPos = verticalPanel.anchoredPosition;
        defaultVerticalPanelSize = verticalPanel.sizeDelta;
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
            var animSpeed = GameManager.Instance.HudManager.HudAnimSpeed;

            LeanTween.cancel(background);

            if (enable)
            {
                UpdateScores(currentNumberOfLines);

                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = verticalPanel, position = defaultVerticalPanelPos, size = defaultVerticalPanelSize }
                };

                LeanTween.color(background, defCol, animSpeed).setIgnoreTimeScale(true);
            }
            else
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = verticalPanel, position = new(defaultVerticalPanelPos.x, 170), size = new(defaultVerticalPanelSize.x, -620) }
                };

                LeanTween.color(background, new Color(defCol.r, defCol.g, defCol.b, 0), animSpeed).setIgnoreTimeScale(true);
            }

            GameManager.Instance.HudManager.AnimateRectTransforms(transform, anims, enable, actionAfterAnim, force, 0.7f);
        }
    }

    public void UpdateScores(int numberOfLines)
    {
        if (currentNumberOfLines != (byte)numberOfLines)
            GameManager.Instance.PlayerManager.DeferredSaveSettings();

        CurrentNumberOfLines = (byte)numberOfLines;

        for (var i = scoresPanel.childCount - 1; i >= 1; i--)
        {
            Destroy(scoresPanel.GetChild(i).gameObject);
        }

        var prefab = scoresPanel.GetChild(0).gameObject;

        var scores = numberOfLines switch
        {
            1 => GameManager.Instance.PlayerManager.OneLineScores,
            3 => GameManager.Instance.PlayerManager.ThreeLinesScores,
            _ => GameManager.Instance.PlayerManager.TwoLinesScores,
        };

        if (scores != null && scores.Count > 0)
        {
            scores = scores.ToList().OrderByDescending(x => x.score).ToList();

            foreach (var score in scores)
            {
                var obj = Instantiate(prefab, scoresPanel).GetComponent<ScoreUI>();
                obj.Init(score.score, score.coins, score.date);
                obj.gameObject.SetActive(true);
            }
        }
    }
}
