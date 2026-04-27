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

    [Header("Perfect Dodge Slow Motion")]
    [SerializeField] private float slowMotionScale = 0.2f;
    [SerializeField] private float slowMotionDuration = 0.25f;

    private int currentHealth;
    private bool isDead;
    private bool isInvincible;

    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private Coroutine invincibleRoutine;

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
        if (isDead) return;

        if (playerController != null && playerController.IsDashing && playerController.IsInPerfectDodge())
        {
            Debug.Log("PERFECT DODGE!");
            StartCoroutine(SlowMotionBurst());
            return;
        }

        if (isInvincible) return;

        if (playerController != null && playerController.CanParry && attacker != null)
        {
            Debug.Log("PARRY!");
            attacker.ParryStun(parryStunTime);
            return;
        }

        if (playerController != null && playerController.IsGuarding)
        {
            Debug.Log("BLOCK!");
            SetInvincible(invincibleDuration);
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        SetInvincible(invincibleDuration);
    }

    public void SetInvincible(float duration)
    {
        if (isDead) return;

        if (invincibleRoutine != null)
            StopCoroutine(invincibleRoutine);

        invincibleRoutine = StartCoroutine(InvincibilityFrames(duration));
    }

    private IEnumerator InvincibilityFrames(float duration)
    {
        isInvincible = true;

        float timer = 0f;

        while (timer < duration)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval * 2f;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
        invincibleRoutine = null;
    }

    private IEnumerator SlowMotionBurst()
    {
        Time.timeScale = slowMotionScale;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;
    }

    private void UpdateHealthUI()
    {
        if (healthUI != null)
            healthUI.UpdateHealth(currentHealth, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died");
    }
}