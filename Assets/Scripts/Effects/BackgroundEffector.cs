using UnityEngine;

public class BackgroundEffector : MonoBehaviour
{
    [SerializeField] private GameObject effectPrefab;

    private BallonsSpawner spawner;
    private Transform effectsPool;
    private Transform currentEffects;
    private byte maxEffects = 20;
    private bool canSpawn;
    private bool initialized = false;
    private float maxEffectDistance = 13f;
    private float distanceToNewEffect = 1.1f;

    private void Awake()
    {
        effectsPool = transform.Find("Pool");
        currentEffects = transform.Find("Effects");
    }

    public void Init(BallonsSpawner spawner)
    {
        this.spawner = spawner;
        initialized = true;
    }

    private void SpawnEffect()
    {
        BackgroundEffect effect;
        if (effectsPool.childCount > 0)
        {
            effect = effectsPool.GetChild(0).GetComponent<BackgroundEffect>();
        }
        else
        {
            effect = Instantiate(effectPrefab, transform.position, Quaternion.identity, currentEffects).GetComponent<BackgroundEffect>();
        }

        float scale = Random.Range(0.85f, 1.1f);
        effect.transform.localScale = new(scale, scale, scale);

        float spawnYOffset = 3;
        float xPos = Random.Range(transform.position.x - spawnYOffset, transform.position.x + spawnYOffset);
        effect.transform.SetPositionAndRotation(new(xPos, transform.position.y, transform.position.z), Quaternion.identity);
        effect.UpdateSpeed(Mathf.Abs(spawner.CurrentBallonsSpeed));
        effect.transform.SetParent(currentEffects);

        effect.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (initialized)
        {
            canSpawn = true;

            for (short i = (short)(currentEffects.childCount - 1); i >= 0; i--)
            {
                if (currentEffects.GetChild(i).TryGetComponent<BackgroundEffect>(out var child))
                {
                    child.UpdateSpeed(Mathf.Abs(spawner.CurrentBallonsSpeed));

                    var dist = Mathf.Abs(child.transform.position.y - transform.position.y);
                    if (dist <= distanceToNewEffect)
                    {
                        canSpawn = false;
                    }

                    if (dist > maxEffectDistance)
                    {
                        child.AddToPool(effectsPool);
                    }
                }
            }

            if (canSpawn)
            {
                if (currentEffects.childCount < maxEffects)
                    SpawnEffect();
            }
        }
    }
}
