using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float destroyDelay = 1.2f;

    private int currentHealth;
    private Animator animator;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    private EnemyAI enemyAI;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        enemyAI = GetComponent<EnemyAI>();
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

        animator.SetTrigger("Hit");
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

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hit");
        animator.SetFloat("Speed", 0f);
        animator.SetBool("Dead", true);

        Destroy(gameObject, destroyDelay);
    }
}