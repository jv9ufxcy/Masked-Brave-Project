using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPatrolManager : MonoBehaviour
{
    [SerializeField] private Player thePlayer;

    private Vector2 theTarget, playerTarget;
    private enum BossStates { intro, idle, moveToPlayer, punchAttack, chargeAttack, stunned, formChangeAnim, frenzyIdle, frenzyJump, frenzySlam, frenzySlimeToss, frenzyFist, frenzyArcherAttack };
    [SerializeField] private BossStates _bossStates;

    [SerializeField] private string jumpAnim, punchAttackAnim, chargeAttackAnim, formChangeAnim, fJumpAnim, fSlamAnim, fSlimeTossAnim, fFistAnim, fArcherAnim;
    private bool isInvul, isPhaseTwo;

    [Header("Movement")]
    [SerializeField] private bool moveRight;
    [SerializeField] private float walkSpeed = 4f, chargeAttackSpeed = 20f,jumpStrength, frenzyJumpStrength = 16f, jumpLength = 2f, frenzyDuration = 1.5f;
    private int direction = -1;

    [Space]
    [Header("Patrol Variables")]
    [SerializeField] Transform wallDetectPoint, groundDetectPoint, edgeDetectPoint;
    private Vector2 groundPosition;
    [SerializeField] float groundedRadius = 0.2f;
    [SerializeField] LayerMask whatCountsAsWall, whatCountsAsPlayer;
    private bool notAtEdge;
    private bool hittingWall;
    private bool isOnGround;
    [SerializeField] private bool playerIsToTheRight, isFacingRight, isPlayerInRange;

    [Space]
    [Header("Attacks")]
    [SerializeField] private bool attackFired = false;
    [SerializeField] private float punchRangeDetect = 2f;
    [SerializeField] private float timerToTransition, attackTimer, cooldownTimer = .6f, minTime = 1, walkDuration = 2, punchAttackDuration = 0.6f, punchDelay = 0.3f, chargeAttackDuration = 2f, chargeDelay = 1.5f;
    [SerializeField] private int whichStateToTransitionTo, minChanceToCharge = 4, maxChanceToCharge = 8;

    private int randNum;

    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;
    private Animator enemyAnim;

    //audio
    private AudioManager audioManager;
    [SerializeField] private string enemyTakeDamageSound, enemyDeathSound, jumpSound, chargeAttackSound, punchAttackSound, attackFlashSound;
    private int deathCount = 1;
    [Header("Polish")]
    [SerializeField] private Color defaultColor;
    [SerializeField] private ParticleSystem landingParticles, attackFlashParticle;

    private ParabolaController parabolaController;
    private EnemyHealthManager enemyHM;
    #region Properties

    public bool IsInvul
    {
        get
        {
            return isInvul;
        }

        set
        {
            isInvul = value;
        }
    }
    public bool IsNotAtEdge
    {
        get
        {
            return notAtEdge;
        }

        set
        {
            notAtEdge = value;
        }
    }
    public bool IsHittingWall
    {
        get
        {
            return hittingWall;
        }

        set
        {
            hittingWall = value;
        }
    }

    public bool IsOnGround
    {
        get
        {
            return isOnGround;
        }

        set
        {
            isOnGround = value;
        }
    }

    public bool PlayerIsToTheRight
    {
        get
        {
            return playerIsToTheRight;
        }

        set
        {
            playerIsToTheRight = value;
        }
    }

    public bool IsFacingRight
    {
        get
        {
            return isFacingRight;
        }

        set
        {
            isFacingRight = value;
        }
    }
    #endregion
    // Use this for initialization
    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        enemyHM = GetComponent<EnemyHealthManager>();
        enemyRend = GetComponent<SpriteRenderer>();
        enemyAnim = GetComponent<Animator>();
        enemyRB = GetComponent<Rigidbody2D>();
        parabolaController = GetComponent<ParabolaController>();
        thePlayer = FindObjectOfType<Player>();

        groundPosition = transform.position;

        if (thePlayer != null)
            theTarget = thePlayer.transform.position;
        defaultColor = enemyRend.color;
        timerToTransition = minTime;
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_bossStates != BossStates.punchAttack)
            TurnEnemyAround();

        PassPatrolVariables();
        WhereIsPlayer();
        switch (_bossStates)
        {
            case BossStates.intro:
                randNum = UnityEngine.Random.Range(0, 2);

                timerToTransition -= Time.deltaTime;
                if (timerToTransition <= 0)
                {
                    if (isPlayerInRange)
                    {
                        EnemyAttackChange(chargeAttackDuration, minTime, chargeAttackAnim, chargeAttackSound, BossStates.chargeAttack);
                    }
                    else
                    {
                        TransitionToMove(BossStates.moveToPlayer);
                    }
                }

                break;
            case BossStates.idle:
                timerToTransition -= Time.deltaTime;
                randNum = UnityEngine.Random.Range(0, minChanceToCharge);
                ChangeDirection();
                if (timerToTransition <= 0)
                {
                    if (isPlayerInRange)
                    {
                        EnemyAttackChange(punchAttackDuration, minTime, punchAttackAnim, punchAttackSound, BossStates.punchAttack);
                    }
                    else
                    {
                        if (randNum > 0)
                            TransitionToMove(BossStates.moveToPlayer);
                        else
                             EnemyAttackChange(chargeAttackDuration, minTime, chargeAttackAnim, chargeAttackSound, BossStates.chargeAttack);
                    }
                }
                break;
            case BossStates.chargeAttack:

                break;
            case BossStates.moveToPlayer:
                ChangeDirection();
                timerToTransition -= Time.deltaTime;
                
                if (timerToTransition <= 0)
                {
                    if (isPlayerInRange)
                    {
                        EnemyAttackChange(punchAttackDuration, minTime, punchAttackAnim, punchAttackSound, BossStates.punchAttack);
                    }
                    else
                    {
                        EnemyAttackChange(chargeAttackDuration, minTime, chargeAttackAnim, chargeAttackSound, BossStates.chargeAttack);
                    }
                }
                else
                {
                    if (isPlayerInRange)
                    {
                        EnemyAttackChange(punchAttackDuration, minTime, punchAttackAnim, punchAttackSound, BossStates.punchAttack);
                    }
                }
                break;
            case BossStates.punchAttack:
                if (!attackFired)
                {
                    StartCoroutine(PunchAttack());
                }
                if (attackTimer <= 0)
                {
                    TransitionToMove(BossStates.idle);
                }
                else
                {
                    timerToTransition = minTime;
                    attackTimer -= Time.deltaTime;
                }

                break;
            case BossStates.formChangeAnim:
               
                break;
            case BossStates.frenzyIdle:
               
                break;
            case BossStates.frenzyJump:
               
                break;
            case BossStates.frenzySlam:
                break;
            case BossStates.frenzySlimeToss:
                
                break;
            case BossStates.frenzyFist:
                break;
            case BossStates.frenzyArcherAttack:
                
                break;
            default:
                break;
        }
    }

    private void TransitionToMove( BossStates actionState)
    {
        GetTargetLocation();
        timerToTransition = walkDuration;
        _bossStates = actionState;
    }

    private void EnemyAttackChange(float attackDuration, float transitionDelay, string attackAnim, string attackSound, BossStates ActionState)
    {
        attackFired = false;
        attackTimer = attackDuration;
        //cooldownTimer = attackDuration;
        //timerToTransition = transitionDelay;
        attackFlashParticle.Play();
        audioManager.PlaySound(attackFlashSound);
        ChangeDirection();

        enemyAnim.Play(attackAnim);
        _bossStates = ActionState;
    }

    private void FixedUpdate()
    {
        switch (_bossStates)
        {
            case BossStates.intro:
                break;
            case BossStates.idle:
                break;
            case BossStates.moveToPlayer:
                Move(walkSpeed);
                break;
            case BossStates.punchAttack:
                enemyRB.velocity = Vector2.zero;
                break;
            case BossStates.chargeAttack:
                if (attackTimer <= 0)
                {
                    _bossStates = BossStates.idle;
                }
                else
                {
                    timerToTransition = minTime;
                    attackTimer -= Time.fixedDeltaTime;
                    if (!attackFired)
                    {
                        StartCoroutine(ChargeAttack());
                    }
                }
                break;
            case BossStates.formChangeAnim:
                break;
            case BossStates.frenzyIdle:
                break;
            case BossStates.frenzyJump:
                break;
            case BossStates.frenzySlam:
                break;
            case BossStates.frenzySlimeToss:
                break;
            case BossStates.frenzyFist:
                break;
            case BossStates.frenzyArcherAttack:
                break;
            default:
                break;
        }

    }

    public void Move(float movementSpeed)
    {
        enemyRB.velocity = new Vector2(movementSpeed * direction, enemyRB.velocity.y);
    }
    IEnumerator PunchAttack()
    {
        attackFired = true;
        GetPlayerLocation();
        yield return new WaitForSeconds(punchDelay);
        audioManager.PlaySound(punchAttackSound);

    }
    IEnumerator ChargeAttack()
    {
        attackFired = true;
        enemyRB.velocity = Vector2.zero;
        yield return new WaitForSeconds(chargeDelay);
        audioManager.PlaySound(chargeAttackSound);
        Move(chargeAttackSpeed);
    }
    public void OnHalfHealth()
    {
        Debug.Log("Phase 2");
        isPhaseTwo = true;
        enemyAnim.SetBool("PhaseTwo", isPhaseTwo);
        //enemyAnim.Play(formChangeAnim);
        //_bossStates = BossStates.formChangeAnim;
        timerToTransition = frenzyDuration;
        walkSpeed = 16f;
    }
    public void OnDamaged()
    {
        StopCoroutine(ChargeAttack());
        StopCoroutine(PunchAttack());
        _bossStates = BossStates.stunned;
        enemyAnim.SetBool("IsHurt", true);
    }
    public void OnRecovery()
    {
        enemyAnim.SetBool("IsHurt", false);
        timerToTransition = minTime;
        _bossStates = BossStates.idle;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, punchRangeDetect);
    }
    private void PassPatrolVariables()
    {
        isPlayerInRange = Physics2D.OverlapCircle(transform.position, punchRangeDetect, whatCountsAsPlayer);
        IsNotAtEdge = Physics2D.OverlapCircle(edgeDetectPoint.position, groundedRadius, whatCountsAsWall);
        IsHittingWall = Physics2D.OverlapCircle(wallDetectPoint.position, groundedRadius, whatCountsAsWall);

        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, groundedRadius, whatCountsAsWall);
        IsOnGround = groundObjects.Length > 0;
        enemyAnim.SetFloat("hSpeed", Mathf.Abs(enemyRB.velocity.x));
        enemyAnim.SetFloat("vSpeed", enemyRB.velocity.y);
        enemyAnim.SetBool("Ground", IsOnGround);
        if (isOnGround)
        {
            enemyAnim.SetBool("Ground", true);
        }
        if (enemyRB.velocity.y < 0)
        {
            enemyAnim.SetBool("Ground", false);
        }
    }
    private void TurnEnemyAround()
    {
        if (IsHittingWall)
            Flip();
    }
    private void Flip()
    {
        moveRight = !moveRight;
        IsFacingRight = !IsFacingRight;
    }
    private void FlipFacingRight()
    {
        direction = 1;
        moveRight = true;
        IsFacingRight = true;
        transform.localScale = new Vector3(-1f, 1f, 1f);
    }
    private void FlipFacingLeft()
    {
        direction = -1;
        moveRight = false;
        IsFacingRight = false;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }
    private void CheckLocationOfPlayer()
    {
        moveRight = PlayerIsToTheRight;
    }
    private void WhereIsPlayer()
    {
        //if player is to the right, charge at the right side
        if (thePlayer.transform.position.x > transform.position.x)
            PlayerIsToTheRight = true;
        else
            PlayerIsToTheRight = false;
    }
    private void GetTargetLocation()
    {
        if (thePlayer != null)
        {
            theTarget = new Vector2(thePlayer.transform.position.x - (direction * 3), groundPosition.y);
            playerTarget = new Vector2(thePlayer.transform.position.x - (direction * 3), groundPosition.y);
        }
    }
    private void GetPlayerLocation()
    {
        if (thePlayer != null)
        {
            playerTarget = new Vector2(thePlayer.transform.position.x, groundPosition.y + 1);
        }
    }
    private void ChangeDirection()
    {
        if (PlayerIsToTheRight)
        {
            FlipFacingRight();
        }
        else
        {
            FlipFacingLeft();
        }
    }
}