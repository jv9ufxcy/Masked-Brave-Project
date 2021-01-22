using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KhrisSlimeBoss : MonoBehaviour
{
    [SerializeField] private Player thePlayer;

    private Vector2 theTarget,playerTarget;
    private enum BossStates { intro, idle, jump, slimeToss, slimeStab, stunned, frenzyIntro, frenzyIdle, frenzyJump, frenzySlam, frenzySlimeToss, frenzyFist, frenzyArcherAttack };
    [SerializeField] private BossStates _bossStates;

    [SerializeField] private string jumpAnim, slimeTossAnim, slimeStabAnim, frenzyAnim, fJumpAnim, fSlamAnim, fSlimeTossAnim, fFistAnim, fArcherAnim;
    private bool isInvul, isFrenzy;

    [Header("Movement")]
    [SerializeField] private bool moveRight;
    [SerializeField] private float jumpStrength, frenzyJumpStrength=16f, jumpLength = 2f, frenzyDuration = 1.5f;
    private int direction = -1;
    protected float Animation;
    [Space]
    [Header("Gun")]
    [SerializeField] private bool fired=false;
    [SerializeField] private GameObject slimeBullet, greatArrow;
    [SerializeField] private float timeToNextFire = 0f, slimeFireRate = .5f, archerFireRate = 2f, bulletSpeed = 20f, greatArrowSpeed = 30f;
    [Space]
    [Header("Patrol Variables")]
    [SerializeField] Transform[] parabolaPoints;
    [SerializeField] private float detectionRange = 40, stabRangeDetect=4f, fistRangeDetect=6f;
    [SerializeField] Transform wallDetectPoint, groundDetectPoint, edgeDetectPoint;
    private Vector2 groundPosition;
    [SerializeField] float DetectRadius = 0.2f;
    [SerializeField] LayerMask whatCountsAsWall, whatCountsAsPlayer;
    private bool notAtEdge;
    private bool hittingWall;
    private bool isOnGround;
    [SerializeField] private bool playerIsToTheRight, isFacingRight,isPlayerInRange;

    [SerializeField] private float timerToTransition, attackTimer, cooldownTimer = .6f, minTime = 1, maxTime = 2, throwSlimeDur = 0.6f, throwSlimeAction=0.3f, slimeStabDur=2f, fFistDur=2.2f, slimeArcherDur= 2.5f,slimeArcherShot=1.5f;
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

        groundPosition = transform.position;

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
                        TransitionToJump(jumpAnim, BossStates.jump);
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
                            TransitionToJump(jumpAnim, BossStates.jump);
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
                if (!fired)
                {
                    StartCoroutine(ThrowSlime());
                }
                if (attackTimer <= 0)
                {
                    ThrowSlime();
                    TransitionToJump(jumpAnim, BossStates.jump);
                }
                else
                {
                    timerToTransition = minTime;
                    attackTimer -= Time.deltaTime;
                }

                break;
            case BossStates.frenzyIntro:
                randNum = UnityEngine.Random.Range(0, 2);

                timerToTransition -= Time.deltaTime;
                if (timerToTransition <= 0)
                {
                    if (isPlayerInRange)
                    {
                        EnemyAttackChange(fFistDur, minTime, fFistAnim, slimeFistSound, BossStates.frenzyFist);
                    }
                    else
                    {
                        TransitionToJump(fJumpAnim, BossStates.frenzyJump);
                    }
                }

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
                        switch (randNum)
                        {
                            case 0:
                                TransitionToJump(fJumpAnim, BossStates.frenzyJump);
                                break;
                            case 1:
                                EnemyAttackChange(throwSlimeDur, minTime, fSlimeTossAnim, throwSlimeSound, BossStates.frenzySlimeToss);
                                break;
                            case 2:
                                EnemyAttackChange(slimeArcherDur, minTime, fArcherAnim, throwSlimeSound, BossStates.frenzyArcherAttack);
                                break;
                            default:
                                break;
                        }
                    }
                }
                break;
            case BossStates.frenzyJump:
                timerToTransition -= Time.deltaTime;
                randNum = UnityEngine.Random.Range(0, 2);
                if (IsOnGround && timerToTransition <= 0)
                {
                    ChangeDirection();
                    if (!isPlayerInRange)
                    {
                        EnemyAttackChange(throwSlimeDur, minTime, fSlimeTossAnim, throwSlimeSound, BossStates.frenzySlimeToss);
                    }
                    else
                    {
                        switch (randNum)
                        {
                            case 0:
                                EnemyAttackChange(slimeArcherDur, minTime, fArcherAnim, throwSlimeSound, BossStates.frenzyArcherAttack);
                                break;
                            case 1:
                                EnemyAttackChange(throwSlimeDur, minTime, fSlimeTossAnim, throwSlimeSound, BossStates.frenzySlimeToss);
                                break;
                            case 2:
                                EnemyAttackChange(slimeArcherDur, minTime, fArcherAnim, throwSlimeSound, BossStates.frenzyArcherAttack);
                                break;
                            default:
                                break;
                        }
                    }
                }
                break;
            case BossStates.frenzySlam:
                break;
            case BossStates.frenzySlimeToss:
                timeToNextFire -= Time.deltaTime;
                randNum = UnityEngine.Random.Range(0, 2);
                if (!fired)
                {
                    StartCoroutine(ThrowSlime());
                }
                if (attackTimer <= 0)
                {
                    switch (randNum)
                    {
                        case 0:
                            TransitionToJump(fJumpAnim, BossStates.frenzyJump);
                            break;
                        case 1:
                            EnemyAttackChange(slimeArcherDur, minTime, fArcherAnim, throwSlimeSound, BossStates.frenzyArcherAttack);
                            break;
                        case 2:
                            EnemyAttackChange(slimeArcherDur, minTime, fArcherAnim, throwSlimeSound, BossStates.frenzyArcherAttack);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    timerToTransition = minTime;
                    attackTimer -= Time.deltaTime;
                }

                break;
            case BossStates.frenzyFist:
                break;
            case BossStates.frenzyArcherAttack:
                timeToNextFire -= Time.deltaTime;
                if (!fired)
                {
                    StartCoroutine(GreatArrowShot());
                }
                if (attackTimer <= 0)
                {
                    TransitionToJump(fJumpAnim, BossStates.frenzyJump);
                }
                else
                {
                    timerToTransition = minTime;
                    attackTimer -= Time.deltaTime;
                }

                break;
            default:
                break;
        }
    }

    private void TransitionToJump(string jumpingAnimation, BossStates attackState)
    {
        GetTargetLocation();
        timerToTransition = minTime;
        enemyAnim.Play(jumpingAnimation);
        _bossStates = attackState;
    }

    private void EnemyAttackChange(float attackDuration, float transitionDelay,string attackAnim, string attackSound, BossStates attackState)
    {
        fired = false;
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
                if (timerToTransition >= minTime)
                {
                    Jump(jumpStrength, jumpLength);
                }
                break;
            case BossStates.frenzySlam:
                break;
            case BossStates.frenzySlimeToss:
                enemyRB.velocity = Vector2.zero;
                break;
            case BossStates.frenzyFist:
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
            case BossStates.frenzyArcherAttack:
                enemyRB.velocity = Vector2.zero;
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
    IEnumerator ThrowSlime()
    {
        fired = true;
        GetPlayerLocation();
        //timeToNextFire = slimeFireRate;

        yield return new WaitForSeconds(throwSlimeAction);
        audioManager.PlaySound(attackFlashSound);

        Vector2 moveDirection = (thePlayer.transform.position - transform.position).normalized * bulletSpeed;

        GameObject newbullet = Instantiate(slimeBullet, wallDetectPoint.position, Quaternion.identity);
        newbullet.GetComponent<ParabolaController>().ParabolaRoot = parabolaController.ParabolaRoot;
    }
    IEnumerator GreatArrowShot()
    {
        fired = true;
        GetPlayerLocation();
        //timeToNextFire = archerFireRate;
        audioManager.PlaySound(attackFlashSound);

        yield return new WaitForSeconds(slimeArcherShot);

        GameObject newArrow = Instantiate(greatArrow, wallDetectPoint.position, Quaternion.identity);
        newArrow.GetComponent<Rigidbody2D>().velocity = new Vector2(greatArrowSpeed*direction,0);
        newArrow.transform.localScale= new Vector3(direction,1,1);

        newArrow.GetComponent<HurtPlayerOnHit>().enemyHM = enemyHM;
    }
    public void OnHalfHealth()
    {
        Debug.Log("Phase 2");
        isFrenzy = true;
        enemyAnim.SetBool("Frenzy", isFrenzy);
        enemyAnim.Play(frenzyAnim);
        _bossStates = BossStates.frenzyIntro;
        timerToTransition = frenzyDuration;
        parabolaController.Speed = 20;
    }
    public void OnDamaged()
    {
        StopCoroutine(GreatArrowShot());
        StopCoroutine(ThrowSlime());
        _bossStates = BossStates.stunned;
        enemyAnim.SetBool("IsHurt", true);
    }
    public void OnRecovery()
    {
        enemyAnim.SetBool("IsHurt", false);
        timerToTransition = UnityEngine.Random.Range(minTime, maxTime);

        if (isFrenzy)
            _bossStates = BossStates.frenzyIdle;
        else
            _bossStates = BossStates.idle;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, stabRangeDetect);
    }
    private void PassPatrolVariables()
    {
        isPlayerInRange = Physics2D.OverlapCircle(transform.position, stabRangeDetect, whatCountsAsPlayer);
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
            if (IsFacingRight)
            {
                theTarget = new Vector2(Mathf.Round(thePlayer.transform.position.x - 2), groundPosition.y);
                playerTarget = new Vector2(Mathf.Round(thePlayer.transform.position.x - 2), groundPosition.y);
            }
            else
            {
                theTarget = new Vector2(Mathf.Round(thePlayer.transform.position.x + 2), groundPosition.y);
                playerTarget = new Vector2(Mathf.Round(thePlayer.transform.position.x + 2), groundPosition.y);
            }
        }
            
        Debug.Log("TargetPos: " + theTarget);
        parabolaPoints[0].transform.position = transform.position;
        parabolaPoints[1].transform.position = new Vector2((transform.position.x+theTarget.x)/2, groundPosition.y + jumpStrength);
        parabolaPoints[2].transform.position = theTarget;
    }
    private void GetPlayerLocation()
    {
        if (thePlayer != null)
        {
            playerTarget = new Vector2(thePlayer.transform.position.x, groundPosition.y+1);
        }
            
        Debug.Log("playerPos: " + playerTarget);
        parabolaPoints[0].transform.position = transform.position;
        parabolaPoints[1].transform.position = new Vector2((transform.position.x+playerTarget.x)/2, groundPosition.y + jumpStrength);
        parabolaPoints[2].transform.position = playerTarget;
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
    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
    }

}
