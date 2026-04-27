using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private HealthUI healthUI;

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

    private void Start()
    {
        UpdateHealthUI();
    }

    public void TakeDamage(int damage, EnemyAI attacker = null)
    {
        if (isDead || isInvincible) return;

        // Parry
        if (playerController != null && playerController.CanParry && attacker != null)
        {
            Debug.Log("PARRY!");
            attacker.ParryStun(parryStunTime);
            return;
        }

        // Block
        if (playerController != null && playerController.IsGuarding)
        {
            Debug.Log("BLOCK!");
            StartCoroutine(InvincibilityFrames());
            return;
        }

        // Normal damage
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityFrames());
    }

    private void UpdateHealthUI()
    {
        if (healthUI != null)
            healthUI.UpdateHealth(currentHealth, maxHealth);
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