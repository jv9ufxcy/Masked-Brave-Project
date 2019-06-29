using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class Player : MonoBehaviour
{
    public PlayerState _state;
    public GlobalVars _globalVars;
    [Space]
    [Header("Stats")]
    //Health
    private int maxHitPoints = 3;
    private int currentHitPoints_UseProperty;

    private int maxRecoveryPoints = 15;
    [SerializeField] private int currentRecoveryPoints_UseProperty = 15;
    //Meter and Ammo
    private int maxSpecialEnergyMeter = 30;
    [SerializeField] private int currentSpecialEnergyMeter_UseProperty;

    [SerializeField] private int strikeCost = 5, launcherCost = 3, shootCost = 2;

    private int maxNumberOfBullets = 3;
    private int currentNumberOfBullets_UseProperty;
    private int numberOfDashes_UseProperty = 1;
    private int minimumNumberOfDashes = 1;

    [Space]
    [Header("Movement")]

    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] float jumpStrength = 12f;
    [SerializeField] private float fallMultiplier=4.5f;
    [SerializeField] private float lowJumpMultiplier=6f;
    [SerializeField] private bool isOnGround;
    private float defaultGravityScale;
    private bool shouldJump;
    private bool hasLanded=false;
    private bool shouldMove;
    private Vector2 jumpForce;

    [Space]
    [Header("Charged Jump")]

    [SerializeField]
    private float jumpPressure;
    [SerializeField] private float minJumpPressure = 6f;
    [SerializeField] private float maxJumpPressure = 24f;
    [SerializeField] private float jumpChargeMultiplier = 10f;
    private bool shouldChargeJump;
    private bool hasReleasedJump=false;
    private bool hasJumpChargingStarted = false;
    private bool hasReachedMaxJump = false;
    
    [Space]
    [Header("Dashing")]
    private float dashTimer;
    [SerializeField] private float maxDashTime = 0.2f;
    [SerializeField] private float airDashTime = 0.3f;
    [SerializeField] private float groundDashTime = 0.15f;
    [SerializeField] private float dashSpeed = 24f;
    [SerializeField] private string dashAnimation = "Dash";
    [SerializeField] private int dashCounter;
    [SerializeField] private int maxDashCounter=3;
    private bool isDashKeyDown = false;
    private bool hasDashed;
    private bool isDashing;
    private float savedGravity_UseProperty;

    [SerializeField]
    private float fadeTime = 0.5f, aIInterval = 0.05f;
    [SerializeField]
    private Transform afterImageParent;
    [SerializeField]
    private Color trailColor = new Vector4(50, 50, 50, 0.2f), fadeColor;

    [Space]
    [Header("Detect Bounds")]

    [SerializeField] Transform groundDetectPoint;
    [SerializeField] float groundDetectRadius = 0.25f;
    [SerializeField] LayerMask whatCountsAsGround, whatCountsAsWall;

    [Space]
    [Header("Damage and Invul")]

    private bool isInvulnerable_UseProperty = false;
    [SerializeField] private float damageCooldownInSeconds = 2f;
    public float knockbackDuration, knockbackForce, maxKnockbackDuration;
    public bool knockbackFromRight;
    [SerializeField] private GameObject deathParticle;

    [Space]
    [Header("Wall Jump")]

    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallSpeed = -1.7f;
    [SerializeField] float wallJumpDistance = 8f;
    [SerializeField] float wallJumpHeight = 16f;
    [SerializeField] float wallJumpMaxTimer = 2f;
    [SerializeField] private float wallJumpTimer;
    private bool isAtPeakWallJumpHeight;
    private bool canWallSlide=true;
    private bool isJumpKeyDown;
    private bool isOnWall;
    private bool isWallSliding;
    private bool shouldWallJump;
    private Vector2 jumpLeftForce;
    private Vector2 jumpRightForce;
    private bool isAtPeakJumpHeight;

    #region SwordmasterAttackingVariables
    [Space]
    [Header("Swordmaster Attaacking Stats")]

    private bool isAttackKeyDown;
    private bool isAttackKeyUp;
    private bool isSecondAttackKeyDown;
    private bool isThirdAttackKeyDown;
    private bool isSpecialKeyDown;
    private bool isTransformKeyDown;
    private bool isLauncherKeyDown;
    private bool hasAttacked;

    [SerializeField] private float attackTimer=0f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float comboA1Cooldown = 0.39f;
    [SerializeField] private float comboA2Cooldown = 0.65f;
    [SerializeField] private float specialAttackCooldown = 0.8f;
    [SerializeField] private float shootingCooldown = 0.2f;
    [SerializeField] private Vector2 braveSlamSpeed = new Vector2(0f,24f);

    [SerializeField] private int energyMeterCost = 5;
    [SerializeField] float jumpStrengthBraveStrike = 5f;
    [SerializeField] private float jumpDistBraveStrike = 5f;
    private Vector2 jumpForceBraveStrike;


    //DashSlash
    [SerializeField] private float UpVerticalDSSpeed = 18f;
    [SerializeField] private float DownVerticalDSSpeed = 12f;
    [SerializeField] private float horizontalDashSlashSpeed = 20f;
    [SerializeField] private float dashSlashMaxTime = 0.3f;
    private bool anySlashReady;
    private bool upSlashReady;
    private bool downSlashReady;
    #endregion
    #region TricksterShootingVariables
    [Space]
    [Header("Trickster Shooting Stats")]

    [SerializeField] private Vector2 bulletOffset = new Vector2(0, 0.5f);
    [SerializeField] private Vector2 bulletVelocity = new Vector2 (20f,0), busterVelocity = new Vector2 (30f,0), critBusterVelocity = new Vector2(35f, 0);
    [SerializeField] private float fireRate = 3.3f;
    private float timeToNextFire = 0f;
    [SerializeField] private GameObject bulletPrefab, busterPrefab, criticalBusterPrefab;
    [SerializeField] private Transform bulletMuzzle;

    [Space]
    [Header("Charged Buster")]

    [SerializeField]
    private float shotPressure;
    [SerializeField] private float minShotPressure = 6f;
    [SerializeField] private float maxShotPressure = 24f;
    [SerializeField] private float shotChargeMultiplier = 10f;
    private bool shouldChargeBuster;
    private bool hasReleasedShot = false;
    private bool hasShotChargingStarted = false;
    private bool hasReachedMaxShotCharge = false;
    private bool firstCharge, secondCharge;
    private Color c;

    #endregion
    [Space]
    [Header("Audio")]

    private AudioManager audioManager;
    [SerializeField] private string deathSound="Death", jumpSound="Jump", takeDamageSound="PlayerTakeDamage", wallStickSound="WallStick", 
        henshinSound="Henshin", recoveryPickupSound="PickupRecovery", healthPickupSound="PickupHitPoint", ammoRecoverySound="PickupBullet", 
        meterRecoverySound = "PickupEnergy", attackingSound="SwordSlash", comboA2Sound="SwordSlam", riderPunchSound="RiderPunch", 
        bulletSound="Shoot", busterSound = "Buster", critBusterSound = "CriticalBuster";

    [Space]
    [Header("Particles")]
    public ParticleSystem chargeJumpParticle;
    public ParticleSystem chargeJumpMaxColor;
    public ParticleSystem dashParticle;
    public ParticleSystem landingParticles;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;
    [Header("ParticleGroups")]
    public Transform gunChargeParticles;
    public Transform jumpChargeParticles;
    public Transform flashParticles;
    public Color[] turboColors;
    public List<ParticleSystem> primaryGunParticles = new List<ParticleSystem>();
    public List<ParticleSystem> primaryJumpParticles = new List<ParticleSystem>();
    public List<ParticleSystem> secondaryParticles = new List<ParticleSystem>();


    //bools
    private static Player instance;
    private Rigidbody2D myRB;
    private Animator currentAnim;
    [SerializeField] private SpriteRenderer currentSpriteRenderer_UseProperty;
    private float horizontalInput, verticalInput;
    private Color defaultColor;
    private bool isSwordmaster=true;
    bool isFacingRight_UseProperty = true;
    private bool canAimRight_UseProperty = true;
    private bool coroutineStarted;

    [SerializeField] private RespawnPlayer playerRespawn;
    [SerializeField] private PlayerAfterImage playerAfterImage;

    #region ButtonNames
    //public PlayerControllerManager controllerManager;
    public enum ControllerList { keyboard, xbox, ps4 }
    public ControllerList myController;
    //We're gonna set these names based on what controller is connected
    public string horizontalAxisName;
    public string altHorizontalAxisName;
    public string verticalAxisName;
    public string altVerticalAxisName;
    private bool bulletCoroutineHasStarted;

    public string topFaceButtonName { get; private set; }
    public string bottomFaceButtonName { get; private set; }
    public string leftFaceButtonName { get; private set; }
    public string rightFaceButtonName { get; private set; }
    public string leftBumperName { get; private set; }
    public string rightBumperName { get; private set; }
    public string leftTriggerName { get; private set; }
    public string rightTriggerName { get; private set; }
    public string startButtonName { get; private set; }
    public string selectButtonName { get; private set; }
    #endregion

    #region Properties
    public bool ShouldAct
    {
        get { return shouldMove; }
    }
    public bool PlayerIsOnGround
    {
        get { return isOnGround; }
    }

    public bool IsInvulnerable
    {
        get { return isInvulnerable_UseProperty; }
        set
        {
            isInvulnerable_UseProperty = value;
            if (isInvulnerable_UseProperty)
                StartCoroutine(BlinkWhileInvulnerableCoroutine());
        }
    }
    public int CurrentHitPoints
    {
        get
        {
            return currentHitPoints_UseProperty;
        }
        private set
        {
            currentHitPoints_UseProperty = value;

        }
    }

    public int CurrentSpecialEnergyMeter
    {
        get
        {
            return currentSpecialEnergyMeter_UseProperty;
        }
        private set
        {
            currentSpecialEnergyMeter_UseProperty = value;
            if (currentSpecialEnergyMeter_UseProperty < 0)
                currentSpecialEnergyMeter_UseProperty = 0;
            if (currentSpecialEnergyMeter_UseProperty > maxSpecialEnergyMeter)
                currentSpecialEnergyMeter_UseProperty = maxSpecialEnergyMeter;
        }
    }

    public float CurrentEnergyMeterAsPercentage()
    {
        return CurrentSpecialEnergyMeter / (float)maxSpecialEnergyMeter;
    }

    public int RecoveryPoints
    {
        get
        {
            return currentRecoveryPoints_UseProperty;
        }
        private set
        {
            currentRecoveryPoints_UseProperty = value;
            if (currentRecoveryPoints_UseProperty < 0)
                currentRecoveryPoints_UseProperty = 0;
            if (currentRecoveryPoints_UseProperty > maxRecoveryPoints)
                currentRecoveryPoints_UseProperty = maxRecoveryPoints;
        }
    }
    public int CurrentNumberOfBullets
    {
        get
        {
            return currentNumberOfBullets_UseProperty;
        }
        private set
        {
            currentNumberOfBullets_UseProperty = value;
            if (currentNumberOfBullets_UseProperty < 0)
                currentNumberOfBullets_UseProperty = 0;
            if (currentNumberOfBullets_UseProperty > maxNumberOfBullets)
                currentNumberOfBullets_UseProperty = maxNumberOfBullets;
        }
    }

    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<Player>();
            }
            return instance;
        }
    }

    public SpriteRenderer CurrentSpriteRenderer
    {
        get
        {
            return currentSpriteRenderer_UseProperty;
        }

        set
        {
            currentSpriteRenderer_UseProperty = value;
        }
    }

    public bool IsFacingRight
    {
        get
        {
            return isFacingRight_UseProperty;
        }

        set
        {
            isFacingRight_UseProperty = value;
        }
    }

    public bool CanAimRight
    {
        get
        {
            return canAimRight_UseProperty;
        }

        set
        {
            canAimRight_UseProperty = value;
        }
    }

    public int NumberOfDashes
    {
        get
        {
            return numberOfDashes_UseProperty;
        }

        set
        {
            numberOfDashes_UseProperty = value;
            if (numberOfDashes_UseProperty < 0)
                numberOfDashes_UseProperty = 0;
        }
    }
    public float SavedGravity
    {
        get
        {
            return savedGravity_UseProperty;
        }
        set
        {
            savedGravity_UseProperty = myRB.gravityScale;
        }
    }

    public bool IsSwordmaster
    {
        get { return isSwordmaster; }
        private set { isSwordmaster = value; }
    }


    #endregion
    void Start()
    {
        Initialize();

    }

    private void Initialize()
    {
        CheckWhichControllersAreConnected();
        _state = PlayerState.STATE_IDLE_SM;
        //GetComponents
        CurrentSpriteRenderer = GetComponent<SpriteRenderer>();
        myRB = GetComponent<Rigidbody2D>();
        currentAnim = GetComponent<Animator>();
        playerRespawn = GetComponent<RespawnPlayer>();
        _globalVars = GameObject.FindGameObjectWithTag("GV").GetComponent<GlobalVars>();
        playerAfterImage = GetComponent<PlayerAfterImage>();
        //defaults
        RecoveryPoints = maxRecoveryPoints;
        currentSpecialEnergyMeter_UseProperty = maxSpecialEnergyMeter;
        defaultColor = CurrentSpriteRenderer.color;
        jumpForce = new Vector2(0, jumpStrength);
        jumpLeftForce = new Vector2(-wallJumpDistance, wallJumpHeight);
        jumpRightForce = new Vector2(wallJumpDistance, wallJumpHeight);
        jumpForceBraveStrike = new Vector2(jumpDistBraveStrike, jumpStrengthBraveStrike);
        defaultGravityScale = myRB.gravityScale;
        currentAnim.SetBool("IsSwordmaster", isSwordmaster);

        for (int i = 0; i < gunChargeParticles.GetChild(0).childCount; i++)
        {
            primaryGunParticles.Add(gunChargeParticles.GetChild(0).GetChild(i).GetComponent<ParticleSystem>());
        }

        //for (int i = 0; i < gunChargeParticles.GetChild(1).childCount; i++)
        //{
        //    primaryGunParticles.Add(gunChargeParticles.GetChild(1).GetChild(i).GetComponent<ParticleSystem>());
        //}

        foreach (ParticleSystem p in flashParticles.GetComponentsInChildren<ParticleSystem>())
        {
            secondaryParticles.Add(p);
        }

        for (int i = 0; i < jumpChargeParticles.GetChild(0).childCount; i++)
        {
            primaryJumpParticles.Add(jumpChargeParticles.GetChild(0).GetChild(i).GetComponent<ParticleSystem>());
        }

        //for (int i = 0; i < jumpChargeParticles.GetChild(1).childCount; i++)
        //{
        //    primaryJumpParticles.Add(jumpChargeParticles.GetChild(1).GetChild(i).GetComponent<ParticleSystem>());
        //}

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    void Update()
    {
        OnDeath();
        GetMovementInput();
        GetJumpInput();
        GetDashInput();
        GetTransformInput();
        GetAttackInput();
    }
    private void FixedUpdate()
    {
        HandleInput();
        //Move();
        //Jump();
        //BetterJump();
        UpdateIsOnGround();
        UpdateIsOnWall();
        UpdateIsTargetReady();
       // HandleDash();
        PassVSpeed();
        if (!isOnGround) return;
    }


    private void HandleInput()
    {
        switch (_state)
        {
            //Swordmaster Style
            #region SwordmasterStates
            case PlayerState.STATE_IDLE_SM:
                currentAnim.SetFloat("Speed", Mathf.Abs(horizontalInput));
                myRB.velocity = new Vector2(0, myRB.velocity.y);
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_IDLE_TR;
                }
                if (shouldJump||!isOnGround)
                {
                    _state = PlayerState.STATE_JUMPING_SM;
                }
                if (horizontalInput!=0)
                {
                    _state = PlayerState.STATE_RUNNING_SM;
                }

                if (isAttackKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_FIRST_ATTACK_SM;
                }
                if (CurrentSpecialEnergyMeter >= strikeCost && isSpecialKeyDown && verticalInput == 0)
                {
                    attackTimer = specialAttackCooldown;
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_STRIKE_SM;
                }
                if (isLauncherKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_BRAVE_LAUNCHER_SM;
                }
                if (isDashKeyDown)
                {
                    //Right
                    if (CanAimRight)
                    {
                        myRB.velocity = new Vector2(dashSpeed, 0);
                        audioManager.PlaySound("Dash");
                        currentAnim.SetTrigger(dashAnimation);
                    }
                    //Left    
                    else if (!CanAimRight)
                    {
                        myRB.velocity = new Vector2(-dashSpeed, 0);
                        audioManager.PlaySound("Dash");
                        currentAnim.SetTrigger(dashAnimation);
                    }
                    NumberOfDashes--;
                    _state = PlayerState.STATE_GROUND_DASHING_SM;
                }
                break;
            case PlayerState.STATE_RUNNING_SM:
                Move();
                if (horizontalInput == 0)
                {
                    _state = PlayerState.STATE_IDLE_SM;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_RUNNING_TR;
                }
                if (shouldJump == true || !isOnGround)
                {
                    _state = PlayerState.STATE_JUMPING_SM;
                }
                if (isAttackKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_FIRST_ATTACK_SM;
                }
                if (isLauncherKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_BRAVE_LAUNCHER_SM;
                }
                if (CurrentSpecialEnergyMeter >= strikeCost && isSpecialKeyDown && verticalInput == 0)
                {
                    attackTimer = specialAttackCooldown;
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_STRIKE_SM;
                }
                if (isDashKeyDown)
                {
                    //Right
                    if (CanAimRight)
                    {
                        myRB.velocity = new Vector2(dashSpeed, 0);
                        audioManager.PlaySound("Dash");
                        currentAnim.SetTrigger(dashAnimation);
                    }
                    //Left    
                    else if (!CanAimRight)
                    {
                        myRB.velocity = new Vector2(-dashSpeed, 0);
                        audioManager.PlaySound("Dash");
                        currentAnim.SetTrigger(dashAnimation);
                    }
                    NumberOfDashes--;
                    _state = PlayerState.STATE_GROUND_DASHING_SM;
                }
                break;
            case PlayerState.STATE_JUMPING_SM:
                Jump();
                BetterJump();
                Move();
                canWallSlide = true;
                if (isOnGround)
                {
                    if (horizontalInput != 0)
                    {
                        _state = PlayerState.STATE_RUNNING_SM;
                    }
                    if (horizontalInput == 0)
                    {
                        _state = PlayerState.STATE_IDLE_SM;
                    }
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_JUMPING_TR;
                }
                if (isOnWall&&isJumpKeyDown)
                    _state = PlayerState.STATE_WALLJUMPING_SM;

                if (isWallSliding)
                    _state = PlayerState.STATE_WALLSLIDING_SM;

                if (CurrentSpecialEnergyMeter >= strikeCost && isSpecialKeyDown && verticalInput != -1 && !anySlashReady)
                {
                    hasAttacked = false;
                    attackTimer = specialAttackCooldown;
                    _state = PlayerState.STATE_BRAVE_STRIKE_SM;
                }
                else if (isLauncherKeyDown)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_SM;
                }
                if (upSlashReady)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_UP_KICK_READY_SM;
                }
                if (downSlashReady)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_DOWN_KICK_READY_SM;
                }
                if (isAttackKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_JUMPING_ATTACK_SM;
                }
                
                break;
            case PlayerState.STATE_WALLSLIDING_SM:

                WallSliding();

                if (isJumpKeyDown)
                {
                    wallJumpTimer = wallJumpMaxTimer;
                    _state = PlayerState.STATE_WALLJUMPING_SM;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_WALLSLIDING_TR;
                }
                if (!isWallSliding)
                    _state = PlayerState.STATE_JUMPING_SM;

                break;
            case PlayerState.STATE_WALLJUMPING_SM:
                WallJump();
                BetterWallJump();
                
                if (wallJumpTimer > 0)
                {
                    wallJumpTimer -= Time.fixedDeltaTime;

                    if (CurrentSpecialEnergyMeter >= strikeCost && isSpecialKeyDown)
                    {
                        wallJumpTimer = 0;
                        attackTimer = specialAttackCooldown;
                        hasAttacked = false;
                        _state = PlayerState.STATE_BRAVE_STRIKE_SM;
                    }
                    if (upSlashReady)
                    {
                        wallJumpTimer = 0;
                        hasAttacked = false;
                        _state = PlayerState.STATE_UP_KICK_READY_SM;
                    }
                    if (downSlashReady)
                    {
                        wallJumpTimer = 0;
                        hasAttacked = false;
                        _state = PlayerState.STATE_DOWN_KICK_READY_SM;
                    }
                    if (isAttackKeyDown && !anySlashReady)
                    {
                        wallJumpTimer = 0;
                        hasAttacked = false;
                        attackTimer = attackCooldown;
                        _state = PlayerState.STATE_JUMPING_ATTACK_SM;
                    }
                    else if (isLauncherKeyDown)
                    {
                        hasAttacked = false;
                        _state = PlayerState.STATE_BRAVE_SLAM_SM;
                    }

                }
                else if (wallJumpTimer <= 0)
                {
                    wallJumpTimer = 0;
                    if (isWallSliding)
                    {
                        _state = PlayerState.STATE_WALLSLIDING_SM;
                    }
                    else
                        _state = PlayerState.STATE_JUMPING_SM;
                }

                break;
            case PlayerState.STATE_DAMAGED_SM:
                Move();
                attackTimer = 0;
                if (knockbackDuration <= 0)
                {
                    knockbackDuration = 0;
                    ReturnToIdleState();
                }
                break;
            case PlayerState.STATE_GROUND_DASHING_SM:
                myRB.gravityScale = 0;
                dashTimer += Time.fixedDeltaTime;
                if (dashTimer >= maxDashTime)
                {
                    dashTimer = maxDashTime;
                    _state = PlayerState.STATE_GROUND_DASHING_CD_SM;
                }
                if (shouldJump == true || !isOnGround)
                {
                    dashTimer = 0;
                    _state = PlayerState.STATE_JUMPING_SM;
                }
                break;
            case PlayerState.STATE_GROUND_DASHING_CD_SM:
                dashTimer -= Time.fixedDeltaTime;
                myRB.gravityScale = SavedGravity;
                if (dashTimer <= 0)
                {
                    dashTimer = 0;
                    _state = PlayerState.STATE_IDLE_SM;
                }
                if (shouldJump == true || !isOnGround)
                {
                    dashTimer = 0;
                    _state = PlayerState.STATE_JUMPING_SM;
                }
                break;
            #region AttackingStates
            case PlayerState.STATE_FIRST_ATTACK_SM:

                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    myRB.velocity = new Vector2(myRB.velocity.x / 2, myRB.velocity.y);

                    if (!hasAttacked)
                    {
                        currentAnim.SetTrigger("Attack");
                        audioManager.PlaySound(attackingSound);
                        hasAttacked = true;
                    }
                    if (isAttackKeyDown)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = comboA1Cooldown;
                        _state = PlayerState.STATE_SECOND_ATTACK_SM;
                    }
                    else if (CurrentSpecialEnergyMeter >= strikeCost && isSpecialKeyDown && verticalInput == 0)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = specialAttackCooldown;
                        _state = PlayerState.STATE_BRAVE_STRIKE_SM;
                    }
                    else if (isLauncherKeyDown)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = attackCooldown;
                        _state = PlayerState.STATE_BRAVE_LAUNCHER_SM;
                    }

                    if (shouldJump == true || !isOnGround)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_JUMPING_SM;
                    }
                }
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_SECOND_ATTACK_SM:

                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    myRB.velocity = new Vector2(myRB.velocity.x / 2, myRB.velocity.y);


                    if (!hasAttacked)
                    {
                        currentAnim.SetTrigger("AttackComboA1");
                        audioManager.PlaySound(attackingSound);
                        hasAttacked = true;
                    }
                    if (isAttackKeyDown)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = comboA2Cooldown;
                        _state = PlayerState.STATE_THIRD_ATTACK_SM;
                    }
                    else if (CurrentSpecialEnergyMeter >= strikeCost && isSpecialKeyDown && verticalInput == 0)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = specialAttackCooldown;
                        _state = PlayerState.STATE_BRAVE_STRIKE_SM;
                    }
                    else if (isLauncherKeyDown)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = attackCooldown;
                        _state = PlayerState.STATE_BRAVE_LAUNCHER_SM;
                    }

                    if (shouldJump == true || !isOnGround)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_JUMPING_SM;
                    }
                }
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_THIRD_ATTACK_SM:
                    
                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    isAttackKeyDown = false;

                    if (!hasAttacked)
                    {
                        currentAnim.SetTrigger("AttackComboA2");
                        audioManager.PlaySound(comboA2Sound);
                        hasAttacked = true;
                    }
                    else if (CurrentSpecialEnergyMeter >= strikeCost && isSpecialKeyDown && verticalInput == 0)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = specialAttackCooldown;
                        _state = PlayerState.STATE_BRAVE_STRIKE_SM;
                    }

                }

                if (shouldJump == true || !isOnGround)
                {
                    attackTimer = 0;
                    _state = PlayerState.STATE_JUMPING_SM;
                }

                    ReturnToIdleState();
                break;
            case PlayerState.STATE_JUMPING_ATTACK_SM:
                Move();
                BetterJump();
                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    isAttackKeyDown = false;
                    if (isOnGround)
                    {
                        myRB.velocity = new Vector2(0, myRB.velocity.y);
                    }
                    if (!hasAttacked)
                    {
                        currentAnim.SetTrigger("AirAttack");
                        audioManager.PlaySound(attackingSound);
                        hasAttacked = true;
                    }
                    if (CurrentSpecialEnergyMeter >= strikeCost && isSpecialKeyDown && verticalInput != -1)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = specialAttackCooldown;
                        _state = PlayerState.STATE_BRAVE_STRIKE_SM;
                    }
                    else if (isLauncherKeyDown && !isOnGround)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = 0;
                        //attackTimer = attackCooldown;
                        _state = PlayerState.STATE_BRAVE_SLAM_SM;
                    }
                }

                    BetterJump();
                    ReturnToIdleState();

                break;
            #region BraveAttacks
            case PlayerState.STATE_BRAVE_STRIKE_SM:
                
                BetterJump();
                
                if (attackTimer >= 0)
                {
                    isSpecialKeyDown = false;
                    attackTimer -= Time.deltaTime;
                    if (!hasAttacked)
                    {
                        SpendMeter(strikeCost);
                        myRB.velocity = new Vector2(0, 0);
                        currentAnim.SetTrigger("RiderPunch");
                        audioManager.PlaySound(riderPunchSound);
                        hasAttacked = true;
                        if (CanAimRight)
                        {
                            myRB.AddForce(jumpForceBraveStrike, ForceMode2D.Impulse);
                            myRB.AddForce(new Vector2(jumpDistBraveStrike, jumpDistBraveStrike), ForceMode2D.Impulse);
                        }
                        else
                        {
                            myRB.AddForce(new Vector2(-jumpDistBraveStrike, jumpStrengthBraveStrike), ForceMode2D.Impulse);
                            myRB.AddForce(new Vector2(-jumpDistBraveStrike, jumpDistBraveStrike), ForceMode2D.Impulse);
                        }
                    }

                }
                if (attackTimer <= 0)
                {
                    attackTimer = 0;
                    ReturnToIdleState();
                }
                break;

            case PlayerState.STATE_BRAVE_LAUNCHER_SM:
                
                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    myRB.velocity = new Vector2(myRB.velocity.x / 2, myRB.velocity.y);

                    if (!hasAttacked)
                    {
                        SpendMeter(launcherCost);
                        currentAnim.SetTrigger("Launcher");
                        audioManager.PlaySound(attackingSound);
                        hasAttacked = true;
                    }
                }
                if (shouldJump == true || !isOnGround)
                {
                    attackTimer = 0;
                    _state = PlayerState.STATE_JUMPING_SM;
                }
                ReturnToIdleState();
                break;
            case PlayerState.STATE_BRAVE_SLAM_SM:
                Move();
                if (!isOnGround)
                {
                    //attackTimer -= Time.fixedDeltaTime;
                    myRB.AddForce(-braveSlamSpeed, ForceMode2D.Impulse);

                    if (!hasAttacked)
                    {
                        SpendMeter(launcherCost);
                        currentAnim.SetTrigger("Slam");
                        audioManager.PlaySound(comboA2Sound);
                        hasAttacked = true;
                    }
                }
                else
                {
                    attackTimer = 0;
                    ReturnToIdleState();
                }
                break;
            case PlayerState.STATE_UP_KICK_READY_SM:
                Jump();
                BetterJump();
                Move();
                if (isSpecialKeyDown&&verticalInput!=-1)
                    _state = PlayerState.STATE_UP_KICK_ATTACK_SM;
                else if (isLauncherKeyDown && verticalInput == -1)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_SM;
                }

                if (isAttackKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_JUMPING_ATTACK_SM;
                }

                if (!upSlashReady&&downSlashReady)
                    _state = PlayerState.STATE_DOWN_KICK_READY_SM;

                if (!anySlashReady)
                    _state = PlayerState.STATE_JUMPING_SM;
                break;
            case PlayerState.STATE_UP_KICK_ATTACK_SM:
                myRB.gravityScale = -1;
                myRB.velocity = new Vector2(0, 0);
                attackTimer = dashSlashMaxTime;
                if (attackTimer >= dashSlashMaxTime)
                {
                    if (CanAimRight)
                    {
                        myRB.AddForce(new Vector2(horizontalDashSlashSpeed, UpVerticalDSSpeed), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRB.AddForce(new Vector2(-horizontalDashSlashSpeed, UpVerticalDSSpeed), ForceMode2D.Impulse);
                    }

                    currentAnim.SetTrigger("UpKick");
                    audioManager.PlaySound(attackingSound);

                    _state = PlayerState.STATE_BRAVE_KICK_CD_SM;
                }
                break;
            case PlayerState.STATE_DOWN_KICK_READY_SM:
                Jump();
                BetterJump();
                Move();
                if (isSpecialKeyDown&&verticalInput!=-1)
                    _state = PlayerState.STATE_DOWN_KICK_ATTACK_SM;
                else if (isLauncherKeyDown && verticalInput == -1)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_SM;
                }

                if (isAttackKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_JUMPING_ATTACK_SM;
                }

                if (!downSlashReady&&upSlashReady)
                {
                    _state = PlayerState.STATE_UP_KICK_READY_SM;
                }
                if (!anySlashReady)
                {
                    _state = PlayerState.STATE_JUMPING_SM;
                }
                break;
            case PlayerState.STATE_DOWN_KICK_ATTACK_SM:
                myRB.gravityScale = 0;
                myRB.velocity = new Vector2(0, 0);
                attackTimer = dashSlashMaxTime;
                if (attackTimer >= dashSlashMaxTime)
                {
                    if (CanAimRight)
                    {
                        myRB.AddForce(new Vector2(horizontalDashSlashSpeed, -DownVerticalDSSpeed), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRB.AddForce(new Vector2(-horizontalDashSlashSpeed, -DownVerticalDSSpeed), ForceMode2D.Impulse);
                    }

                    currentAnim.SetTrigger("DownKick");
                    audioManager.PlaySound(attackingSound);

                    _state = PlayerState.STATE_BRAVE_KICK_CD_SM;
                }
                break;
            case PlayerState.STATE_BRAVE_KICK_CD_SM:
                isAttackKeyDown = false;
                upSlashReady = false;
                downSlashReady = false;
                attackTimer -= Time.fixedDeltaTime;
                myRB.gravityScale = SavedGravity;
                ReturnToIdleState();
                break;
            #endregion
            #endregion
            #endregion

            #region TricksterStates
            case PlayerState.STATE_IDLE_TR:
                HandleCharging();
                if (shouldJump||!isOnGround)
                {
                    _state = PlayerState.STATE_JUMPING_TR;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_IDLE_SM;
                }
                if (horizontalInput != 0)
                {
                    _state = PlayerState.STATE_RUNNING_TR;
                }

                if (isAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_IDLE_TR;
                }

                if (isDashKeyDown&&!hasDashed)
                {
                    audioManager.PlaySound("Dash");
                    currentAnim.SetTrigger(dashAnimation);
                    _state = PlayerState.STATE_DASHING_TR;
                }
                break;
            case PlayerState.STATE_RUNNING_TR:
                Move();
                HandleCharging();
                if (horizontalInput == 0)
                {
                    _state = PlayerState.STATE_IDLE_TR;
                }
                if (shouldJump || !isOnGround)
                {
                    _state = PlayerState.STATE_JUMPING_TR;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_RUNNING_SM;
                }
                if (isAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_RUNNING_TR;
                }
                if (isDashKeyDown && !hasDashed)
                {
                    audioManager.PlaySound("Dash");
                    currentAnim.SetTrigger(dashAnimation);
                    _state = PlayerState.STATE_DASHING_TR;
                }
                break;
            case PlayerState.STATE_JUMPING_TR:
                HandleCharging();
                Jump();
                BetterJump();
                Move();
                hasReleasedJump = false;
                canWallSlide = true;
                if (isOnGround)
                {
                    if (horizontalInput != 0)
                    {
                        _state = PlayerState.STATE_RUNNING_TR;
                    }
                    if (horizontalInput == 0)
                    {
                        _state = PlayerState.STATE_IDLE_TR;
                    }
                }

                if (isWallSliding)
                {
                    _state = PlayerState.STATE_WALLSLIDING_TR;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_JUMPING_SM;
                }
                if (isAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_JUMPING_TR;
                }
                if (isDashKeyDown && !hasDashed)
                {
                    audioManager.PlaySound("Dash");
                    currentAnim.SetTrigger(dashAnimation);
                    _state = PlayerState.STATE_DASHING_TR;
                }
                break;
            case PlayerState.STATE_WALLSLIDING_TR:
                WallSliding();

                if (isJumpKeyDown)
                {
                    wallJumpTimer = wallJumpMaxTimer;
                    _state = PlayerState.STATE_WALLJUMPING_TR;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_WALLSLIDING_SM;
                }
                if (!isWallSliding)
                    _state = PlayerState.STATE_JUMPING_TR;

                break;
            case PlayerState.STATE_WALLJUMPING_TR:
                WallJump();
                BetterWallJump();

                if (wallJumpTimer > 0)
                {
                    wallJumpTimer -= Time.fixedDeltaTime;
                }
                else if (wallJumpTimer <= 0)
                {
                    wallJumpTimer = 0;
                    if (isWallSliding)
                    {
                        _state = PlayerState.STATE_WALLSLIDING_TR;
                    }
                    else
                        _state = PlayerState.STATE_JUMPING_TR;
                }
                _state = PlayerState.STATE_JUMPING_TR;

                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_WALLSLIDING_TR;
                }
                break;
            case PlayerState.STATE_DAMAGED_TR:
                Move();
                if (knockbackDuration <= 0)
                {
                    knockbackDuration = 0;
                    _state = PlayerState.STATE_IDLE_TR;
                }
                break;
            case PlayerState.STATE_DASHING_TR:
                HandleCharging();
                NumberOfDashes--;
                if (!coroutineStarted)
                    Dash(horizontalInput, verticalInput);
                break;
            #region Trickster Shooting States
            case PlayerState.STATE_SHOOTING_IDLE_TR:
                    FireBullet();
                HandleCharging();
                if (attackTimer > 0)
                {
                    attackTimer -= Time.fixedDeltaTime;

                    if (isAttackKeyDown && horizontalInput != 0)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_RUNNING_TR;
                    }
                    else if (isAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_TR;
                    }
                    else if (!isAttackKeyDown&&horizontalInput!=0)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_RUNNING_TR;
                    }
                    else if (!isAttackKeyDown&&shouldJump)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_JUMPING_TR;
                    }
                }
                else if (attackTimer <= 0)
                {
                    attackTimer = 0;
                        _state = PlayerState.STATE_IDLE_TR;
                }

                break;
            case PlayerState.STATE_SHOOTING_RUNNING_TR:
                Move();
                FireBullet();
                HandleCharging();
                if (attackTimer > 0)
                {
                    attackTimer -= Time.fixedDeltaTime;

                    if (isAttackKeyDown && horizontalInput == 0)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_IDLE_TR;
                    }
                    else if (isAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_TR;
                    }
                    else if (!isAttackKeyDown && horizontalInput != 0)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_RUNNING_TR;
                    }
                    else if (!isAttackKeyDown && shouldJump)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_JUMPING_TR;
                    }
                }
                else if (attackTimer <= 0)
                {
                    attackTimer = 0;
                    _state = PlayerState.STATE_IDLE_TR;
                }

                break;
            case PlayerState.STATE_SHOOTING_JUMPING_TR:
                Move();
                HandleCharging();
                FireBullet();
                hasReleasedJump = true;
                if (attackTimer > 0)
                {
                    attackTimer -= Time.fixedDeltaTime;

                    if (isAttackKeyDown && isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_IDLE_TR;
                    }
                    else if (isAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_TR;
                    }
                    else if (!isAttackKeyDown && isOnGround)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_RUNNING_TR;
                    }
                    else if (isWallSliding)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_WALLSLIDING_TR;
                    }
                }
                else if (attackTimer <= 0)
                {
                    attackTimer = 0;
                    _state = PlayerState.STATE_JUMPING_TR;
                }
                break;
            case PlayerState.STATE_SHOOTING_AIR_DASHING_TR:

                break;
                #endregion
                #endregion
        }
    }


    #region Attacking Functions
    private void ReturnToIdleState()
    {
        if (attackTimer <= 0)
        {
            attackTimer = 0;
            if (horizontalInput != 0)
            {
                _state = PlayerState.STATE_RUNNING_SM;
            }
            if (horizontalInput == 0)
            {
                _state = PlayerState.STATE_IDLE_SM;
            }
        }
        upSlashReady = false;
        downSlashReady = false;
    }
    public void EnemyTargetedUpDash()
    {
        upSlashReady = true;
    }
    public void UpDashEmpty()
    {
        upSlashReady = false;
    }
    public void EnemyTargetedDownDash()
    {
        downSlashReady = true;
    }
    public void DownDashEmpty()
    {
        downSlashReady = false;
    }
    void FireBullet()
    {
        if (CurrentNumberOfBullets>=1)
        {
            shotPressure = 0;
            BusterShooting();
            
        }
        else
        {
            if (Time.time > timeToNextFire)
            {
                currentAnim.SetTrigger("Shooting");
                timeToNextFire = Time.time + 1 / fireRate;
                audioManager.PlaySound(bulletSound);
                if (CanAimRight)
                    ShootBulletRight();
                else
                    ShootBulletLeft();
            }
        }
        
    }
    private void ShootBulletRight()
    {
        GameObject newbullet = Instantiate(bulletPrefab, bulletMuzzle.position, Quaternion.identity);
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(bulletVelocity.x * transform.localScale.x, bulletVelocity.y);
    }
    private void ShootBulletLeft()
    {
        GameObject newbullet = Instantiate(bulletPrefab, bulletMuzzle.position, Quaternion.Euler(new Vector3(0, 0, -180)));
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(bulletVelocity.x * transform.localScale.x, bulletVelocity.y);
    }
    private void BusterShooting()
    {
        shotPressure = 0;
        if (Time.time > timeToNextFire)
        {
            timeToNextFire = Time.time + 1 / fireRate;
            currentAnim.SetTrigger("Shooting");
            SpendAmmo(1);
            audioManager.PlaySound(busterSound);
            if (CanAimRight)
                ShootBusterRight();
            else
                ShootBusterLeft();
        }
    }
    private void ShootBusterRight()
    {
        GameObject newbullet = Instantiate(busterPrefab, bulletMuzzle.position, Quaternion.identity);
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(busterVelocity.x * transform.localScale.x, busterVelocity.y);
    }
    private void ShootBusterLeft()
    {
        GameObject newbullet = Instantiate(busterPrefab, bulletMuzzle.position, Quaternion.Euler(new Vector3(0, 0, -180)));
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(busterVelocity.x * transform.localScale.x, busterVelocity.y);
    }
    private void CriticalBusterShooting()
    {
        if (Time.time > timeToNextFire)
        {
            timeToNextFire = Time.time + 1 / fireRate;
            currentAnim.SetTrigger("Shooting");
            audioManager.PlaySound(critBusterSound);
            Screenshake();
            if (CanAimRight)
                ShootCriticalBusterRight();
            else
                ShootCriticalBusterLeft();
        }
    }
    private void ShootCriticalBusterRight()
    {
        GameObject newbullet = Instantiate(criticalBusterPrefab, bulletMuzzle.position, Quaternion.identity);
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(critBusterVelocity.x * transform.localScale.x, critBusterVelocity.y);
        
    }
    private void ShootCriticalBusterLeft()
    {
        GameObject newbullet = Instantiate(criticalBusterPrefab, bulletMuzzle.position, Quaternion.Euler(new Vector3(0, 0, -180)));
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(critBusterVelocity.x * transform.localScale.x, critBusterVelocity.y);
    }
    private void UpdateIsTargetReady()
    {
        anySlashReady = (upSlashReady || downSlashReady);
        if (isOnGround)
        {
            upSlashReady = false;
            downSlashReady = false;
            anySlashReady = false;
        }
    }
#endregion
    #region Get Input
    private void GetMovementInput()
    {
        //if one controller button is going, switch input to that one
        if (Mathf.Abs(Input.GetAxis(horizontalAxisName)) < Mathf.Abs(Input.GetAxis(altHorizontalAxisName)))
            horizontalInput = Input.GetAxis(altHorizontalAxisName);
        else
            horizontalInput = Input.GetAxis(horizontalAxisName);

        if (Mathf.Abs(Input.GetAxis(verticalAxisName)) < Mathf.Abs(Input.GetAxis(altVerticalAxisName)))
            verticalInput = Input.GetAxis(altVerticalAxisName);
        else
            verticalInput = Input.GetAxis(verticalAxisName);
    }
    private void GetJumpInput()
    {
        if (Input.GetButtonDown(bottomFaceButtonName))
        {
            isJumpKeyDown = true;
        }
        else
            isJumpKeyDown = false;
        if (Input.GetButtonDown(bottomFaceButtonName) && isOnGround)
        {
            shouldJump = true;
        }
        if (Input.GetButtonDown(bottomFaceButtonName) && isOnWall && isAtPeakJumpHeight)
        {
            shouldWallJump = true;
        }

        shouldChargeJump = Input.GetButton(bottomFaceButtonName);
        
    }
    private void GetDashInput()
    {
        isDashKeyDown = (Input.GetButtonDown(rightFaceButtonName));
        //if (Input.GetButtonDown(rightFaceButtonName)||Input.GetButtonDown(leftTriggerName) && NumberOfDashes > 0)
        //    isDashKeyDown = true;
        //else
        //    isDashKeyDown = false;
    }
    private void GetAttackInput()
    {
        if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
            isAttackKeyDown = true;
        else
            isAttackKeyDown = false;

        if (Input.GetButtonDown(topFaceButtonName) || Input.GetButtonDown(rightTriggerName)/* && CurrentSpecialEnergyMeter >= energyMeterCost*/)
            isSpecialKeyDown = true;
        else
            isSpecialKeyDown = false;

        if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName) && _state == PlayerState.STATE_FIRST_ATTACK_SM && isOnGround)
        {
            isSecondAttackKeyDown = true;
        }
        else
            isSecondAttackKeyDown = false;
        if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName) && _state == PlayerState.STATE_SECOND_ATTACK_SM && isOnGround)
        {
            isThirdAttackKeyDown = true;
        }
        else
            isThirdAttackKeyDown = false;

        if (Input.GetButtonDown(topFaceButtonName) && verticalInput == -1&&CurrentSpecialEnergyMeter>=launcherCost)
            isLauncherKeyDown = true;
        else
            isLauncherKeyDown = false;

        //Buster
        if (Input.GetButton(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
        {
            shouldChargeBuster = true;
        }
        else
            shouldChargeBuster = false;

        if (Input.GetButtonUp(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
        {
            isAttackKeyUp = true;
        }
        else
            isAttackKeyUp = false;

    }
    private void GetTransformInput()
    {
        isTransformKeyDown = (Input.GetButtonDown(leftBumperName));
    }
    private void CheckWhichControllersAreConnected()
    {
        #region ControllerCheck

        //int joystickNumber = Input.GetJoystickNames().Length;//get how many axes are connected to our controller
        int joystickNumber = GlobalVars.controllerNumber;
        Debug.Log(joystickNumber);
        switch (joystickNumber)
        {
            case 0:/*was 19*/
                {
                    myController = ControllerList.ps4;

                    horizontalAxisName = "Ps4Horizontal";
                    altHorizontalAxisName = "altPs4Horizontal";
                    verticalAxisName = "Ps4Vertical";
                    altVerticalAxisName = "altPs4Vertical";
                    topFaceButtonName = "Ps4Triangle";
                    bottomFaceButtonName = "Ps4X";
                    leftFaceButtonName = "Ps4Square";
                    rightFaceButtonName = "Ps4O";
                    leftBumperName = "Ps4L1";
                    rightBumperName = "Ps4R1";
                    leftTriggerName = "Ps4L2";
                    rightTriggerName = "Ps4R2";
                    startButtonName = "Ps4Options";
                    selectButtonName = "Ps4Share";
                }
                break;
            case 1:/*was 33*/
                {
                    myController = ControllerList.xbox;

                    horizontalAxisName = "XboxHorizontal";
                    altHorizontalAxisName = "altXboxHorizontal";
                    verticalAxisName = "XboxVertical";
                    altVerticalAxisName = "altXboxVertical";
                    topFaceButtonName = "XboxY";
                    bottomFaceButtonName = "XboxA";
                    leftFaceButtonName = "XboxX";
                    rightFaceButtonName = "XboxB";
                    leftBumperName = "XboxLB";
                    rightBumperName = "XboxRB";
                    leftTriggerName = "XboxLT";
                    rightTriggerName = "XboxRT";
                    startButtonName = "XboxMenu";
                    selectButtonName = "XboxBack";
                }
                break;
            default:
                {
                    myController = ControllerList.keyboard;
                    horizontalAxisName = "KeyboardHorizontal";
                    altHorizontalAxisName = horizontalAxisName;//no alt button with keyboard
                    verticalAxisName = "KeyboardVertical";
                    altVerticalAxisName = verticalAxisName;
                    topFaceButtonName = "KeyboardV";
                    bottomFaceButtonName = "KeyboardZ";
                    leftFaceButtonName = "KeyboardX";
                    rightFaceButtonName = "KeyboardC";
                    leftBumperName = "KeyboardQ";
                    rightBumperName = "KeyboardE";
                    leftTriggerName = "KeyboardLeftShift";
                    rightTriggerName = "KeyboardLeftCtrl";
                    startButtonName = "KeyboardEscape";
                    selectButtonName = "KeyboardBackspace";
                }
                break;
        }
    #endregion
    }
    #endregion
    #region Movement Functions
    private void Move()
    {
        if (knockbackDuration <= 0)
        {
            shouldMove = true;
        }
        else
        {
            shouldMove = false;
            if (knockbackFromRight)
            {
                myRB.velocity = new Vector2(-knockbackForce, knockbackForce / 2);
            }
            if (!knockbackFromRight)
            {
                myRB.velocity = new Vector2(knockbackForce, knockbackForce / 2);
            }
            knockbackDuration -= Time.deltaTime;
        }

        currentAnim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        if (shouldMove)
        {
            myRB.velocity = new Vector2(horizontalInput * movementSpeed, myRB.velocity.y);
            TurnAround();
        }
    }
    private void WallSliding()
    {
        myRB.velocity = (new Vector2(myRB.velocity.x, wallSpeed));
        currentAnim.SetBool("WallSliding", isWallSliding);
        currentAnim.SetFloat("vSpeed", myRB.velocity.y);
    }

    private void WallJump()
    {
        if (shouldWallJump)
        {
            canWallSlide = false;
            myRB.gravityScale = SavedGravity;
            shouldMove = false;
            audioManager.PlaySound(jumpSound);
            if (CanAimRight)
            {
                //jump to the left
                myRB.AddForce(jumpLeftForce, ForceMode2D.Impulse);
   
                shouldWallJump = false;
            }
            if (!CanAimRight)
            {
                //jump to the right
                myRB.AddForce(jumpRightForce, ForceMode2D.Impulse);
                shouldWallJump = false;
            }
        }

    }

    private void TurnAround()
    {
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            CanAimRight = true;
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            CanAimRight = false;
        }
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;
    }
    private void Jump()
    {
        if (shouldJump)
        {
            currentAnim.SetBool("Ground", false);
            currentAnim.SetFloat("vSpeed", myRB.velocity.y);
            myRB.AddForce(jumpForce, ForceMode2D.Impulse);
            audioManager.PlaySound(jumpSound);

            isOnGround = false;
            shouldJump = false;
        }
    }
    private void HandleCharging()
    {
        ChargeJump();
        ChargedShot();
    }

    private void ChargeJump()
    {
        if (shouldChargeJump)
        {
            CheckParticles();
            if (jumpPressure < maxJumpPressure)
            {
                jumpPressure += Time.fixedDeltaTime * jumpChargeMultiplier;
                hasJumpChargingStarted = true;
            }
            else
            {
                hasJumpChargingStarted = false;
                jumpPressure = maxJumpPressure;
            }

        }
        else
        {
            if (jumpPressure > 0f && isOnGround)
            {
                ShowAfterImage();
                currentAnim.SetBool("Ground", false);
                currentAnim.SetFloat("vSpeed", myRB.velocity.y);
                hasReleasedJump = true;
                jumpPressure = jumpPressure + minJumpPressure;
                myRB.AddForce(new Vector2(0f, jumpPressure), ForceMode2D.Impulse);
                jumpPressure = 0f;
                audioManager.PlaySound(jumpSound);
                isOnGround = false;
                shouldJump = false;
                hasReachedMaxJump = false;
                chargeJumpParticle.Stop();
                chargeJumpMaxColor.Stop();
            }
            else if (!shouldChargeJump)
            {
                jumpPressure = 0;
            }
        }
    }

    
    private void CheckParticles()
    {
        if (!hasJumpChargingStarted)
        {
            chargeJumpParticle.Play();
            hasJumpChargingStarted = true;
        }

        if (jumpPressure >= maxJumpPressure)
        {
            if (!hasReachedMaxJump)
            {
                hasReachedMaxJump = true;
                chargeJumpMaxColor.Play();
            }
            
        }
    }

    private void BetterJump()
    {
        if (myRB.velocity.y < 0)
        {
            myRB.gravityScale = fallMultiplier;
            isAtPeakJumpHeight = true;
        }
        else if (myRB.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            myRB.gravityScale = lowJumpMultiplier;
            isAtPeakJumpHeight = true;
        }
        else
        {
            myRB.gravityScale = defaultGravityScale;
            isAtPeakJumpHeight = false;
        }
    }
    private void BetterWallJump()
    {
        if (myRB.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            myRB.gravityScale = lowJumpMultiplier;
            isAtPeakWallJumpHeight = true;
        }
        else
        {
            myRB.gravityScale = defaultGravityScale;
            isAtPeakWallJumpHeight = false;
        }
    }
    private void PassVSpeed()
    {
        currentAnim.SetFloat("vSpeed", myRB.velocity.y);
        currentAnim.SetFloat("hSpeed", myRB.velocity.x);
    }
    private void UpdateIsOnGround()
    {
        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, groundDetectRadius, whatCountsAsGround);
        isOnGround = groundObjects.Length > 0;

        currentAnim.SetBool("Ground", isOnGround);
        if (isOnGround)
        {
            GroundTouch();
        }
        if (myRB.velocity.y < 0)
        {
            currentAnim.SetBool("Ground", false);
            hasLanded = false;
        }
    }

    private void GroundTouch()
    {
        currentAnim.SetBool("Ground", true);
        hasDashed = false;
        isDashing = false;
        NumberOfDashes = minimumNumberOfDashes;
        if (!hasLanded)
        {
            hasLanded = true;
            landingParticles.Play();
        }
    }

    private void UpdateIsOnWall()
    {
        isOnWall = Physics2D.Linecast(transform.position, wallCheck.position, whatCountsAsWall);
        if (canWallSlide)
        {
            isWallSliding = (CanAimRight && horizontalInput > 0.1f || !CanAimRight && horizontalInput < -0.1f) && (isOnWall && !isOnGround && isAtPeakJumpHeight);
        }
        else
            isWallSliding = false;
        currentAnim.SetBool("WallSliding", isWallSliding);
    }

    private void Dash(float x, float y)
    {
        Screenshake();

        hasDashed = true;
        myRB.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);
        myRB.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        coroutineStarted = true;
        ShowAfterImage();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        dashParticle.Play();
        myRB.gravityScale = 0;
        isDashing = true;

        yield return new WaitForSeconds(airDashTime);

        dashParticle.Stop();
        myRB.gravityScale = defaultGravityScale;
        BetterJump();
        isDashing = false;
        coroutineStarted = false;
        _state = PlayerState.STATE_JUMPING_TR;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(groundDashTime);
        if (isOnGround)
            hasDashed = false;
    }
    void RigidbodyDrag(float x)
    {
        myRB.drag = x;
    }
    #endregion

    #region ChargingShots
    private void ChargedShot()
    {
        if (shouldChargeBuster)
        {
            
            ColorCharge();
            if (shotPressure < maxShotPressure)
            {
                shotPressure += Time.fixedDeltaTime * shotChargeMultiplier;

                hasJumpChargingStarted = true;
            }
            else
            {
                hasShotChargingStarted = false;
                shotPressure = maxShotPressure;
                if (CurrentNumberOfBullets < maxNumberOfBullets)
                {
                    if (!bulletCoroutineHasStarted)
                    {
                        StartCoroutine(AddBulletsEveryTimeCoroutine());
                    }
                }
            }

        }
        else if (isAttackKeyUp)
        {
            if (shotPressure >= minShotPressure && shotPressure < maxShotPressure)
            {
                BusterShooting();
                shotPressure = 0f;
            }
            else if (shotPressure >= maxShotPressure)
            {
                CriticalBusterShooting();
                shotPressure = 0f;
            }
            if (shotPressure > 0f)
            {
                hasReleasedShot = true;
                shotPressure = 0f;
                firstCharge = false; secondCharge = false;
            }
            if (!shouldChargeBuster)
            {
                foreach (ParticleSystem p in primaryGunParticles)
                {
                    p.startColor = Color.clear;
                    p.Stop();
                }
            }
        }
    }
    #endregion

    private void Henshin()
    {
        audioManager.PlaySound(henshinSound);
        isSwordmaster = !isSwordmaster;
        currentAnim.SetBool("IsSwordmaster", isSwordmaster);
        //TODO: Set box collider active false
    }

    private void OnDeath()
    {
        CurrentSpriteRenderer.color = defaultColor;
        if (RecoveryPoints <= 0)
        {
            audioManager.PlaySound(deathSound);
            Instantiate(deathParticle, transform.position, transform.rotation);
            StartCoroutine(WaitWhileRespawningCoroutine());
        }
    }
    #region PolishRegion
    public void ShowAfterImage()
    {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < afterImageParent.childCount; i++)
        {
            Transform currentGhost = afterImageParent.GetChild(i);
            s.AppendCallback(() => currentGhost.position = Player.Instance.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = CurrentSpriteRenderer.flipX);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().sprite = CurrentSpriteRenderer.sprite);
            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(trailColor, 0));
            s.AppendCallback(() => FadeSprite(currentGhost));
            s.AppendInterval(aIInterval);
        }
    }
    public void FadeSprite(Transform current)
    {
        current.GetComponent<SpriteRenderer>().material.DOKill();
        current.GetComponent<SpriteRenderer>().material.DOColor(fadeColor, fadeTime);
    }

    public void ColorCharge()
    {
        if (!firstCharge)
            c = Color.clear;

        if (shotPressure >= minShotPressure && shotPressure < maxShotPressure && !firstCharge)
        {
            StartCoroutine(BlinkWhileChargingCoroutine());
            foreach (ParticleSystem p in primaryGunParticles)
            {
                p.startColor = Color.clear;
                p.Play();
            }
            firstCharge = true;
            c = turboColors[0];

            PlayFlashParticle(c);
        }

        if (shotPressure>=maxShotPressure && !secondCharge)
        {
            secondCharge = true;
            c = turboColors[1];

            PlayFlashParticle(c);
        }
        foreach (ParticleSystem p in primaryGunParticles)
        {
            var pmain = p.main;
            pmain.startColor = c;
        }

        foreach (ParticleSystem p in secondaryParticles)
        {
            var pmain = p.main;
            pmain.startColor = c;
        }
    }
    void PlayFlashParticle(Color c)
    {
        Screenshake();

        foreach (ParticleSystem p in secondaryParticles)
        {
            var pmain = p.main;
            pmain.startColor = c;
            p.Play();
        }
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
    #endregion
    #region Damage
    public void TakeDamage(int damageToGive)
    {
        if (!IsInvulnerable)
        {
            if (isSwordmaster)
                _state = PlayerState.STATE_DAMAGED_SM;
            else
                _state = PlayerState.STATE_DAMAGED_TR;
            currentAnim.SetTrigger("Damaged");
            RecoveryPoints -= damageToGive;
            audioManager.PlaySound(takeDamageSound);
            OnDeath();
            //start damage cooldown
            StartCoroutine(DamageCooldownCoroutine());
        }
    }
    public void KillPlayer(int damageToGive)
    {
        currentAnim.SetTrigger("Damaged");
        RecoveryPoints -= damageToGive;
        audioManager.PlaySound(takeDamageSound);

        OnDeath();
    }
    #endregion
    #region CoRoutines
    private IEnumerator BlinkWhileChargingCoroutine()
    {
        Color chargingColor = turboColors[0]; ;

        float blinkInterval = .5f;

        while (shouldChargeBuster)
        {
            CurrentSpriteRenderer.color = chargingColor;
            yield return new WaitForSeconds(blinkInterval);

            CurrentSpriteRenderer.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private IEnumerator AddBulletsEveryTimeCoroutine()
    {
        bulletCoroutineHasStarted=true;
        float bulletInterval = 1f;
        while (CurrentNumberOfBullets<maxNumberOfBullets&&(shouldChargeBuster&&shotPressure>=maxShotPressure))
        {
            AddAmmo(1);
            yield return new WaitForSeconds(bulletInterval);
        }
        bulletCoroutineHasStarted = false;
    }
    private IEnumerator DamageCooldownCoroutine()
    {
        IsInvulnerable = true;
        StartCoroutine(BlinkWhileInvulnerableCoroutine());
        yield return new WaitForSeconds(damageCooldownInSeconds);
        IsInvulnerable = false;
    }

    private IEnumerator BlinkWhileInvulnerableCoroutine()
    {

        Color invulnerableColor = Color.red;

        float blinkInterval = .1f;

        while (IsInvulnerable)
        {
            CurrentSpriteRenderer.color = invulnerableColor;
            yield return new WaitForSeconds(blinkInterval);

            CurrentSpriteRenderer.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
    private IEnumerator WaitWhileRespawningCoroutine()
    {
        float respawnTimer = .1f;
        yield return new WaitForSeconds(respawnTimer);
        playerRespawn.Respawn();
        RecoveryPoints = maxRecoveryPoints;
    }
    #endregion
    #region Resources
    public void AddRecovery(int healthToRestore)
    {
        if (RecoveryPoints < maxRecoveryPoints)
        {
            audioManager.PlaySound(recoveryPickupSound);
            RecoveryPoints += healthToRestore;
        }
        if (currentRecoveryPoints_UseProperty < 0)
            currentRecoveryPoints_UseProperty = 0;
        if (currentRecoveryPoints_UseProperty > maxRecoveryPoints)
            currentRecoveryPoints_UseProperty = maxRecoveryPoints;
    }

    public void AddAmmo(int ammoToRestore)
    {
        audioManager.PlaySound(ammoRecoverySound);
        currentNumberOfBullets_UseProperty += ammoToRestore;
        if (currentNumberOfBullets_UseProperty < 0)
        {
            currentNumberOfBullets_UseProperty = 0;
        }
        if (currentNumberOfBullets_UseProperty > maxNumberOfBullets)
        {
            currentNumberOfBullets_UseProperty = maxNumberOfBullets;
        }
    }
    public void SpendAmmo(int ammoToSpend)
    {
        currentNumberOfBullets_UseProperty -= ammoToSpend;
        if (currentNumberOfBullets_UseProperty < 0)
        {
            currentNumberOfBullets_UseProperty = 0;
        }
        if (currentNumberOfBullets_UseProperty > maxNumberOfBullets)
        {
            currentNumberOfBullets_UseProperty = maxNumberOfBullets;
        }
    }

    public void AddMeter(int meterToRestore)
    {
        audioManager.PlaySound(meterRecoverySound);
        currentSpecialEnergyMeter_UseProperty += meterToRestore;
        if (currentSpecialEnergyMeter_UseProperty < 0)
        {
            currentSpecialEnergyMeter_UseProperty = 0;
        }
        if (currentSpecialEnergyMeter_UseProperty > maxSpecialEnergyMeter)
        {
            currentSpecialEnergyMeter_UseProperty = maxSpecialEnergyMeter;
        }
    }

    public void SpendMeter(int meterToSpend)
    {
        currentSpecialEnergyMeter_UseProperty -= meterToSpend;
        if (currentSpecialEnergyMeter_UseProperty < 0)
        {
            currentSpecialEnergyMeter_UseProperty = 0;
        }
        if (currentSpecialEnergyMeter_UseProperty > maxSpecialEnergyMeter)
        {
            currentSpecialEnergyMeter_UseProperty = maxSpecialEnergyMeter;
        }
    }
    #endregion
    public enum PlayerState
    {
        STATE_IDLE_SM,
        STATE_RUNNING_SM,
        STATE_JUMPING_SM,
        STATE_WALLSLIDING_SM,
        STATE_WALLJUMPING_SM,
        STATE_DAMAGED_SM,
        STATE_GROUND_DASHING_SM,
        STATE_GROUND_DASHING_CD_SM,
        STATE_BRAVE_STRIKE_SM,
        STATE_BRAVE_STRIKE_CDN_SM,
        STATE_FIRST_ATTACK_SM,
        STATE_SECOND_ATTACK_SM,
        STATE_THIRD_ATTACK_SM,
        STATE_FIRST_ATTACK_CD_SM,
        STATE_SECOND_ATTACK_CD_SM,
        STATE_THIRD_ATTACK_CD_SM,
        STATE_JUMPING_ATTACK_SM,
        STATE_JUMPING_ATTACK_CD_SM,
        STATE_UP_KICK_READY_SM,
        STATE_UP_KICK_ATTACK_SM,
        STATE_DOWN_KICK_READY_SM,
        STATE_DOWN_KICK_ATTACK_SM,
        STATE_BRAVE_KICK_CD_SM,
        STATE_BRAVE_LAUNCHER_SM,
        STATE_BRAVE_SLAM_SM,

        STATE_IDLE_TR,
        STATE_RUNNING_TR,
        STATE_JUMPING_TR,
        STATE_WALLSLIDING_TR,
        STATE_WALLJUMPING_TR,
        STATE_DAMAGED_TR,
        STATE_DASHING_TR,
        STATE_GROUND_DASHING_TR,
        STATE_GROUND_DASHING_CD_TR,
        STATE_AIR_DASHING_TR,
        STATE_AIR_DASHING_CD_TR,
        STATE_SHOOTING_IDLE_TR,
        STATE_SHOOTING_RUNNING_TR,
        STATE_SHOOTING_JUMPING_TR,
        STATE_SHOOTING_G_DASHING_TR,
        STATE_SHOOTING_G_DASHING_CD_TR,
        STATE_SHOOTING_AIR_DASHING_TR,
        STATE_SHOOTING_AIR_DASHING_CD_TR,
    }
}