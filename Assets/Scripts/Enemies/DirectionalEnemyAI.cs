using UnityEngine;

public class DirectionalEnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.6f;
    [SerializeField] private float detectionRange = 7f;

    [Header("Swipe Attack")]
    [SerializeField] private float swipeRange = 1.8f;
    [SerializeField] private float swipeCooldown = 1.4f;
    [SerializeField] private float swipeLockTime = 0.8f;

    [Header("Stomp Attack")]
    [SerializeField] private float stompRange = 2.6f;
    [SerializeField] private float stompCooldown = 4f;
    [SerializeField] private float stompLockTime = 1.1f;

    [Header("Hit / Stun")]
    [SerializeField] private float hitLockTime = 0.25f;

    [Header("Attack Hitbox")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackPointDistance = 1f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D rb;
    private Animator animator;

    private float swipeTimer;
    private float stompTimer;
    private float lockTimer;
    private float knockbackTimer;

    private bool isAttacking;
    private bool isDead;
    private bool isKnocked;

    private Vector2 lastMoveDirection = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        UpdateAnimatorDirection(lastMoveDirection);
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        swipeTimer -= Time.fixedDeltaTime;
        stompTimer -= Time.fixedDeltaTime;

        if (isKnocked)
        {
            knockbackTimer -= Time.fixedDeltaTime;

            if (knockbackTimer <= 0f)
            {
                isKnocked = false;
                StopMoving();
            }

            return;
        }

        if (lockTimer > 0f)
        {
            lockTimer -= Time.fixedDeltaTime;
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        if (directionToPlayer != Vector2.zero)
        {
            lastMoveDirection = directionToPlayer;
            UpdateAnimatorDirection(lastMoveDirection);
            UpdateAttackPointDirection(lastMoveDirection);
        }

        if (distanceToPlayer <= stompRange && stompTimer <= 0f)
        {
            StartStompAttack();
            return;
        }

        if (distanceToPlayer <= swipeRange && swipeTimer <= 0f)
        {
            StartSwipeAttack();
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(directionToPlayer);
        }
        else
        {
            StopMoving();
        }
    }

    private void ChasePlayer(Vector2 direction)
    {
        if (isAttacking || isKnocked) return;

        rb.linearVelocity = direction * moveSpeed;

        animator.SetFloat("Speed", rb.linearVelocity.sqrMagnitude);
        UpdateAnimatorDirection(direction);
    }

    private void StartSwipeAttack()
    {
        isAttacking = true;
        swipeTimer = swipeCooldown;
        lockTimer = swipeLockTime;

        StopMoving();

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Stomp");
        animator.SetTrigger("Attack");

        Invoke(nameof(EndAttack), swipeLockTime);
    }

    private void StartStompAttack()
    {
        isAttacking = true;
        stompTimer = stompCooldown;
        lockTimer = stompLockTime;

        StopMoving();

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Stomp");
        animator.SetTrigger("Stomp");

        Invoke(nameof(EndAttack), stompLockTime);
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    private void UpdateAnimatorDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
    }

    private void UpdateAttackPointDirection(Vector2 direction)
    {
        if (attackPoint == null) return;

        attackPoint.localPosition = direction.normalized * attackPointDistance;
    }

    public void DamagePlayer()
    {
        if (attackPoint == null) return;

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            playerLayer
        );

        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(1);
        }
    }

    public void HitStun()
    {
        if (isDead) return;

        lockTimer = hitLockTime;
        isAttacking = false;

        CancelInvoke(nameof(EndAttack));

        // Μην κάνεις StopMoving εδώ, γιατί μπορεί να κόψει knockback.
    }

    public void ParryStun(float stunTime)
    {
        if (isDead) return;

        lockTimer = stunTime;
        isAttacking = false;
        isKnocked = false;

        CancelInvoke(nameof(EndAttack));
        StopMoving();

        // Δεν έχεις Hit animation ακόμα, άρα δεν κάνουμε animator.SetTrigger("Hit").
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (isDead) return;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        isKnocked = true;
        isAttacking = false;
        knockbackTimer = duration;

        CancelInvoke(nameof(EndAttack));

        rb.linearVelocity = direction.normalized * force;
    }

    public void SetDead()
    {
        isDead = true;
        isAttacking = false;
        isKnocked = false;
        lockTimer = 0f;

        CancelInvoke(nameof(EndAttack));

        StopMoving();

        // Δεν έχεις Death animation ακόμα.
        // Όταν βάλεις death anim, εδώ θα βάλουμε animator.SetBool("Dead", true).
    }

    private void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}