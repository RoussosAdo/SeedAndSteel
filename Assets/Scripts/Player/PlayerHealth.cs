using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;

    [Header("I-Frames")]
    [SerializeField] private float invincibleDuration = 0.7f;
    [SerializeField] private float flashInterval = 0.08f;

    private int currentHealth;
    private bool isDead;
    private bool isInvincible;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityFrames());
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        float timer = 0f;

        while (timer < invincibleDuration)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval * 2f;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died");
    }
}