using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI dateText;

    public void Init(int score, uint coins, string date)
    {
        scoreText.text = LocalizationManager.Instance.GetLocalizedText("Score") + ": " + score.ToString() + "\n" + 
            LocalizationManager.Instance.GetLocalizedText("Coins") + ": " + coins.ToString();
        dateText.text = date;
    }
}
