using UnityEngine;

public class TutorialScreen : MonoBehaviour
{
    [SerializeField] private RectTransform handImage;
    [SerializeField] private RectTransform clickEffectImage;
    [SerializeField] private RectTransform clickEffectParent;
    [SerializeField] private float animHandRotation;
    [SerializeField] private float animSpeed = 0.2f;
    [SerializeField] private LeanTweenType animType = LeanTweenType.linear;
    [SerializeField] private int animLoopCount = 4;

    private Quaternion defaultHandRotation;

    private float disableEffectTimer;

    private void Awake()
    {
        defaultHandRotation = handImage.localRotation;
    }

    private void OnEnable()
    {
        Animate();
    }

    private void OnDisable()
    {
        StopAnim();
    }

    private void Animate()
    {
        StopAnim();

        LeanTween.rotate(handImage, animHandRotation, animSpeed).setIgnoreTimeScale(true).setEase(animType)
            .setOnUpdate((float rot) =>
            {
                if (Mathf.Abs(rot - animHandRotation) > 6)
                {
                    if (clickEffectImage.gameObject.activeSelf)
                    {
                        clickEffectImage.SetParent(clickEffectParent);
                        clickEffectImage.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                        clickEffectImage.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (!clickEffectImage.gameObject.activeSelf)
                    {
                        clickEffectImage.SetParent(transform);
                        clickEffectImage.gameObject.SetActive(true);
                    }
                }
                
            })
            .setOnComplete(() =>
            {
                gameObject.SetActive(false);
            })
            .setLoopPingPong(animLoopCount);
    }

    private void StopAnim()
    {
        LeanTween.cancel(handImage);
        handImage.localRotation = defaultHandRotation;
        clickEffectImage.SetParent(clickEffectParent);
        clickEffectImage.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        if (clickEffectImage.gameObject.activeSelf)
            clickEffectImage.gameObject.SetActive(false);
    }
}
