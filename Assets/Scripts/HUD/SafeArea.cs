using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private void Awake()
    {
        SetupArea();
    }

    public void SetupArea()
    {
        var rectTransform = GetComponent<RectTransform>();
        var safeArea = Screen.safeArea;
        var anchorMin = safeArea.position;
        var anchorMax = anchorMin + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMax = anchorMax;
        rectTransform.anchorMin = anchorMin;
    }
}
