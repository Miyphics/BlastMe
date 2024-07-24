using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Yandex : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void GetPlayerData();

    [DllImport("__Internal")]
    private static extern void RateGame();

    private string playerName;
    private bool gameRated;

    public static Yandex Instance { get; private set; }
    [SerializeField] private YandexAds yandexAds;
    public YandexAds YandexAds => yandexAds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GetData()
    {
        GetPlayerData();
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void SetPlayerPhoto(string url)
    {

    }

    public void RateGameClicked()
    {
        if (!gameRated)
            RateGame();
    }

    public void GameRated(bool rated)
    {
        gameRated = rated;
    }
}
