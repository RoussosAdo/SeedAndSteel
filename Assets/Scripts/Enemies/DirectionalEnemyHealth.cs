using UnityEngine;

public class DirectionalEnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float destroyDelay = 0.15f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackDuration = 0.12f;

    private int currentHealth;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    private DirectionalEnemyAI enemyAI;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<DirectionalEnemyAI>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (enemyAI != null)
            enemyAI.HitStun();

        ApplyKnockback();
    }

    private void ApplyKnockback()
    {
        if (enemyAI == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector2 direction = (transform.position - player.transform.position).normalized;

        enemyAI.ApplyKnockback(direction, knockbackForce, knockbackDuration);
    }

    private void Die()
    {
        isDead = true;

        if (enemyAI != null)
            enemyAI.SetDead();

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, destroyDelay);
    }
}