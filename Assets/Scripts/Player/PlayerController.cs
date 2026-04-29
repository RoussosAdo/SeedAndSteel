using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;

    [Header("Hit Effects")]
    [SerializeField] private GameObject attack1HitEffectPrefab;
    [SerializeField] private GameObject attack2HitEffectPrefab;

    [Header("Combo")]
    [SerializeField] private float attack1Duration = 0.35f;
    [SerializeField] private float attack2Duration = 0.45f;
    [SerializeField] private float comboWindow = 0.45f;

    [Header("Guard")]
    [SerializeField] private float guardDuration = 1f;
    [SerializeField] private float parryWindow = 0.2f;

    public bool IsGuarding => isGuarding;
    public bool CanParry => isGuarding && guardTimer > guardDuration - parryWindow;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private float perfectDodgeWindow = 0.2f;

    public bool IsDashing => isDashing;

    [Header("Dash Afterimage")]
    [SerializeField] private GameObject afterImagePrefab;
    [SerializeField] private float afterImageSpawnRate = 0.04f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attack1SFX;
    [SerializeField] private AudioClip attack2SFX;
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip blockSFX;
    [SerializeField] private AudioClip footstepSFX;
    [SerializeField] private float footstepRate = 0.4f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;

    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    private bool isAttacking;
    private bool isGuarding;
    private bool isDashing;
    private bool canCombo;

    private float attackTimer;
    private float comboTimer;
    private float guardTimer;
    private float dashTimer;
    private float dashCooldownTimer;
    private float perfectDodgeTimer;
    private float afterImageTimer;
    private float footstepTimer;

    private int comboStep;
    private Vector2 dashDirection;
    private GameObject currentHitEffectPrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        ReadMovementInput();
        ReadCombatInput();
        ReadDashInput();

        UpdateCombatTimers();
        UpdateGuardTimer();
        UpdateDashTimer();
        UpdatePerfectDodgeTimer();

        UpdateAnimator();
        UpdateFacingDirection();
        HandleFootsteps();

        dashCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            SpawnAfterImage();
            return;
        }

        if (isAttacking || isGuarding)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    public bool IsInPerfectDodge()
    {
        return perfectDodgeTimer > 0f;
    }

    private void ReadMovementInput()
    {
        if (isAttacking || isGuarding || isDashing)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput.normalized;
    }

    private void ReadCombatInput()
    {
        if (Input.GetMouseButtonDown(1) && !isAttacking && !isGuarding && !isDashing)
            StartGuard();

        if (Input.GetMouseButtonDown(0) && !isGuarding && !isDashing)
        {
            if (!isAttacking)
                StartAttack1();
            else if (canCombo && comboStep == 1)
                StartAttack2();
        }
    }

    private void StartAttack1()
    {
        isAttacking = true;
        comboStep = 1;
        attackTimer = attack1Duration;
        comboTimer = comboWindow;
        canCombo = true;
        currentHitEffectPrefab = attack1HitEffectPrefab;

        PlaySFX(attack1SFX);

        animator.ResetTrigger("Attack1");
        animator.SetTrigger("Attack1");

        HitEnemies();
    }

    private void StartAttack2()
    {
        comboStep = 2;
        attackTimer = attack2Duration;
        comboTimer = 0f;
        canCombo = false;
        currentHitEffectPrefab = attack2HitEffectPrefab;

        PlaySFX(attack2SFX);

        animator.ResetTrigger("Attack2");
        animator.SetTrigger("Attack2");

        HitEnemies();
    }

    private void StartGuard()
    {
        isGuarding = true;
        guardTimer = guardDuration;

        moveInput = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        PlaySFX(blockSFX);
        animator.SetBool("Guard", true);
    }

    private void EndGuard()
    {
        isGuarding = false;
        animator.SetBool("Guard", false);
    }

    private void ReadDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isAttacking && !isGuarding)
            StartDash();
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        perfectDodgeTimer = perfectDodgeWindow;
        afterImageTimer = 0f;

        dashDirection = moveInput != Vector2.zero ? moveInput.normalized : lastMoveDirection;

        if (playerHealth != null)
            playerHealth.SetInvincible(dashDuration);

        PlaySFX(dashSFX);
    }

    private void UpdateDashTimer()
    {
        if (!isDashing) return;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdatePerfectDodgeTimer()
    {
        if (perfectDodgeTimer > 0f)
            perfectDodgeTimer -= Time.deltaTime;
    }

    private void SpawnAfterImage()
    {
        if (afterImagePrefab == null) return;

        afterImageTimer -= Time.fixedDeltaTime;
        if (afterImageTimer > 0f) return;

        afterImageTimer = afterImageSpawnRate;

        GameObject ghost = Instantiate(afterImagePrefab, transform.position, transform.rotation);

        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();
        ghostRenderer.sprite = spriteRenderer.sprite;
        ghostRenderer.flipX = spriteRenderer.flipX;
        ghostRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        ghostRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
    }

    private void UpdateCombatTimers()
    {
        if (!isAttacking) return;

        attackTimer -= Time.deltaTime;

        if (canCombo)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                canCombo = false;
        }

        if (attackTimer <= 0f)
        {
            isAttacking = false;
            comboStep = 0;
            canCombo = false;
            currentHitEffectPrefab = null;
        }
    }

    private void UpdateGuardTimer()
    {
        if (!isGuarding) return;

        guardTimer -= Time.deltaTime;

        if (guardTimer <= 0f)
            EndGuard();
    }

    private void HitEnemies()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);

                SpawnHitEffect(attackPoint.position);

                if (CombatFeedback.Instance != null)
                    CombatFeedback.Instance.PlayHitFeedback();
            }
        }

        Debug.Log("Attack happened. Hits: " + hits.Length);
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (currentHitEffectPrefab == null) return;

        Instantiate(currentHitEffectPrefab, position, Quaternion.identity);
    }

    private void HandleFootsteps()
    {
        if (moveInput == Vector2.zero || isAttacking || isGuarding || isDashing)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            PlaySFX(footstepSFX);
            footstepTimer = footstepRate;
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", moveInput.sqrMagnitude);
    }

    private void UpdateFacingDirection()
    {
        if (isAttacking || isGuarding) return;

        if (moveInput.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (moveInput.x < -0.01f)
            spriteRenderer.flipX = true;

        if (attackPoint == null) return;

        Vector2 attackOffset = lastMoveDirection.normalized;

        if (attackOffset.y > 0.3f)
            attackPoint.localPosition = new Vector3(attackOffset.x * 0.35f, 0.45f, 0f);
        else if (attackOffset.y < -0.3f)
            attackPoint.localPosition = new Vector3(attackOffset.x * 0.35f, -0.45f, 0f);
        else
            attackPoint.localPosition = new Vector3(attackOffset.x * 0.55f, -0.05f, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}