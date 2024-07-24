using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class SaveProduct
{
    public ushort index;

    public SaveProduct(ushort index)
    {
        this.index = index;
    }
}

[Serializable]
public class SaveProductBall : SaveProduct
{
    public BallType ballType;

    public SaveProductBall(ushort index, BallType ballType) : base(index)
    {
        this.ballType = ballType;
    }
}

[Serializable]
public class SaveProductCoin : SaveProduct
{
    public string lastPickupTime;

    public SaveProductCoin(ushort index, string lastPickupTime) : base(index)
    {
        this.lastPickupTime = lastPickupTime;
    }
}

[Serializable]
public class PlayerData
{
    public uint coins;
    public List<SaveProductBall> purchasedBalls;
    public List<SaveProductCoin> purchasedCoins;
    public ushort selectedBallIndex;
    public ushort selectedBombIndex;
    public SettingsData settingsData;
    public List<ScoreData> scores;

    public PlayerData()
    {
        settingsData = new();
    }

    public PlayerData(uint coins, IEnumerable<ProductBall> balls, IEnumerable<ProductBall> bombs, IEnumerable<ProductCoin> productCoins, SettingsData settingsData, List<ScoreData> scores)
    {
        this.coins = coins;
        purchasedBalls = new();
        purchasedCoins = new();

        var player = GameManager.Instance.PlayerManager;
        ushort i = 0;
        foreach (var ball in balls)
        {
            if (ball.Purchased)
                purchasedBalls.Add(new SaveProductBall(i, ball.BallType));

            if (player.SelectedBall == ball)
                selectedBallIndex = i;

            i++;
        }

        i = 0;
        foreach (var bomb in bombs)
        {
            if (bomb.Purchased)
                purchasedBalls.Add(new SaveProductBall(i, bomb.BallType));

            if (player.SelectedBomb == bomb)
                selectedBombIndex = i;

            i++;
        }

        i = 0;
        foreach (var coin in productCoins)
        {
            if (!coin.CanPickup)
            {
                purchasedCoins.Add(new SaveProductCoin(i, coin.LastPickupTime));
            }
        }

        this.settingsData = settingsData;
        this.scores = scores;
    }
}

[Serializable]
public class SettingsData
{
    public int numberOfLines = 2;
    public bool canPlayMusic = true;
    public bool batterySaving;
    public int scoresNumberOfLines = 2;
    public float musicVolume = 1f;

    public SettingsData() { }

    public SettingsData(int numberOfLines, bool canPlayMusic, bool batterySaving, int scoresNumberOfLines, float musicVolume)
    {
        this.numberOfLines = numberOfLines;
        this.canPlayMusic = canPlayMusic;
        this.batterySaving = batterySaving;
        this.scoresNumberOfLines = scoresNumberOfLines;
        this.musicVolume = musicVolume;
    }
}

[Serializable]
public class ScoreData
{
    public int score;
    public uint coins;
    public string date;
    public byte numberOfLines;

    public ScoreData(int score, uint coins, string date, byte numberOfLines)
    {
        this.score = score;
        this.coins = coins;
        this.date = date;
        this.numberOfLines = numberOfLines;
    }
}


public class SaveManager
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SaveGameExtern(string data, bool flush);

    [DllImport("__Internal")]
    private static extern void LoadGameExtern();
#endif

    private const string SAVE_NAME = "Save.sav";
    private static readonly string fullSavePath;
    private static readonly string saveDirectory;

    static SaveManager()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.OSXEditor:
                saveDirectory = Path.Combine(Application.dataPath, "Editor", "Saves");
            break;
    
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
                break;

            default:
                fullSavePath = Path.Combine(Application.dataPath, "Saves");
                break;
        }

        fullSavePath = Path.Combine(saveDirectory, SAVE_NAME);
    }

    public static void SaveData(PlayerData data, bool flush = false)
    {
        if (data == null)
            return;

        string json = JsonUtility.ToJson(data, true);

        switch (Application.platform)
        {
            case RuntimePlatform.WebGLPlayer:
                {
                    SaveGameExtern(json, flush);
                }
                break;

            default:
                {
                    if (!Directory.Exists(saveDirectory))
                        Directory.CreateDirectory(saveDirectory);

                    json = Encryptor.Encode(json);

                    using StreamWriter stream = new(fullSavePath);
                    stream.Write(json);
                }
                break;
        }
    }

    public static PlayerData LoadData()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WebGLPlayer:
                {
                    LoadGameExtern();
                    return null;
                }

            default:
                {
                    if (!Directory.Exists(saveDirectory))
                        return null;

                    if (!File.Exists(fullSavePath))
                        return null;

                    using StreamReader stream = new(fullSavePath);
                    string json = stream.ReadToEnd();
                    json = Encryptor.Decode(json);

                    return JsonUtility.FromJson<PlayerData>(json);
                }
        }
    }
}
