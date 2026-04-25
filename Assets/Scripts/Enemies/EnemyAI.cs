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

    [Header("Teleport Attack")]
    [SerializeField] private float teleportOffsetFromPlayer = 0.8f;
    [SerializeField] private float teleportCooldown = 3f;

    
    [Header("Lock Times")]
    [SerializeField] private float attackLockTime = 0.6f;
    [SerializeField] private float hitLockTime = 0.25f;
    [SerializeField] private float teleportLockTime = 1.0f;

    [Header("Attack Hitbox")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackPointDistance = 0.55f;
    [SerializeField] private float attackRadius = 0.7f;
    [SerializeField] private LayerMask playerLayer;

    private float attackTimer;
    private float lockTimer;
    private float teleportCooldownTimer;
    private float teleportLockTimer;

    private bool isAttacking;
    private bool isTeleportAttacking;
    private bool isDead;
    private bool playerWasInDetectionRange;

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
        teleportCooldownTimer -= Time.fixedDeltaTime;

        UpdateAttackPointDirection();

        if (isTeleportAttacking)
        {
            teleportLockTimer -= Time.fixedDeltaTime;

            rb.linearVelocity = Vector2.zero;
            animator.SetFloat("Speed", 0f);

            if (teleportLockTimer <= 0f)
                EndTeleportAttack();

            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerIsInDetectionRange = distanceToPlayer <= detectionRange;

        if (playerIsInDetectionRange && !playerWasInDetectionRange && teleportCooldownTimer <= 0f)
        {
            teleportLockTimer = teleportLockTime;
            StartTeleportAttack();
            playerWasInDetectionRange = true;
            return;
        }

        playerWasInDetectionRange = playerIsInDetectionRange;

        if (distanceToPlayer <= attackRange)
        {
            StopMoving();
            TryAttack();
        }
        else if (playerIsInDetectionRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void StartTeleportAttack()
    {
        isTeleportAttacking = true;
        isAttacking = false;
        teleportCooldownTimer = teleportCooldown;

        CancelInvoke(nameof(EndAttack));
        StopMoving();

        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        animator.SetFloat("Speed", 0f);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hit");
        animator.ResetTrigger("Dash");
        animator.SetTrigger("Dash");
    }


    public void TeleportNearPlayer()
    {
        if (player == null || isDead) return;

        Vector2 directionFromPlayerToEnemy = (transform.position - player.position).normalized;

        if (directionFromPlayerToEnemy == Vector2.zero)
            directionFromPlayerToEnemy = Vector2.right;

        transform.position = (Vector2)player.position + directionFromPlayerToEnemy * teleportOffsetFromPlayer;

        rb.linearVelocity = Vector2.zero;

        UpdateAttackPointDirection();
    }

    
    public void EndTeleportAttack()
    {
        isTeleportAttacking = false;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        StopMoving();
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
        if (isAttacking || isTeleportAttacking) return;

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
        if (attackTimer > 0f || isAttacking || isTeleportAttacking) return;

        isAttacking = true;
        attackTimer = attackCooldown;
        lockTimer = attackLockTime;

        StopMoving();

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        Invoke(nameof(EndAttack), attackLockTime);
    }

    public void DamagePlayer()
    {
        if (attackPoint == null) return;

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            playerLayer
        )  ;

        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(1);
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    public void HitStun()
    {
        if (isDead) return;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        lockTimer = hitLockTime;
        isAttacking = false;
        isTeleportAttacking = false;

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
        isTeleportAttacking = false;
        lockTimer = 0f;

        CancelInvoke(nameof(EndAttack));

        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        StopMoving();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}