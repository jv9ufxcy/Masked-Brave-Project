using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPatrolManager : MonoBehaviour
{
    [SerializeField] private Player thePlayer;
    private enum BossStates { intro, idle, run, jump, longRangeAttack, closeRangeAttack, superAttack, stunned };
    [SerializeField] private BossStates _bossStates;
    [SerializeField] private string jumpAnim, chargeAnim, idleAnim;
    [SerializeField] private bool isEnemyInvincible = false;
    [SerializeField] private int currentEnemyHealth, maxEnemyHealth;

    [SerializeField] private GameObject deathParticle, itemDropped, energyDropped;

    [SerializeField] private bool shouldDropAtHalf;


    //knockback
    public float enemyKnockbackDuration, enemyForce, enemyMaxKnockbackDuration;
    public bool enemyKnockFromRight;
    private bool isInvul;
    [SerializeField] private float damageCooldownInSeconds = .75f;

    [Header("Movement")]
    [SerializeField] private float jumpStrength, moveSpeed;
    private bool moveRight;
    private Vector2 jumpForce;
    private float defaultGravityScale;

    //patrol variables
    [SerializeField] private float detectionRange=40;
    [SerializeField] Transform wallDetectPoint, groundDetectPoint, edgeDetectPoint;
    [SerializeField] float DetectRadius = 0.2f;
    [SerializeField] LayerMask whatCountsAsWall;
    private bool notAtEdge;
    private bool hittingWall;
    private bool isOnGround;
    [SerializeField] private bool playerIsToTheRight, isFacingRight;

    [SerializeField] private float timerToTransition, attackTimer, cooldownTimer=.6f, minTime = 4, maxTime = 8, longRangeAttackTime=0.8f;
    [SerializeField] private int whichStateToTransitionTo, minChanceToCharge = 4, maxChanceToCharge = 8;

    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;
    private Animator enemyAnim;
    [SerializeField] private BossDoorTrigger bossDoorTrigger;
    //audio
    private AudioManager audioManager;
    [SerializeField] private string enemyTakeDamageSound, enemyDeathSound, jumpSound, shortRangeSound, longRangeSound, attackFlashSound;
    private int deathCount=1;
    [Header("Polish")]
    [SerializeField] private Color defaultColor;
    [SerializeField] private ParticleSystem landingParticles, attackFlashParticle;
    #region Properties
    public int CurrentHitPoints
    {
        get
        {
            return currentEnemyHealth;
        }
        private set
        {
            currentEnemyHealth = value;
            if (currentEnemyHealth < 0)
                currentEnemyHealth = 0;
            if (currentEnemyHealth > maxEnemyHealth)
                currentEnemyHealth = maxEnemyHealth;
        }
    }

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
        enemyRend = GetComponent<SpriteRenderer>();
        enemyAnim = GetComponent<Animator>();
        enemyRB = GetComponent<Rigidbody2D>();
        bossDoorTrigger = GetComponent<BossDoorTrigger>();
        currentEnemyHealth = maxEnemyHealth;

        defaultGravityScale = enemyRB.gravityScale;

        thePlayer = FindObjectOfType<Player>();

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
        if (_bossStates != BossStates.longRangeAttack)
            TurnEnemyAround();

        PassPatrolVariables();
        WhereIsPlayer();
        switch (_bossStates)
        {
            case BossStates.intro:
                int randNum = UnityEngine.Random.Range(0, 2);

                timerToTransition -= Time.deltaTime;
                if (timerToTransition <= 0)
                {
                    if (randNum == 0)
                    {
                        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
                        whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
                        ChangeDirection();
                        _bossStates = BossStates.run;
                    }
                    else
                    {
                        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
                        whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
                        enemyAnim.Play(jumpAnim);
                        _bossStates = BossStates.jump;
                    }
                }
                    
                break;
            case BossStates.idle:
                timerToTransition -= Time.deltaTime;
                if (timerToTransition <= 0)
                {
                    if (whichStateToTransitionTo <= minChanceToCharge)
                    {
                        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
                        whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
                        ChangeDirection();
                        _bossStates = BossStates.run;
                    }
                    else
                    {
                        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
                        whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
                        enemyAnim.Play(jumpAnim);
                        _bossStates = BossStates.jump;
                    }
                }
                break;
            case BossStates.run:
                timerToTransition -= Time.deltaTime;
                if (timerToTransition <= 0)
                {
                    if (whichStateToTransitionTo <= minChanceToCharge)
                    {
                        attackTimer = longRangeAttackTime;
                        cooldownTimer = longRangeAttackTime;
                        timerToTransition = minTime;
                        attackFlashParticle.Play();
                        ChangeDirection();
                        enemyAnim.Play(chargeAnim);
                        audioManager.PlaySound(jumpSound);
                        _bossStates = BossStates.longRangeAttack;
                    }
                    else
                    {
                        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
                        whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
                        enemyAnim.Play(jumpAnim);
                        _bossStates = BossStates.jump;
                    }
                }
                else
                    timerToTransition -= Time.deltaTime;
                break;
            case BossStates.jump:
                if (isOnGround)
                {
                    if (whichStateToTransitionTo <= minChanceToCharge)
                    {
                        attackTimer = longRangeAttackTime;
                        cooldownTimer = longRangeAttackTime;
                        timerToTransition = minTime;
                        attackFlashParticle.Play();
                        ChangeDirection();
                        enemyAnim.Play(chargeAnim);
                        audioManager.PlaySound(jumpSound);
                        _bossStates = BossStates.longRangeAttack;
                    }
                    else
                    {
                        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
                        whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
                        ChangeDirection();
                        _bossStates = BossStates.run;
                    }
                }
                break;
            case BossStates.longRangeAttack:
                if (timerToTransition<=0)
                {
                    timerToTransition = minTime;
                    whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
                    ChangeDirection();
                    enemyAnim.SetBool("Shield", false);
                    enemyAnim.Play(idleAnim);
                    _bossStates = BossStates.idle;
                }

                break;
            case BossStates.closeRangeAttack:
                break;
            case BossStates.superAttack:
                break;
            default:
                break;
        }
    }
    private void FixedUpdate()
    {
        switch (_bossStates)
        {
            case BossStates.intro:
                break;
            case BossStates.idle:
                break;
            case BossStates.run:
                RunningBehavior();
                break;
            case BossStates.jump:
                Jump();
                RunningBehavior();
                break;
            case BossStates.longRangeAttack:
                
                if (attackTimer <= 0)
                {
                    Charge();
                    timerToTransition -= Time.fixedDeltaTime;
                }
                else
                {
                    timerToTransition = cooldownTimer;
                    attackTimer -= Time.fixedDeltaTime;
                    enemyRB.velocity = Vector2.zero;
                }
                break;
            case BossStates.closeRangeAttack:
                break;
            case BossStates.superAttack:
                break;
            default:
                break;
        }

    }
    private void RunningBehavior()
    {
        if (moveRight)
        {
            FlipFacingRight();
            enemyRB.velocity = new Vector2(moveSpeed, enemyRB.velocity.y);
        }
        else
        {
            FlipFacingLeft();
            enemyRB.velocity = new Vector2(-moveSpeed, enemyRB.velocity.y);
        }
    }

    public void Jump()
    {
        if (isOnGround)
        {
            enemyRB.velocity = new Vector2(enemyRB.velocity.x,jumpStrength);
            audioManager.PlaySound(jumpSound);
            landingParticles.Play();
        }
    }
    public void OnDamaged()
    {
        _bossStates = BossStates.stunned;
        enemyAnim.SetBool("Shield", false);
        enemyAnim.SetBool("IsHurt", true);
    }
    public void OnRecovery()
    {
        enemyAnim.SetBool("IsHurt", false);
        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
        whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
        _bossStates = BossStates.run;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
    private void PassPatrolVariables()
    {
        IsNotAtEdge = Physics2D.OverlapCircle(edgeDetectPoint.position, DetectRadius, whatCountsAsWall);
        IsHittingWall = Physics2D.OverlapCircle(wallDetectPoint.position, DetectRadius, whatCountsAsWall);

        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, DetectRadius, whatCountsAsWall);
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
    public void FlipFacingRight()
    {
        moveRight = true;
        IsFacingRight = true;
        transform.localScale = new Vector3(-1f, 1f, 1f);
    }
    public void FlipFacingLeft()
    {
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
    public void Charge()
    {
        if (!IsFacingRight)
            enemyRB.velocity = new Vector2(-enemyForce, 0);
        else if (isFacingRight)
            enemyRB.velocity = new Vector2(enemyForce, 0);
    }

    public void ChangeDirection()
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
