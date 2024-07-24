using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class BallChance
{
    public int currentValue;
    public int damagePerClick;
    public float currentChance;

    public float maxChance;
    private readonly float minChance;
    public float MinChance => minChance;

    public BallChance(int currentValue, int damagePerClick, float maxChance)
    {
        this.currentValue = currentValue;
        this.damagePerClick = damagePerClick;
        this.maxChance = maxChance;
        minChance = Mathf.Clamp(maxChance / 2f, 1, 100);
        Reset();
    }

    public BallChance(int currentValue, float maxChance)
    {
        this.currentValue = currentValue;
        this.maxChance = maxChance;
        minChance = Mathf.Clamp(maxChance / 2f, 1, 100);
        Reset();
    }

    public void AddChance(float multiplier)
    {
        currentChance = Mathf.Clamp(currentChance + multiplier, minChance, maxChance);
    }

    public void Reset()
    {
        currentChance = minChance;
    }
}

public class BallonsSpawner : MonoBehaviour
{
    public const float MAX_BALLONS_SPEED = 380f;
    public const float MIN_BALLONS_SPEED = 45f;

    private readonly float ballsChanceMultiplier = 0.11f;
    private readonly float ballonsSpeedMultiplier = 2f;
    private readonly float minBallonDistanceToSpawn = 1.6f;
    private readonly float maxBombChance = 39f;
    private readonly float minBombChance = 2f;
    private readonly float bombChanceMultiplier = 0.4f;
    private readonly float maxTimeToReverse = 40f;
    private readonly float minTimeToReverse = 20f;

    [SerializeField] private GameObject ballonPrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float destroyBallonDistance = 10f;
    [SerializeField] private bool isReversed;

    private float currentBallonsSpeed;
    private float currentBallonsSpeedLerp;

    private bool canSpawn = true;
    private bool disabled = false;

    private float chanceToBomb = 2f;

    private float timeToReverse;
    private Coroutine reverseRoutine;
    private WaitForSeconds reverseWait = new(0.05f);

    private Coroutine increaseSpeedCoroutine;
    private WaitForSeconds increaseSpeedWait = new(2.5f);

    private Vector2 spawnPosition;

    private bool ballonsHided;
    private bool defaultReversed;

    public float CurrentBallonsSpeed => currentBallonsSpeed;
    public float CurrentBallonsSpeedLerp => currentBallonsSpeedLerp;
    public bool BallonsHided => ballonsHided;
    public bool IsReady { get; private set; }


    private readonly List<BallChance> ballsChances = new()
    {
        new(50, 10, 3),
        new(30, 10, 10),
        new(10, 5, 15),
    };

    private readonly List<BallChance> bombsChances = new()
    {
        new(40, 3),
        new(20, 10),
        new(10, 15),
    };

    private void Awake()
    {
        defaultReversed = isReversed;
        Restart();
    }

    public void Restart()
    {
        IsReady = false;

        spawnPosition = transform.position;

        isReversed = defaultReversed;
        currentBallonsSpeed = MIN_BALLONS_SPEED;
        currentBallonsSpeedLerp = isReversed ? -currentBallonsSpeed : currentBallonsSpeed;

        chanceToBomb = minBombChance;

        foreach (var b in ballsChances)
        {
            b.Reset();
        }

        foreach (var b in bombsChances)
        {
            b.Reset();
        }
    }

    private void Update()
    {
        if (!disabled)
        {
            var curSpeed = (Mathf.Abs(currentBallonsSpeed) + GameManager.Instance.PlayerManager.SpeedMultiplier) * (isReversed ? -1f : 1f);

            currentBallonsSpeedLerp = Mathf.Lerp(currentBallonsSpeedLerp, curSpeed, Time.deltaTime * Mathf.Abs(curSpeed) / 125f * 2f);

            if (transform.childCount > 0)
            {
                float maxDistance = 100f;
                bool allDestroying = true;
                for (var i = transform.childCount - 1; i >= 0; i--)
                {
                    if (transform.GetChild(i).TryGetComponent<MovingObject>(out var child))
                    {
                        if (!child.Destroying)
                        {
                            allDestroying = false;

                            child.speed = currentBallonsSpeedLerp;

                            var dist = Vector2.Distance(spawnPosition, child.transform.position);
                            if (dist < maxDistance)
                            {
                                maxDistance = dist;
                            }

                            if (!child.AnimDestorying)
                            {
                                if (dist >= destroyBallonDistance)
                                {
                                    if (IsReady && child.CanDealDamageAfterDestroy)
                                    {
                                        GameManager.Instance.PlayerManager.TakeDamage(child.DealDamage);
                                    }

                                    child.AddToPool();
                                    DecreaseBombChance();
                                }
                            }
                        }
                    }
                }

                if (maxDistance > minBallonDistanceToSpawn || allDestroying || transform.childCount < 1)
                {
                    canSpawn = true;
                }
                else
                {
                    canSpawn = false;
                }
            }
            else
                canSpawn = true;

            if (canSpawn)
            {
                CreateBallon();
                canSpawn = false;
            }
        }
    }

    private void CreateBallon()
    {
        if (!GameManager.Instance.PlayerManager.CanBallsSpawn)
            return;

        MovingObject moveObject;
        uint coins = 0;
        int points = 1;
        int damagePerClick = 1;

        float chance = Random.Range(0f, 100f);
        if (chance <= chanceToBomb)
        {
            if (GameManager.Instance.BombsPool.childCount > 0)
            {
                moveObject = GameManager.Instance.BombsPool.GetChild(0).GetComponent<MovingObject>();
            }
            else
            {
                moveObject = Instantiate(bombPrefab, spawnPosition, transform.rotation, transform).GetComponent<MovingObject>();
            }

            // Calculate bomb points
            float bChance = Random.Range(0f, 100f);
            bool chanceSelected = false;
            foreach (var b in bombsChances)
            {
                if (bChance <= b.currentChance && !chanceSelected)
                {
                    points = GameManager.Instance.PlayerManager.Score * b.currentValue / 100;
                    b.Reset();
                    chanceSelected = true;
                }
                else
                {
                    b.AddChance(ballsChanceMultiplier);
                }
            }

            if (!chanceSelected)
            {
                points = GameManager.Instance.PlayerManager.Score * 1 / 100;
            }

            damagePerClick = points;

            GameManager.Instance.PlayerManager.AddBomb();
        }
        else
        {
            if (GameManager.Instance.BallonsPool.childCount > 0)
            {
                moveObject = GameManager.Instance.BallonsPool.GetChild(0).GetComponent<MovingObject>();
            }
            else
            {
                moveObject = Instantiate(ballonPrefab, spawnPosition, transform.rotation, transform).GetComponent<MovingObject>();
            }

            var ballsSpawned = GameManager.Instance.PlayerManager.TotalBallsSpawned;
            if (ballsSpawned > GameManager.Instance.PlayerManager.BallsToCoins && ballsSpawned % GameManager.Instance.PlayerManager.BallsToCoins == 0)
            {
                coins = (uint)Random.Range(1, 4);
            }

            // Calculate ball points
            float bChance = Random.Range(0f, 100f);
            bool chanceSelected = false;
            foreach (var b in ballsChances)
            {
                if (bChance <= b.currentChance && !chanceSelected)
                {
                    points = b.currentValue;
                    damagePerClick = b.damagePerClick;
                    b.Reset();
                    chanceSelected = true;
                }
                else
                {
                    b.AddChance(ballsChanceMultiplier);
                }
            }

            points = Mathf.Clamp(points, 1, int.MaxValue);

            GameManager.Instance.PlayerManager.AddBall();
        }

        if (moveObject.transform.parent != transform)
            moveObject.transform.SetParent(transform);

        moveObject.transform.SetPositionAndRotation(spawnPosition, transform.rotation);

        if (!BallonsHided)
            moveObject.Show();
        else
            moveObject.Hide();

        if (!moveObject.gameObject.activeSelf)
            moveObject.gameObject.SetActive(true);

        moveObject.Init(currentBallonsSpeedLerp, this, coins, points, damagePerClick);
    }

    public void IncreaseBombChance()
    {
        IncreaseBombChance(1f);
    }

    public void IncreaseBombChance(float multiplier)
    {
        chanceToBomb = Mathf.Clamp(chanceToBomb + bombChanceMultiplier * multiplier, minBombChance, maxBombChance);
    }

    public void DecreaseBombChance()
    {
        DecreaseBombChance(2f);
    }

    public void DecreaseBombChance(float multiplier)
    {
        chanceToBomb = Mathf.Clamp(chanceToBomb - bombChanceMultiplier * multiplier, minBombChance, maxBombChance);
    }

    public void StartGame()
    {
        if (!BallonsHided)
        {
            currentBallonsSpeed = MIN_BALLONS_SPEED;
            currentBallonsSpeedLerp = isReversed ? -currentBallonsSpeed : currentBallonsSpeed;
            disabled = false;
            StartIncreaseSpeed();
            StartReverseTimer();
        }
        else
        {
            disabled = true;
        }
    }

    public void StartIncreaseSpeed()
    {
        StopIncreaseSpeed();
        increaseSpeedCoroutine = StartCoroutine(IncreaseSpeedIE());
    }

    public void StopIncreaseSpeed()
    {
        if (increaseSpeedCoroutine != null)
            StopCoroutine(increaseSpeedCoroutine);
    }

    public void DestroyAllBallons(bool restart = false)
    {
        disabled = true;
        Restart();

        if (transform.childCount > 0)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.TryGetComponent<MovingObject>(out var chComp))
                {
                    chComp.StartDestroying();
                }
            }
        }

        StartCoroutine(DestroyAllBallonsIE(restart));
    }

    private IEnumerator DestroyAllBallonsIE(bool restart = false)
    {
        yield return new WaitForSeconds(1.1f);

        if (!restart)
        {
            IsReady = true;
            GameManager.Instance.PlayerManager.StartGameWhenSpawnersReady();
            if (!BallonsHided)
            {
                disabled = false;
            }
        }
        else
        {
            StopAllCoroutines();
            Restart();
            disabled = false;
            StartReverseTimer();
        }
    }

    private IEnumerator IncreaseSpeedIE()
    {
        while (true)
        {
            yield return increaseSpeedWait;
            currentBallonsSpeed = Mathf.Clamp(currentBallonsSpeed + ballonsSpeedMultiplier, MIN_BALLONS_SPEED, MAX_BALLONS_SPEED);
        }
    }

    public void StartReverseTimer()
    {
        StopReverseTimer();
        reverseRoutine = StartCoroutine(ReverseIE());
    }

    public void StopReverseTimer()
    {
        if (reverseRoutine != null)
            StopCoroutine(reverseRoutine);
    }

    private IEnumerator ReverseIE()
    {
        while (true)
        {
            timeToReverse = Random.Range(minTimeToReverse, maxTimeToReverse);

            yield return new WaitForSeconds(timeToReverse);
            isReversed = !isReversed;

            spawnPosition.y = -spawnPosition.y;
            if (GameManager.Instance.PlayerManager.IsPlaying)
                currentBallonsSpeed = Mathf.Clamp(currentBallonsSpeed - ballonsSpeedMultiplier * Random.Range(2f, 5f), MIN_BALLONS_SPEED, MAX_BALLONS_SPEED);

            while (Mathf.Abs(currentBallonsSpeed - Mathf.Abs(currentBallonsSpeedLerp)) > 2f)
            {
                yield return reverseWait;
            }
        }
    }


    public void ShowBallons()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).TryGetComponent<MovingObject>(out var ballon))
            {
                ballon.Show();
            }
        }

        ballonsHided = false;
    }

    public void HideBallons()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).TryGetComponent<MovingObject>(out var ballon))
            {
                ballon.Hide();
            }
        }

        ballonsHided = true;
    }

    public void UpdateBallsSkin()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).TryGetComponent<MovingObject>(out var ballon))
            {
                ballon.UpdateSkin();
            }
        }
    }
}
