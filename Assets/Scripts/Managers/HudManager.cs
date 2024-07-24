using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using UnityEditor;

public class HudManager : MonoBehaviour
{
    public struct RectTransformAnimated
    {
        public RectTransform rectTransform;
        public Vector2 position;
        public Vector2 size;
    }

    [SerializeField] private RectTransform mainCanvasTransform;
    public RectTransform MainCanvas => mainCanvasTransform;

    [Space, SerializeField] private TextMeshProUGUI gameInfoText;
    [SerializeField] private TextMeshProUGUI scoreText;
    private Vector2 defaultScoreTextPos;
    private Vector2 defaultScoreTextSize;

    [Header("Menus"), SerializeField] private MenuHud menuHud;
    public MenuHud MenuHud => menuHud;
    [SerializeField] private InGameHud inGameHud;
    public InGameHud InGameHud => inGameHud;
    [SerializeField] private GameOverHud gameOverHud;
    public GameOverHud GameOverHud => gameOverHud;

    [SerializeField] private ConfirmationWindow confirmationWindow;
    public ConfirmationWindow ConfirmationWindow => confirmationWindow;

    [SerializeField] private PauseHud pauseHud;
    public PauseHud PauseHud => pauseHud;

    [Header("Ballons"), SerializeField] private GameObject ballonTexts;
    [SerializeField] private GameObject ballonTextPool;

    [Header("Music"), SerializeField] private RectTransform currentMusicMainPanel;
    [SerializeField] private RectTransform currentMusicIcon;
    [SerializeField] private RectTransform currentMusicTextPanel;

    private Coroutine currentMusicRoutine;

    public bool scoreTextShowed;

    public LeanTweenType EaseAnimType => LeanTweenType.easeInOutBack;
    public float HudAnimSpeed => 0.5f;

    private bool isAnimated;
    public bool IsAnimated => isAnimated;

    private void Awake()
    {
        scoreText.text = "";

        defaultScoreTextPos = scoreText.rectTransform.anchoredPosition;
        defaultScoreTextSize = scoreText.rectTransform.sizeDelta;

        StartCoroutine(ShowGameInfoIE());

        HideScoreText();
    }

    private void Start()
    {
        //InputManager.Instance.OnStartTouch += ConsoleOnStartTouch;
        //consoleCoroutine = StartCoroutine(ConsoleClearIE());
    }

    public void StartGame()
    {
        HideMenuHud();
        SetScoreText(0);
        ShowScoreText();

        GameManager.Instance.PlayerManager.StartGame();
    }

    public void HideMenuHud()
    {
        menuHud.ToggleAnim(false);
    }

    public void ShowGameHud()
    {
        inGameHud.ToggleAnim(true);
    }

    public void HideGameHud()
    {
        inGameHud.ToggleAnim(false);
    }

    public void ShowMainMenu(Action actionAfterAnim = null)
    {
        void showMenu(Action action)
        {
            menuHud.ToggleAnim(true, action, true);
        }

        if (!inGameHud.Hidden)
        {
            inGameHud.ToggleAnim(false, () => showMenu(actionAfterAnim));
        }
        else
        {
            showMenu(actionAfterAnim);
        }
    }

    public void ShowGameOver()
    {
        gameOverHud.ToggleAnim(true, true);
    }

    public void SetScoreText(int score)
    {
        scoreText.text = LocalizationManager.Instance.GetLocalizedText("Score") + ": " + score;
    }

    public RectTransform CreateTextOnBallon()
    {
        Transform child;

        if (ballonTextPool.transform.childCount > 1)
        {
            child = ballonTextPool.transform.GetChild(1);
            child.SetParent(ballonTexts.transform);
        }
        else
        {
            child = Instantiate(ballonTextPool.transform.GetChild(0), ballonTexts.transform);
        }

        return child.GetComponent<RectTransform>();
    }

    public void AddBallonTextToPool(Transform ballonText)
    {
        if (ballonText != null)
        {
            ballonText.gameObject.SetActive(false);
            ballonText.SetParent(ballonTextPool.transform);
        }
    }

    private IEnumerator ShowGameInfoIE()
    {
        while(true)
        {
            gameInfoText.text = "fps: " + Mathf.RoundToInt(1.0f / Time.deltaTime);

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void ShowCurrentMusicName(string musicName)
    {
        /*
        if (!currentMusicMainPanel.gameObject.activeSelf)
            currentMusicMainPanel.gameObject.SetActive(true);

        var animSpeed = 0.5f;

        LeanTween.cancel(currentMusicMainPanel);
        LeanTween.cancel(currentMusicIcon);
        LeanTween.cancel(currentMusicTextPanel);

        if (currentMusicRoutine != null)
            StopCoroutine(currentMusicRoutine);

        LeanTween.move(currentMusicMainPanel, new Vector3(currentMusicMainPanel.anchoredPosition.x, -100), animSpeed).setEase(EaseAnimType).setOnComplete(() =>
        {
            currentMusicTextPanel.gameObject.SetActive(true);

            var textPanelWidth = musicName.Length * 6f + 150;
            LeanTween.size(currentMusicTextPanel, new Vector2(textPanelWidth, currentMusicTextPanel.sizeDelta.y), animSpeed).setEase(EaseAnimType).setOnComplete(() =>
            {
                currentMusicRoutine = StartCoroutine(HideCurrentMusicNameIE(musicName));
            }).setIgnoreTimeScale(true);

            LeanTween.move(currentMusicIcon, new Vector2(textPanelWidth / 2f, currentMusicIcon.anchoredPosition.y), animSpeed).setEase(EaseAnimType).setIgnoreTimeScale(true);

        }).setIgnoreTimeScale(true);*/
    }

    private IEnumerator HideCurrentMusicNameIE(string currentMusicName)
    {
        StringBuilder musicName = new();
        float textUpdateSpeed = 0.007f;

        var textComp = currentMusicTextPanel.GetChild(0).GetComponent<TextMeshProUGUI>();
        textComp.text = "";
        textComp.gameObject.SetActive(true);

        foreach (var ch in currentMusicName)
        {
            if (currentMusicName.Length > 0)
            {
                musicName.Append(ch);

                textComp.text = musicName.ToString();

                yield return new WaitForSeconds(textUpdateSpeed);
            }
            else
                break;
        }


        yield return new WaitForSeconds(3f);


        for (var i = textComp.text.Length - 1; i >= 0; i--)
        {
            if (musicName.Length > 0)
            {
                musicName.Remove(i, 1);

                textComp.text = musicName.ToString();

                yield return new WaitForSeconds(textUpdateSpeed);
            }
            else
                break;
        }

        textComp.gameObject.SetActive(false);

        LeanTween.size(currentMusicTextPanel, new Vector2(0, currentMusicTextPanel.sizeDelta.y), 0.5f).setEase(EaseAnimType).setOnComplete(() =>
        {
            LeanTween.move(currentMusicMainPanel, new Vector2(currentMusicMainPanel.anchoredPosition.x, 100), 0.5f).setEase(EaseAnimType).setOnComplete(() =>
            {
                if (currentMusicMainPanel.gameObject.activeSelf)
                    currentMusicMainPanel.gameObject.SetActive(false);
            }).setIgnoreTimeScale(true);

        }).setIgnoreTimeScale(true);

        LeanTween.move(currentMusicIcon, new Vector2(0, currentMusicIcon.anchoredPosition.y), 0.5f).setEase(EaseAnimType).setIgnoreTimeScale(true);
    }


    public static void ToggleAllChilds(Transform transform, bool enable)
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (child.activeSelf != enable)
                child.SetActive(enable);
        }
    }

    public void AnimateRectTransforms(Transform parentTransform, List<RectTransformAnimated> transforms, bool enable, Action actionAfterAnim = null, bool force = false, float animSpeed = 0, LeanTweenType animType = LeanTweenType.notUsed)
    {
        if (!isAnimated || force)
        {
            foreach (var transf in transforms)
            {
                LeanTween.cancel(transf.rectTransform);
            }

            if (animSpeed <= 0)
                animSpeed = HudAnimSpeed;

            if (animType == LeanTweenType.notUsed)
                animType = EaseAnimType;

            isAnimated = true;
            void finalAction()
            {
                //Debug.Log("This is finall action! | Enable = " + enable + " | " + (actionAfterAnim == null ? "Action : NULL" : "Action EXISTS"));

                if (!enable)
                    ToggleAllChilds(parentTransform, enable);

                isAnimated = false;

                actionAfterAnim?.Invoke();
            }

            bool actionCreated = false;
            foreach (var transf in transforms)
            {
                if (enable)
                    ToggleAllChilds(parentTransform, enable);

                if (transf.rectTransform.anchoredPosition != transf.position)
                {
                    var moveAnim = LeanTween.move(transf.rectTransform, transf.position, animSpeed).setEase(animType).setIgnoreTimeScale(true);

                    if (!actionCreated)
                    {
                        moveAnim.setOnComplete(() => finalAction());
                        actionCreated = true;
                    }
                }
                
                if (transf.rectTransform.sizeDelta != transf.size)
                {
                    var sizeAnim = LeanTween.size(transf.rectTransform, transf.size, animSpeed).setEase(animType).setIgnoreTimeScale(true);

                    if (!actionCreated)
                    {
                        sizeAnim.setOnComplete(() => finalAction());
                        actionCreated = true;
                    }
                }
            }
        }
    }

    public void ShowScoreText()
    {
        LeanTween.cancel(scoreText.rectTransform);
        LeanTween.move(scoreText.rectTransform, defaultScoreTextPos, HudAnimSpeed).setEase(EaseAnimType).setIgnoreTimeScale(true);
        LeanTween.size(scoreText.rectTransform, defaultScoreTextSize, HudAnimSpeed).setEase(EaseAnimType).setIgnoreTimeScale(true);
    }

    public void HideScoreText()
    {
        LeanTween.cancel(scoreText.rectTransform);
        LeanTween.move(scoreText.rectTransform, new(defaultScoreTextPos.x, 20), HudAnimSpeed).setEase(EaseAnimType).setIgnoreTimeScale(true);
        LeanTween.size(scoreText.rectTransform, new(defaultScoreTextSize.x, 0), HudAnimSpeed).setEase(EaseAnimType).setIgnoreTimeScale(true);
    }
}
