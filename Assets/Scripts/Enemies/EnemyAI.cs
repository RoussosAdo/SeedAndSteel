using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Lock Times")]
    [SerializeField] private float attackLockTime = 0.6f;
    [SerializeField] private float hitLockTime = 0.25f;

    [Header("Attack Hitbox")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackPointDistance = 0.55f;
    [SerializeField] private float attackRadius = 0.7f;
    [SerializeField] private LayerMask playerLayer;

    private float attackTimer;
    private float lockTimer;
    private bool isAttacking;
    private bool isDead;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        attackTimer -= Time.fixedDeltaTime;

        UpdateAttackPointDirection();

        if (lockTimer > 0f)
        {
            lockTimer -= Time.fixedDeltaTime;
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            StopMoving();
            TryAttack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void UpdateAttackPointDirection()
    {
        if (player == null || attackPoint == null) return;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        attackPoint.localPosition = directionToPlayer * attackPointDistance;

        if (directionToPlayer.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (directionToPlayer.x < -0.01f)
            spriteRenderer.flipX = true;
    }

    private void ChasePlayer()
    {
        if (isAttacking) return;

        Vector2 direction = (player.position - transform.position).normalized;

        rb.linearVelocity = direction * moveSpeed;
        animator.SetFloat("Speed", rb.linearVelocity.sqrMagnitude);

        if (direction.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (direction.x < -0.01f)
            spriteRenderer.flipX = true;
    }

    private void TryAttack()
    {
        if (attackTimer > 0f || isAttacking) return;

        isAttacking = true;
        attackTimer = attackCooldown;
        lockTimer = attackLockTime;

        StopMoving();

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        DamagePlayer();

        Invoke(nameof(EndAttack), attackLockTime);
    }

    private void DamagePlayer()
    {
        if (attackPoint == null) return;

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            playerLayer
        );

        if (hit != null)
        {
            Debug.Log("Enemy hit player");
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    public void HitStun()
    {
        if (isDead) return;

        lockTimer = hitLockTime;
        isAttacking = false;
        CancelInvoke(nameof(EndAttack));
        StopMoving();
    }

    private void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);
    }

    public void SetDead()
    {
        isDead = true;
        isAttacking = false;
        lockTimer = 0f;

        CancelInvoke(nameof(EndAttack));
        StopMoving();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}