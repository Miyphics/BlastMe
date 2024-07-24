using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using EZCameraShake;
using System.Runtime.InteropServices;
using sshort = SafeShort;
using sint = SafeInt;
using suint = SafeUint;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SetToLeaderboard(int countOfLines, int score);


    [SerializeField] private Camera mainCamera;
    public Camera MainCamera => mainCamera;

    [SerializeField] private List<BallonsSpawner> ballonSpawners;
    public IReadOnlyList<BallonsSpawner> BallonsSpawners => ballonSpawners;
    [SerializeField] private BackgroundEffector backgroundEffector;
    private int numberOfLines = 2;
    public int NumberOfLines => numberOfLines;
    public bool CanBallsSpawn { get; private set; }

    private sint score;
    public int Score => score.Value;

    private suint coins;
    public uint Coins => coins.Value;

    private suint currentGameCoins;
    public uint CurrentGameCoins => currentGameCoins.Value;

    public sshort ballsToCoins;
    public short BallsToCoins => ballsToCoins;

    private uint totalBallsSpawned;
    public uint TotalBallsSpawned => totalBallsSpawned;
    private uint totalBombsSpawned;
    public uint TotalBombsSpawned => totalBombsSpawned;
    public uint TotalObjectsSpawned => totalBallsSpawned + totalBombsSpawned;

    public ProductBall SelectedBall { get; private set; }
    public ProductBall SelectedBomb { get; private set; }

    private sshort health;
    public short Health => health.Value;

    public bool IsPlaying { get; private set; }
    public bool IsPaused { get; private set; }

    private LayerMask moveObjectsMask;

    private float speedMultiplier;
    public float SpeedMultiplier => speedMultiplier;
    private Coroutine changeSpeedRoutine;

    private Coroutine deferredSavingRoutine;

    public bool IsLoading { get; private set; } = true;

    private const byte MAX_SCORES = 10;

    private readonly List<ScoreData> oneLineScores = new();
    public IReadOnlyList<ScoreData> OneLineScores => oneLineScores;

    private readonly List<ScoreData> twoLinesScores = new();
    public IReadOnlyList<ScoreData> TwoLinesScores => twoLinesScores;

    private readonly List<ScoreData> threeLinesScores = new();
    public IReadOnlyList<ScoreData> ThreeLinesScores => threeLinesScores;

    private AudioSource audioSource;

    private float cursorSize = 0.25f;

    public bool CanShowAds { get; private set; }

    private void Awake()
    {
        ballsToCoins = 20;

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        moveObjectsMask = LayerMask.GetMask("MoveObjects");

        SelectedBall = GameManager.Instance.ProductManager.GetProductBalls(true)[0];
        SelectedBomb = GameManager.Instance.ProductManager.GetProductBombs(true)[0];

        StartCoroutine(LoadSavesIE());

        InputManager.Instance.OnStartTouch += OnTouchPress;
    }

    public bool LoadSaveJson(string json)
    {
        if (json != null && json.Length > 2)
        {
            try
            {
                return LoadSave(JsonUtility.FromJson<PlayerData>(json));
            }
            catch { }
        }

        return LoadSaveNull();
    }

    public bool LoadSaveNull()
    {
        return LoadSave(new PlayerData());
    }

    private bool LoadSave(PlayerData data)
    {
        if (data == null)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SetNumberOfLinesNoSave(2);
                FinalGameLoad();
            }

            return false;
        }

        coins = data.coins;

        List<SaveProduct> products = new();

        if (data.purchasedBalls != null)
            products.AddRange(data.purchasedBalls);

        if (data.purchasedCoins != null)
            products.AddRange(data.purchasedCoins);
            
        if (products.Count > 0)
            GameManager.Instance.ProductManager.LoadData(products);

        if (data.scores != null && data.scores.Count > 0)
        {
            foreach (var score in data.scores)
            {
                switch (score.numberOfLines)
                {
                    case 1:
                        oneLineScores.Add(score);
                        break;

                    case 2:
                        twoLinesScores.Add(score);
                        break;

                    case 3:
                        threeLinesScores.Add(score);
                        break;
                }
            }
        }

        SelectedBall = GameManager.Instance.ProductManager.ProductBalls[data.selectedBallIndex];
        SelectedBomb = GameManager.Instance.ProductManager.ProductBombs[data.selectedBombIndex];

        GameManager.Instance.HudManager.MenuHud.SettingsPanel.ToggleMusic(data.settingsData.canPlayMusic);
        GameManager.Instance.HudManager.MenuHud.SettingsPanel.SetMusicVolumeSliderValue(data.settingsData.musicVolume);
        GameManager.Instance.HudManager.MenuHud.SettingsPanel.ToggleBatterySaving(data.settingsData.batterySaving);
        SetNumberOfLinesNoSave(data.settingsData.numberOfLines);
        GameManager.Instance.HudManager.MenuHud.ScoresHud.CurrentNumberOfLines = data.settingsData.scoresNumberOfLines;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            FinalGameLoad();

        return true;
    }

    private void FinalGameLoad()
    {
        GameManager.Instance.HudManager.MenuHud.UpdateCountOfLines();
        GameManager.Instance.MusicManager.StartPlayingMusic();

        IsLoading = false;

        RestartGame();

        foreach (var spawner in ballonSpawners)
        {
            spawner.StartReverseTimer();
        }
    }

    public IEnumerator LoadSavesIE()
    {
        yield return new WaitForSeconds(0.1f);

        /*
        InputManager.Instance.OnPauseCancelled += () =>
        {
            if (IsPaused)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;

            IsPaused = !IsPaused;
        };*/

        InputManager.Instance.OnPauseCancelled += () => TogglePause(!IsPaused);

        var playerData = SaveManager.LoadData();

        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            if (!LoadSave(playerData))
            {
                SetNumberOfLinesNoSave(2);
            }

            FinalGameLoad();
        }
    }

    public void RestartGame()
    {
        CanBallsSpawn = true;

        foreach (var spawner in ballonSpawners)
        {
            if (spawner.gameObject.activeSelf)
            {
                spawner.Restart();
            }
        }

        GameManager.Instance.HudManager.SetScoreText(score.Value);
        health = 5;
        totalBallsSpawned = 0;
        totalBombsSpawned = 0;
        currentGameCoins = 0;

        GameManager.Instance.HudManager.InGameHud.UpdateHealth(true);

        StartChangingSpeed();
    }

    public void StartGame()
    {
        score = 0;
        health = 5;
        totalBallsSpawned = 0;
        totalBombsSpawned = 0;

        DestroyAllBallons();
    }

    private void DestroyAllBallons()
    {
        StopChangingSpeed();

        foreach (var spawner in ballonSpawners)
        {
            if (spawner.gameObject.activeSelf)
            {
                spawner.DestroyAllBallons(health <= 0);
            }
        }
    }

    public void StartGameWhenSpawnersReady()
    {
        if (!IsPlaying)
        {
            foreach (var spawner in ballonSpawners)
            {
                if (spawner.gameObject.activeSelf && !spawner.IsReady)
                {
                    return;
                }
            }

            IsPlaying = true;
            GameManager.Instance.HudManager.ShowGameHud();
            

            foreach (var spawner in ballonSpawners)
            {
                if (spawner.gameObject.activeSelf)
                {
                    spawner.StartGame();
                }
            }

            StartChangingSpeed();
        }
    }

    private void OnTouchPress(Vector2 position, float time)
    {
        if (IsPlaying && !IsPaused)
        {
            Vector3 mouseOnScreenPos = new(position.x, position.y, Camera.main.nearClipPlane);
            //Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseOnScreenPos);
            //worldPos.z = 0;

            RaycastHit2D hit = Physics2D.CircleCast(Camera.main.ScreenToWorldPoint(mouseOnScreenPos), cursorSize, Vector2.zero, 0f, moveObjectsMask);
            if (hit.collider != null)
            {
                MovingObject touchedObj = hit.collider.GetComponent<MovingObject>();
                touchedObj.OnClicked();
            }
        }
    }

    public void AddScore(int score)
    {
        this.score += score;
        GameManager.Instance.HudManager.SetScoreText(Score);
    }

    public void AddCoins(uint coins)
    {
        currentGameCoins += coins;
    }

    public void AddGlobalCoins(uint coins)
    {
        this.coins += coins;
        GameManager.Instance.HudManager.MenuHud.ShopPanel.UpdateCoins();
    }

    public void AddBall()
    {
        totalBallsSpawned++;
    }

    public void AddBomb()
    {
        totalBombsSpawned++;
    }

    public void SetNumberOfLines(int lines)
    {
        SetNumberOfLinesNoSave(lines);
        DeferredSaveSettings();
    }

    public void DeferredSaveSettings()
    {
        if (deferredSavingRoutine != null)
            StopCoroutine(deferredSavingRoutine);

        deferredSavingRoutine = StartCoroutine(DeferredSavingIE());
    }

    public void SetNumberOfLinesNoSave(int lines)
    {
        numberOfLines = (byte)lines;

        switch (numberOfLines)
        {
            case 1:
                if (!ballonSpawners[0].BallonsHided)
                    ballonSpawners[0].HideBallons();
                if (!ballonSpawners[2].BallonsHided)
                    ballonSpawners[2].HideBallons();

                if (ballonSpawners[1].BallonsHided)
                    ballonSpawners[1].ShowBallons();

                backgroundEffector.Init(ballonSpawners[1]);
                break;

            case 3:
                foreach (var spawner in ballonSpawners)
                {
                    if (spawner.BallonsHided)
                        spawner.ShowBallons();
                }

                backgroundEffector.Init(ballonSpawners[0]);
                break;

            case 2:
            default:
                if (ballonSpawners[0].BallonsHided)
                    ballonSpawners[0].ShowBallons();
                if (ballonSpawners[2].BallonsHided)
                    ballonSpawners[2].ShowBallons();

                if (!ballonSpawners[1].BallonsHided)
                    ballonSpawners[1].HideBallons();

                backgroundEffector.Init(ballonSpawners[0]);
                break;
        }
    }

    public void TakeDamage(int damage, bool saveGame = true)
    {
        //Debug.Log("Health: " + health + " | Damage: " + damage + " | Health - Damage: " + (health - damage));
        if (damage <= 0)
            return;

        health = Mathf.Clamp(health - damage, 0, 100);

        // This method show and hide menus
        GameManager.Instance.HudManager.InGameHud.UpdateHealth();

        CameraShaker.Instance.ShakeOnce(4f, 4f, 0.1f, 0.1f);
        GameManager.Instance.SoundsManager.SetupAudioBomb(audioSource);
        audioSource.Play();

        // Game over
        if (health <= 0)
        {
            if (saveGame)
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    if (score > 0)
                    {
                        if (GameManager.Instance.IsAuth)
                            SetToLeaderboard(numberOfLines, Score);

                        if (totalBallsSpawned > ballsToCoins)
                        {
                            CanShowAds = true;
                        }
                    }
                }

                AddNewScore();

                coins += currentGameCoins;
                GameManager.Instance.HudManager.scoreTextShowed = true;
                SaveData();
            }

            currentGameCoins = 0;

            DestroyAllBallons();
            IsPlaying = false;
            CanBallsSpawn = false;
        }
    }

    public void ShowAds()
    {
        CanShowAds = false;
        Yandex.Instance.YandexAds.ShowAds(1);
    }

    private void AddNewScore()
    {
        if (score > 0)
        {
            ScoreData newScore = new(score, currentGameCoins, DateTime.Now.ToString("HH:mm\ndd.MM.yyyy"), numberOfLines);

            switch (numberOfLines)
            {
                case 1:
                    if (oneLineScores.Count < MAX_SCORES)
                    {
                        oneLineScores.Add(newScore);
                    }
                    else
                    {
                        var index = oneLineScores.FindIndex(x => x.score < score);
                        if (index >= 0)
                        {
                            oneLineScores[index] = newScore;
                        }
                    }
                    break;

                case 2:
                    if (twoLinesScores.Count < MAX_SCORES)
                    {
                        twoLinesScores.Add(newScore);
                    }
                    else
                    {
                        var index = twoLinesScores.FindIndex(x => x.score < score);
                        if (index >= 0)
                        {
                            twoLinesScores[index] = newScore;
                        }
                    }
                    break;

                case 3:
                    if (threeLinesScores.Count < MAX_SCORES)
                    {
                        threeLinesScores.Add(newScore);
                    }
                    else
                    {
                        var index = threeLinesScores.FindIndex(x => x.score < score);
                        if (index >= 0)
                        {
                            threeLinesScores[index] = newScore;
                        }
                    }
                    break;
            }
        }
    }

    public void SaveData()
    {
        List<ScoreData> scores = new();
        scores.AddRange(oneLineScores);
        scores.AddRange(twoLinesScores);
        scores.AddRange(threeLinesScores);

        SettingsData settings = new(numberOfLines, GameManager.Instance.MusicManager.CanPlayMusic, GameManager.Instance.BatterySaving,
            GameManager.Instance.HudManager.MenuHud.ScoresHud.CurrentNumberOfLines, GameManager.Instance.MusicManager.GetVolume());

        PlayerData data = new(Coins, GameManager.Instance.ProductManager.ProductBalls, GameManager.Instance.ProductManager.ProductBombs,
            GameManager.Instance.ProductManager.ProductCoins, settings, scores);

        SaveManager.SaveData(data);
    }

    private void StartChangingSpeed()
    {
        StopChangingSpeed();
        changeSpeedRoutine = StartCoroutine(ChangeSpeedIE());
    }

    private void StopChangingSpeed()
    {
        if (changeSpeedRoutine != null)
            StopCoroutine(ChangeSpeedIE());

        speedMultiplier = 0;
    }

    private IEnumerator ChangeSpeedIE()
    {
        while (true)
        {
            var timeToChangeSpeed = UnityEngine.Random.Range(15f, 35f);
            yield return new WaitForSeconds(timeToChangeSpeed);

            foreach (var spawner in ballonSpawners)
            {
                if (!spawner.BallonsHided)
                {
                    float speedPercent = spawner.CurrentBallonsSpeed / BallonsSpawner.MAX_BALLONS_SPEED - BallonsSpawner.MIN_BALLONS_SPEED / BallonsSpawner.MAX_BALLONS_SPEED;

                    if (speedPercent < 0.5f)
                        speedMultiplier = Mathf.Clamp((Mathf.Abs(spawner.CurrentBallonsSpeed) + 10) / 4f, 0, 100);
                    else
                        speedMultiplier = Mathf.Clamp(-((Mathf.Abs(spawner.CurrentBallonsSpeed) + 10) / 3f), -100, 0);
                    break;
                }
            }

            timeToChangeSpeed = UnityEngine.Random.Range(9f, 15f);
            yield return new WaitForSeconds(timeToChangeSpeed);

            speedMultiplier = 0;
        }
    }

    public bool BuyBall(ProductBall productBall)
    {
        if (productBall == null)
            return false;

        if (coins >= productBall.Price)
        {
            productBall.Buy();
            coins -= productBall.Price;
            ChangeBallSkin(productBall);
            return true;
        }

        return false;
    }

    public void ChangeBallSkin(ProductBall productBall)
    {
        if (productBall == null)
            return;

        switch (productBall.BallType)
        {
            case BallType.Bomb:
                SelectedBomb = productBall;
                break;

            case BallType.Ball:
            default:
                SelectedBall = productBall;
                break;
        }

        foreach (var spawner in ballonSpawners)
        {
            spawner.UpdateBallsSkin();
        }

        SaveData();
    }

    public IEnumerator DeferredSavingIE()
    {
        yield return new WaitForSeconds(1.5f);

        SaveData();
    }

    public void TogglePause(bool enable, bool force)
    {
        if (!force)
            if (IsPaused == enable || GameManager.Instance.HudManager.IsAnimated)
                return;

        if (IsPlaying)
        {
            if (enable)
            {
                GameManager.Instance.HudManager.MenuHud.SettingsPanel.SetCloseAction(() => TogglePause(false));
                GameManager.Instance.HudManager.InGameHud.ToggleAnim(false, () => GameManager.Instance.HudManager.MenuHud.SettingsPanel.ToggleAnim(true), true);

                Time.timeScale = 0;
            }
            else
            {
                GameManager.Instance.HudManager.MenuHud.SettingsPanel.ToggleAnim(false, () => GameManager.Instance.HudManager.InGameHud.ToggleAnim(true, () =>
                {
                    Time.timeScale = 1;

                    if (force)
                    {
                        GameManager.Instance.PlayerManager.TakeDamage(1000, false);
                    }

                }, true), true);
            }

            IsPaused = enable;
        }
    }

    public void TogglePause(bool enable)
    {
        TogglePause(enable, false);
    }

    private void OnDrawGizmos()
    {
        var mousePos = Mouse.current.position.ReadValue();
        Vector2 mouseInWorld = Camera.main.ScreenToWorldPoint(mousePos);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mouseInWorld, cursorSize);
    }
}
