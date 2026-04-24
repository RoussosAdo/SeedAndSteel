using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float destroyDelay = 1.2f;

    private int currentHealth;
    private Animator animator;
    private Collider2D enemyCollider;
    private Rigidbody2D rb;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
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

        animator.SetTrigger("Hit");
    }

    private void Die()
    {
        isDead = true;

        animator.SetBool("Dead", true);

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, destroyDelay);
    }
}