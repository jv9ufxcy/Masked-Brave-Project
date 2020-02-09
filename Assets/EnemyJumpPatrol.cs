using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyJumpPatrol : MonoBehaviour
{
    private Player thePlayer;
    private EnemyHealthManager enemyHealth;
    [Space]
    [Header("Detect Points")]
    [SerializeField] Transform wallDetectPoint;
    [SerializeField] Transform groundDetectPoint;
    [SerializeField] float obstacleDetectRadius = 0.2f, playerDetectRadius = 2f;
    [SerializeField] LayerMask whatCountsAsWall;
    [SerializeField] LayerMask whatCountsAsPlayer;

    [SerializeField] Transform edgeDetectPoint;
    private bool notAtEdge, hittingWall, isPlayerInRange;

    private AudioManager audioManager;

    private bool coroutineStarted=false;
    private bool isFacingRight;
    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;

    [SerializeField] private LayerMask whatIsShootable;
    [SerializeField] private bool isShootableInRange;

    [Space]
    [Header("Enemy Stats")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpStrength, forwardForce=6;
    [SerializeField] private bool moveRight;
    [SerializeField] private bool shouldEnemyWalkOffEdge;
    private bool isOnGround;
    private Vector2 jumpForce;
    [Header("Animations for")]
    private Animator enemyAnim;
    [SerializeField] private string attackAnimation, hurtAnimation, jumpAnimation, windupAnimation;
    [SerializeField] private float attackDelay = 2f;

    [SerializeField] private EnemyState enemyState;
    private enum EnemyState
    {
        Patrolling, Attacking, Damaged,
    }
    // Use this for initialization
    void Start()
    {
        jumpForce = new Vector2(0, jumpStrength);
        thePlayer = FindObjectOfType<Player>();
        enemyRB = GetComponent<Rigidbody2D>();
        enemyRend = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealthManager>();
        enemyAnim = GetComponent<Animator>();
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Patrolling:
                CheckForObstacles();
                break;
            case EnemyState.Attacking:
                break;
            case EnemyState.Damaged:

                break;
            default:
                break;
        }
        PassAnimationSpeed();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        switch (enemyState)
        {
            case EnemyState.Patrolling:
                EnemyMovement();
                break;
            case EnemyState.Attacking:
                if (!coroutineStarted)
                    StartCoroutine(AttackForward());
                break;
            case EnemyState.Damaged:
                //ugokenai
                break;
            default:
                break;
        }
    }

    private void PassAnimationSpeed()
    {
        enemyAnim.SetFloat("vSpeed", enemyRB.velocity.y);
        enemyAnim.SetFloat("hSpeed", enemyRB.velocity.x);

        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, obstacleDetectRadius, whatCountsAsWall);
        isOnGround = groundObjects.Length > 0;

        enemyAnim.SetBool("Ground", isOnGround);
        if (isOnGround)
        {
            enemyAnim.SetBool("Ground", true);
        }
        if (enemyRB.velocity.y < 0)
        {
            enemyAnim.SetBool("Ground", false);
        }
    }
    private void EnemyMovement()
    {
        if (moveRight)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            enemyRB.velocity = new Vector2(moveSpeed, enemyRB.velocity.y);
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            enemyRB.velocity = new Vector2(-moveSpeed, enemyRB.velocity.y);
        }
    }
    public void Jump()
    {
        if (isOnGround)
        {
            enemyAnim.SetBool("Ground", false);
            
            enemyRB.velocity=jumpForce;
            isOnGround = false;
        }
    }

    private void Flip()
    {
        moveRight = !moveRight;
        isFacingRight = !isFacingRight;
    }
    private void CheckForObstacles()
    {
        notAtEdge = Physics2D.OverlapCircle(edgeDetectPoint.position, obstacleDetectRadius, whatCountsAsWall);
        hittingWall = Physics2D.OverlapCircle(wallDetectPoint.position, obstacleDetectRadius, whatCountsAsWall);
        if (hittingWall || (!notAtEdge && !shouldEnemyWalkOffEdge))
            Flip();

        isPlayerInRange = Physics2D.OverlapCircle(edgeDetectPoint.position, playerDetectRadius, whatCountsAsPlayer);
        if (isPlayerInRange&&enemyState==EnemyState.Patrolling)
            enemyState = EnemyState.Attacking;
    }
    IEnumerator AttackForward()
    {
        coroutineStarted = true;
        enemyAnim.Play(windupAnimation);
        enemyRB.velocity = Vector2.zero;
        yield return new WaitForSeconds(attackDelay);
        enemyAnim.Play(attackAnimation);
        Jump();
        if (moveRight)
            enemyRB.velocity = new Vector2(forwardForce, enemyRB.velocity.y);
        else
            enemyRB.velocity = new Vector2(-forwardForce, enemyRB.velocity.y);
        if (isOnGround)
            enemyRB.velocity = Vector2.zero;
        yield return new WaitForSeconds(attackDelay);
        enemyState = EnemyState.Patrolling;
        coroutineStarted = false;
    }
    public void OnDamaged()
    {
        enemyState = EnemyState.Damaged;
        enemyAnim.SetBool("IsHurt", true);
        StopCoroutine(AttackForward());
        coroutineStarted = false;
    }
    public void OnRecovery()
    {
        enemyAnim.SetBool("IsHurt", false);
        enemyState = EnemyState.Patrolling;
    }
}
