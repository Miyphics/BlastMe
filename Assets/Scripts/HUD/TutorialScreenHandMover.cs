using UnityEngine;

public class TutorialScreenHandMover : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        MoveToBallonUpdate();
    }

    private void MoveToBallonUpdate()
    {
        Vector3 worldPosition = GameManager.Instance.PlayerManager.NumberOfLines switch
        {
            2 => GameManager.Instance.PlayerManager.BallonsSpawners[0].transform.GetChild(0).position,
            _ => GameManager.Instance.PlayerManager.BallonsSpawners[1].transform.GetChild(0).position,
        };

        RectTransform canvas = GameManager.Instance.HudManager.MainCanvas;
        Vector3 viewportPos = GameManager.Instance.PlayerManager.MainCamera.WorldToViewportPoint(worldPosition);
        Vector2 screenPos = new(viewportPos.x * canvas.sizeDelta.x, viewportPos.y * canvas.sizeDelta.y);

        rectTransform.anchoredPosition = screenPos;
    }
}
