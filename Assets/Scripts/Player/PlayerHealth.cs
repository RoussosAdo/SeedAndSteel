using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;

    [Header("I-Frames")]
    [SerializeField] private float invincibleDuration = 0.7f;
    [SerializeField] private float flashInterval = 0.08f;

    [Header("Block / Parry")]
    [SerializeField] private float parryStunTime = 0.8f;

    private int currentHealth;
    private bool isDead;
    private bool isInvincible;

    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damage, EnemyAI attacker = null)
    {
        if (isDead || isInvincible) return;

        if (playerController != null && playerController.CanParry && attacker != null)
        {
            Debug.Log("PARRY!");
            attacker.ParryStun(parryStunTime);
            return;
        }

        if (playerController != null && playerController.IsGuarding)
        {
            Debug.Log("BLOCK!");
            StartCoroutine(InvincibilityFrames());
            return;
        }

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