using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Combat")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Combo")]
    [SerializeField] private float attack1Duration = 0.35f;
    [SerializeField] private float attack2Duration = 0.45f;
    [SerializeField] private float comboWindow = 0.45f;

    [Header("Guard")]
    [SerializeField] private float guardDuration = 1f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    private bool isAttacking;
    private bool isGuarding;
    private bool canCombo;

    private float attackTimer;
    private float comboTimer;
    private float guardTimer;

    private int comboStep;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ReadMovementInput();
        ReadCombatInput();

        UpdateCombatTimers();
        UpdateGuardTimer();

        UpdateAnimator();
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (isAttacking || isGuarding)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    private void ReadMovementInput()
    {
        if (isAttacking || isGuarding)
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
        if (Input.GetMouseButtonDown(1) && !isAttacking && !isGuarding)
        {
            StartGuard();
        }

        if (Input.GetMouseButtonDown(0) && !isGuarding)
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

        animator.SetBool("Guard", true);
    }

    private void EndGuard()
    {
        isGuarding = false;
        animator.SetBool("Guard", false);
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
            Debug.Log("Hit enemy: " + hit.name);
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", moveInput.sqrMagnitude);
    }

    private void UpdateFacingDirection()
    {
        if (isAttacking || isGuarding) return;

        if (moveInput.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        if (attackPoint != null)
            attackPoint.localPosition = lastMoveDirection * 0.7f;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}