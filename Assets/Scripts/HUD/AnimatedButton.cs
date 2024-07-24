using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AnimatedButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public UnityEvent onClick;

    [SerializeField] protected bool onClickEventAfterAnim = true;
    [SerializeField] protected bool canClickInConfrimWindow;

    private RectTransform buttonSpriteFront;
    private float animSpeed = 0.15f;
    private bool animPlayed = false;
    private LeanTweenType animType = LeanTweenType.easeInOutExpo;
    private GameObject objectOnButton;

    public GameObject ObjectOnButton => objectOnButton;

    private void Awake()
    {
        buttonSpriteFront = transform.GetChild(1).GetComponent<RectTransform>();
        var mainButton = transform.GetChild(1);
        if (mainButton.childCount > 0)
            objectOnButton = mainButton.GetChild(0).gameObject;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.HudManager.IsAnimated && (GameManager.Instance.HudManager.ConfirmationWindow.Hidden || canClickInConfrimWindow))
        {
            if (!onClickEventAfterAnim)
                onClick.Invoke();

            PlayAnim();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Event: " + eventData.ToString());
        //LeanTween.move(buttonSpriteFront, Vector2.zero, animSpeed);
    }

    public void ShowObjectOnButton()
    {
        if (objectOnButton != null && !objectOnButton.activeSelf)
            objectOnButton.SetActive(true);
    }

    public void HideObjectOnButton()
    {
        if (objectOnButton != null && objectOnButton.activeSelf)
            objectOnButton.SetActive(false);
    }

    public void ToggleObjectOnButton()
    {
        if (objectOnButton != null)
            objectOnButton.SetActive(!objectOnButton.activeSelf);
    }

    protected void PlayAnim(bool withoutAction = false)
    {
        if (!animPlayed)
        {
            LeanTween.move(buttonSpriteFront, Vector2.zero, animSpeed).setEase(animType).setLoopPingPong(1).setOnComplete(() =>
            {
                animPlayed = false;

                if (!withoutAction && onClickEventAfterAnim)
                    onClick.Invoke();
            }).setIgnoreTimeScale(true);

            animPlayed = true;
        }
    }
}
