using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KhrisSlimeBoss : MonoBehaviour
{
    [SerializeField] private Player thePlayer;

    private Vector2 theTarget;
    private enum BossStates { intro, idle, jump, slimeToss, slimeStab, stunned, frenzyIntro, frenzyIdle, frenzyJump, frenzySlam, frenzySlimeToss, frenzyFist, frenzySuperAttack };
    [SerializeField] private BossStates _bossStates;

    [SerializeField] private string jumpAnim, slimeTossAnim, slimeStabAnim, frenzyAnim, fJumpAnim, fSlamAnim, fSlimeTossAnim, fFistAnim, fSuperAnim;
    private bool isInvul;

    [Header("Movement")]
    [SerializeField] private float jumpStrength, frenzyJumpStrength=16f, jumpLength = 2f, frenzyJumpLength = 1.5f;
    protected float Animation;
    private bool moveRight;
    private Vector2 jumpForce;
    private float defaultGravityScale;

    [Header("Gun")]
    [SerializeField] private GameObject slimeBullet;
    [SerializeField] private float fireRate = .5f, timeToNextFire = 0f,bulletSpeed=5f;
    //patrol variables
    [SerializeField] private float detectionRange = 40, stabRangeDetect=4f, fistRangeDetect=6f;
    [SerializeField] Transform wallDetectPoint, groundDetectPoint, edgeDetectPoint;
    [SerializeField] Transform[] parabolaPoints;
    [SerializeField] float DetectRadius = 0.2f;
    [SerializeField] LayerMask whatCountsAsWall, whatCountsAsPlayer;
    private bool notAtEdge;
    private bool hittingWall;
    private bool isOnGround;
    [SerializeField] private bool playerIsToTheRight, isFacingRight,isPlayerInRange;

    [SerializeField] private float timerToTransition, attackTimer, cooldownTimer = .6f, minTime = 1, maxTime = 2, throwSlimeDur = 0.8f, slimeStabDur=2f, fFistDur=2.2f, slimeArcher= 2.5f;
    [SerializeField] private int whichStateToTransitionTo, minChanceToCharge = 4, maxChanceToCharge = 8;

    private int randNum;

    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;
    private Animator enemyAnim;

    //audio
    private AudioManager audioManager;
    [SerializeField] private string enemyTakeDamageSound, enemyDeathSound, jumpSound, slimeFistSound, throwSlimeSound, attackFlashSound;
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

        if (thePlayer!=null)
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
        if (_bossStates != BossStates.slimeToss)
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
                        EnemyAttackChange(slimeStabDur, minTime, slimeStabAnim, slimeFistSound, BossStates.slimeStab);
                    }
                    else 
                    {
                        if (randNum>0)
                        {
                            GetTargetLocation();
                            timerToTransition = minTime;
                            enemyAnim.Play(jumpAnim);
                            _bossStates = BossStates.jump;
                        }
                        else
                            EnemyAttackChange(throwSlimeDur, minTime, slimeTossAnim, throwSlimeSound, BossStates.slimeToss);

                    }
                }

                break;
            case BossStates.idle:
                timerToTransition -= Time.deltaTime;
                randNum = UnityEngine.Random.Range(0, 2);

                if (timerToTransition <= 0)
                {
                    ChangeDirection();
                    if (isPlayerInRange)
                    {
                        EnemyAttackChange(slimeStabDur, minTime, slimeStabAnim, slimeFistSound, BossStates.slimeStab);
                    }
                    else
                    {
                        if (randNum > 0)
                        {
                            GetTargetLocation();
                            timerToTransition = minTime;
                            enemyAnim.Play(jumpAnim);
                            _bossStates = BossStates.jump;
                        }
                        else
                            EnemyAttackChange(throwSlimeDur, minTime, slimeTossAnim, throwSlimeSound, BossStates.slimeToss);

                    }
                }
                break;
            case BossStates.slimeStab:

                break;
            case BossStates.jump:
                timerToTransition -= Time.deltaTime;
                if (IsOnGround && timerToTransition <= 0)
                    {
                        ChangeDirection();
                        if (!isPlayerInRange)
                        {
                            EnemyAttackChange(throwSlimeDur, minTime, slimeTossAnim, throwSlimeSound, BossStates.slimeToss);
                        }
                        else
                        {
                            EnemyAttackChange(slimeStabDur, minTime, slimeStabAnim, slimeFistSound, BossStates.slimeStab);
                        }
                    }
                break;
            case BossStates.slimeToss:
                timeToNextFire -= Time.deltaTime;
                if (timeToNextFire < 0)
                {
                    ShootPlayer();
                }
                    if (attackTimer <= 0)
                    {
                        GetTargetLocation();
                        timerToTransition = minTime;
                        enemyAnim.Play(jumpAnim);
                        _bossStates = BossStates.jump;
                    }
                    else
                    {
                        timerToTransition = minTime;
                        attackTimer -= Time.deltaTime;
                    }
                
                    break;
            case BossStates.frenzyIntro:
                break;
            case BossStates.frenzyIdle:
                timerToTransition -= Time.deltaTime;
                randNum = UnityEngine.Random.Range(0, 2);

                if (timerToTransition <= 0)
                {
                    ChangeDirection();
                    if (isPlayerInRange)
                    {
                        EnemyAttackChange(fFistDur, minTime, fFistAnim, slimeFistSound, BossStates.frenzyFist);
                    }
                    else
                    {
                        if (randNum == 0)
                        {
                            timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
                            whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
                            GetTargetLocation();
                            enemyAnim.Play(fJumpAnim);
                            _bossStates = BossStates.frenzyJump;
                        }
                        else
                            EnemyAttackChange(throwSlimeDur, minTime, fSlimeTossAnim, throwSlimeSound, BossStates.frenzySlimeToss);

                    }
                }
                break;
            case BossStates.frenzyJump:
                if (isOnGround)
                {
                    ChangeDirection();
                    _bossStates = BossStates.frenzyIdle;
                }
                break;
            case BossStates.frenzySlam:
                break;
            case BossStates.frenzySlimeToss:
                timeToNextFire -= Time.deltaTime;
                if (timeToNextFire < 0)
                {
                    ShootPlayer();
                }
                break;
            case BossStates.frenzyFist:
                break;
            case BossStates.frenzySuperAttack:
                break;
            default:
                break;
        }
    }

    private void EnemyAttackChange(float attackDuration, float transitionDelay,string attackAnim, string attackSound, BossStates attackState)
    {
        attackTimer = attackDuration;
        cooldownTimer = attackDuration;
        timerToTransition = transitionDelay;
        attackFlashParticle.Play();
        ChangeDirection();

        enemyAnim.Play(attackAnim);
        audioManager.PlaySound(attackSound);
        _bossStates = attackState;
    }

    private void FixedUpdate()
    {
        switch (_bossStates)
        {
            case BossStates.intro:
                break;
            case BossStates.idle:
                break;
            case BossStates.jump:
                if (timerToTransition>=minTime)
                {
                    Jump(jumpStrength, jumpLength);
                }
                break;
            case BossStates.slimeToss:
                
                    enemyRB.velocity = Vector2.zero;

                break;
            case BossStates.slimeStab:
                if (attackTimer <= 0)
                {
                    _bossStates = BossStates.idle;
                }
                else
                {
                    timerToTransition = maxTime;
                    attackTimer -= Time.fixedDeltaTime;
                    enemyRB.velocity = Vector2.zero;
                }
                break;
            case BossStates.frenzyIntro:
                break;
            case BossStates.frenzyIdle:
                break;
            case BossStates.frenzyJump:
                Jump(jumpStrength*2, jumpLength*2);
                break;
            case BossStates.frenzySlam:
                break;
            case BossStates.frenzySlimeToss:
                if (attackTimer <= 0)
                {
                    _bossStates = BossStates.frenzyIdle;
                }
                else
                {
                    timerToTransition = maxTime;
                    attackTimer -= Time.fixedDeltaTime;
                    enemyRB.velocity = Vector2.zero;
                }
                break;
            case BossStates.frenzyFist:
                if (attackTimer <= 0)
                {
                    timerToTransition -= Time.fixedDeltaTime;
                }
                else
                {
                    timerToTransition = minTime;
                    attackTimer -= Time.fixedDeltaTime;
                    enemyRB.velocity = Vector2.zero;
                }
                break;
            case BossStates.frenzySuperAttack:
                if (attackTimer <= 0)
                {
                    _bossStates = BossStates.frenzyIdle;
                }
                else
                {
                    timerToTransition = maxTime;
                    attackTimer -= Time.fixedDeltaTime;
                    enemyRB.velocity = Vector2.zero;
                }
                break;
            default:
                break;
        }

    }

    public void Jump(float jumpForce, float jumpLength)
    {
        parabolaController.FollowParabola();
        audioManager.PlaySound(jumpSound);
        landingParticles.Play();

        //Animation += Time.fixedDeltaTime;
        //Animation %= 5f;
        //    transform.position = MathParabola.Parabola(transform.position, theTarget, jumpForce, Animation/5f);
    }
    private void ShootPlayer()
    {
        timeToNextFire = fireRate;
        audioManager.PlaySound(attackFlashSound);

        Vector2 moveDirection = (thePlayer.transform.position - transform.position).normalized * bulletSpeed;

        GameObject newbullet = Instantiate(slimeBullet, wallDetectPoint.position, Quaternion.identity);
        //newbullet.GetComponent<HurtPlayerOnHit>().enemyHM = enemyHM;
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(moveDirection.x, moveDirection.y);
    }
    public void OnHalfHealth()
    {
        enemyAnim.SetBool("Frenzy", true);
        _bossStates = BossStates.frenzyIntro;
    }
    public void OnDamaged()
    {
        _bossStates = BossStates.stunned;
        enemyAnim.SetBool("IsHurt", true);
    }
    public void OnRecovery()
    {
        enemyAnim.SetBool("IsHurt", false);
        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);
        whichStateToTransitionTo = UnityEngine.Random.Range(0, maxChanceToCharge);
        _bossStates = BossStates.idle;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, stabRangeDetect);
    }
    private void PassPatrolVariables()
    {
        isPlayerInRange = Physics2D.OverlapCircle(edgeDetectPoint.position, stabRangeDetect, whatCountsAsPlayer);
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
    private void FlipFacingRight()
    {
        moveRight = true;
        IsFacingRight = true;
        transform.localScale = new Vector3(-1f, 1f, 1f);
    }
    private void FlipFacingLeft()
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
    private void GetTargetLocation()
    {
        if (thePlayer != null)
            theTarget = new Vector2(thePlayer.transform.position.x-3, transform.position.y);
        Debug.Log("TargetPos: " + theTarget);
        parabolaPoints[0].transform.position = transform.position;
        //parabolaPoints[1].transform.position = new Vector2(transform.position.x/* + jumpLength*/, transform.position.y + jumpStrength);
        parabolaPoints[2].transform.position = theTarget;
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
public class MathParabola
{

    //public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    //{
    //    Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

    //    var mid = Vector3.Lerp(start, end, t);

    //    return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    //}

    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
    }

}
