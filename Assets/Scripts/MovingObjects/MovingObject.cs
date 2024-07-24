using System.Collections;
using TMPro;
using UnityEngine;
using suint = SafeUint;

public class MovingObject : MonoBehaviour
{
    [SerializeField] protected GameObject destroyParticles;
    [SerializeField] protected GameObject clickParticles;

    [HideInInspector] public float speed;
    protected int maxPoints;
    protected int points;
    protected int damagePerClick;

    protected BallonsSpawner mySpawner;
    protected RectTransform myScoreText;

    protected Rigidbody2D rb;

    protected float defaultScale;
    protected float animSpeed = 0.1f;
    protected bool destroying;
    public bool Destroying => destroying;
    protected bool animDestroying;
    public bool AnimDestorying => animDestroying;

    // Responsible for the distance after overcoming which damage will be inflicted if the ball flew off the screen
    protected bool canDealDamageAfterDestroy;
    public bool CanDealDamageAfterDestroy => canDealDamageAfterDestroy;
    protected virtual float DistanceToDealDamage => 3;
    protected int dealDamage = 1;
    public virtual int DealDamage => dealDamage;

    protected suint coins;
    public uint Coins => coins.Value;

    private SpriteRenderer spriteRenderer;
    public virtual Sprite Skin { get; protected set; }
    public virtual Color Color { get; protected set; }
    public virtual Color FirstParticlesColor { get; protected set; }
    public virtual Color SecondParticlesColor { get; protected set; }

    private GameObject coinObject;

    private bool hidden;

    protected virtual Transform MyPoolObject => GameManager.Instance.BallonsPool;

    protected AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultScale = transform.localScale.x;
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        Hide();

        var coinT = transform.Find("Coin");
        if (coinT != null)
            coinObject = coinT.gameObject;
    }

    public void Init(float speed, BallonsSpawner ballonsSpawner, in suint coins, int points, int damagePerClick)
    {
        this.speed = speed;
        mySpawner = ballonsSpawner;
        myScoreText = GameManager.Instance.HudManager.CreateTextOnBallon();
        this.coins = coins;
        InitSound();
        UpdateSkin();

        if (coinObject != null && spriteRenderer.sprite != null)
        {
            if (coins > 0)
            {
                if (!coinObject.activeSelf)
                    coinObject.SetActive(true);

                StartCoinAnim();
            }
            else
            {
                if (coinObject.activeSelf)
                    coinObject.SetActive(false);
            }
        }

        SetPoints(points, damagePerClick);
        UpdatePointsText();
        UpdateMyTextPosition();

        if (!myScoreText.gameObject.activeSelf)
            myScoreText.gameObject.SetActive(true);
    }

    public virtual void InitSound()
    {
        GameManager.Instance.SoundsManager.SetupAudioPops(audioSource);
    }

    public virtual void SetPoints(int points, int damagePerClick)
    {
        this.points = Mathf.Clamp(points, 1, int.MaxValue);
        maxPoints = this.points;
        this.damagePerClick = damagePerClick;
    }

    protected void UpdatePointsText()
    {
        if (myScoreText != null)
        {
            myScoreText.GetComponent<TextMeshProUGUI>().text = points.ToString();
        }
    }

    private void Update()
    {
        UpdateMyTextPosition();

        if (!canDealDamageAfterDestroy)
        {
            if (Mathf.Abs(transform.position.y) <= DistanceToDealDamage)
            {
                canDealDamageAfterDestroy = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!destroying)
            rb.velocity = speed * Time.fixedDeltaTime * -transform.up;
    }

    protected void UpdateMyTextPosition()
    {
        if (myScoreText != null)
        {
            var myScreenPos = GameManager.Instance.PlayerManager.MainCamera.WorldToViewportPoint(rb.position);
            var canvasSize = GameManager.Instance.HudManager.MainCanvas.sizeDelta;
            myScoreText.anchoredPosition = new Vector2(myScreenPos.x * canvasSize.x, myScreenPos.y * canvasSize.y);
        }
    }

    public virtual void OnClicked()
    {
        if (!destroying)
        {
            GameManager.Instance.PlayerManager.AddScore(damagePerClick);
            points -= damagePerClick;
            PlayAnimClick();

            if (points <= 0)
            {
                destroying = true;

                if (coins > 0)
                    GameManager.Instance.PlayerManager.AddCoins(coins);

                return;
            }

            UpdatePointsText();
            if (spriteRenderer.sprite != null)
            {
                var part = Instantiate(clickParticles, new Vector3(transform.position.x, transform.position.y, -1f), transform.rotation, GameManager.Instance.BallonsSpawner.parent).GetComponent<ParticleSystem>().main;
                part.startColor = new ParticleSystem.MinMaxGradient(FirstParticlesColor, SecondParticlesColor);
            }
        }
    }

    public virtual void PlayPopSound()
    {
        audioSource.Play();

        if (coins > 0)
            GameManager.Instance.SoundsManager.PlayCoinPickup();
    }

    public void AddToPool(bool createParticles = false)
    {
        if (!animDestroying)
        {
            StopCoinAnim();

            animDestroying = true;

            if (createParticles)
            {
                if (spriteRenderer.sprite != null)
                {
                    var part = Instantiate(destroyParticles, new Vector3(transform.position.x, transform.position.y, -1f), transform.rotation, GameManager.Instance.BallonsSpawner.parent).GetComponent<ParticleSystem>().main;
                    part.startColor = new ParticleSystem.MinMaxGradient(FirstParticlesColor, SecondParticlesColor);
                    PlayPopSound();
                }
            }

            speed = 0;
            GameManager.Instance.HudManager.AddBallonTextToPool(myScoreText);
            myScoreText = null;

            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, Vector3.zero, animSpeed).setOnComplete(() =>
            {
                animDestroying = false;

                if (!audioSource.isPlaying)
                {
                    FinalDestroy();
                }
                else
                {
                    StartCoroutine(DestroyAfterSoundIE());
                }
            });
        }
    }

    private IEnumerator DestroyAfterSoundIE()
    {
        while (destroying)
        {
            if (!animDestroying && !audioSource.isPlaying)
            {
                FinalDestroy();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void FinalDestroy()
    {
        gameObject.SetActive(false);
        transform.localScale = new Vector3(defaultScale, defaultScale, defaultScale);
        destroying = false;
        canDealDamageAfterDestroy = false;
        transform.SetParent(MyPoolObject);
    }

    public void StartDestroying()
    {
        destroying = true;
        rb.velocity = Vector2.zero;
        speed = 0;
        StartCoroutine(DestroyingIE());
    }

    protected IEnumerator DestroyingIE()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, 0.65f));

        AddToPool(true);
    }

    protected void PlayAnimClick()
    {
        LeanTween.cancel(gameObject);

        float scale = defaultScale + 0.08f;
        LeanTween.scale(gameObject, new Vector3(scale, scale, scale), animSpeed).setOnComplete(() =>
        {
            float toScale = destroying ? 0 : defaultScale;
            LeanTween.scale(gameObject, new Vector3(defaultScale, defaultScale, defaultScale), animSpeed).setOnComplete(() =>
            {
                if (destroying)
                    AddToPool(true);
            });
        });
    }

    public void Show()
    {
        hidden = false;
        spriteRenderer.sprite = Skin;
        for (sbyte i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (coinObject != null)
            {
                if (child.gameObject == coinObject)
                {
                    if (coins <= 0)
                        continue;
                }
            }

            if (!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);
        }

        if (coinObject != null)
            if (coins > 0 && !coinObject.activeSelf)
            {
                coinObject.SetActive(true);
                StartCoinAnim();
            }
    }

    public void Hide()
    {
        hidden = true;
        spriteRenderer.sprite = null;
        for (sbyte i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
                child.gameObject.SetActive(false);
        }
    }

    public void UpdateSkin()
    {
        if (!hidden)
            spriteRenderer.sprite = Skin;

        spriteRenderer.color = Color;
        UpdateColor();
    }

    public virtual void UpdateColor()
    {
        FirstParticlesColor = Color;
        SecondParticlesColor = Helpers.AddColorValue(Color, 0.8f);
    }

    public void StartCoinAnim()
    {
        if (coinObject != null)
        {
            StopCoinAnim();

            var pos = new Vector3[]
            {
                new(0f, 1f, -1f),
                new(0f, 1f, -1f),
                new(1f, 0f, -1f),
                new(0f, -1f, -1f),
                new(-1f, 0f, -1f),
                new(0f, 1f, -1f),
                new(0f, 1f, -1f),
            };

            LeanTween.moveSplineLocal(coinObject, pos, 4f).setEase(LeanTweenType.linear).setLoopCount(-1);
        }
    }

    public void StopCoinAnim()
    {
        if (coinObject != null)
            LeanTween.cancel(coinObject);
    }
}
