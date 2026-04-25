using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.25f;
    [SerializeField] private float fadeSpeed = 4f;

    private SpriteRenderer spriteRenderer;
    private Color color;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = spriteRenderer.color;
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;

        color.a -= fadeSpeed * Time.deltaTime;
        spriteRenderer.color = color;

        if (lifetime <= 0f || color.a <= 0f)
            Destroy(gameObject);
    }

    public void Setup(Sprite sprite, bool flipX)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.flipX = flipX;
    }
}