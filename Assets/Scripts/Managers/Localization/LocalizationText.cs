using UnityEngine;
using TMPro;

public class LocalizationText : MonoBehaviour
{
    [SerializeField]
    protected string key;

    protected TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDestroy()
    {
        LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    protected virtual void UpdateText()
    {
        if (gameObject == null)
            return;

        text.text = LocalizationManager.Instance.GetLocalizedText(key);
    }
}
