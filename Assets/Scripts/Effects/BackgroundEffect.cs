using UnityEngine;

public class BackgroundEffect : MonoBehaviour
{
    private Rigidbody2D rb;
    private float speed;
    private float speedOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        speedOffset = Random.Range((speed + 10) / 8f, (speed + 20) / 6f);
    }

    private void FixedUpdate()
    {
        rb.velocity = (speed + speedOffset) * Time.fixedDeltaTime * -transform.up;
    }

    public void UpdateSpeed(in float speed)
    {
        this.speed = speed;
    }

    public void AddToPool(Transform pool)
    {
        gameObject.SetActive(false);
        transform.SetParent(pool);
    }
}
