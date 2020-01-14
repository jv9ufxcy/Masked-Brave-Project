using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class Player : MonoBehaviour
{
    public PlayerState _state;
    GlobalVars _globalVars;
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

    [SerializeField] private int strikeCost = 5, launcherCost = 0, shootCost = 0, burstCost = 15;

    private int maxNumberOfBullets = 3;
    private int currentNumberOfBullets_UseProperty;
    private int numberOfDashes_UseProperty = 1;
    private int minimumNumberOfDashes = 1;

    [Space]
    [Header("Movement")]

    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] float jumpStrength = 14f;
    [SerializeField] [Range(0, 1)] private float horiDamping = 0.25f, turnDamping = 0.75f, brakeDamping = 0.65f;
    [SerializeField] [Range(-1, 1)] private float fallAccelMultiplier = .25f;
    [SerializeField] private float maxCoyoteTime = 0.2f, maxJumpInputMemory = 0.2f;
    private float coyoteTimer, wallCoyoteTimer, jumpInputMemory;
    [SerializeField] private bool isOnGround;
    private int direction;
    private float defaultGravityScale;
    private bool shouldJump;
    private bool hasLanded=false;
    private bool shouldMove;
    private Vector2 jumpForce;

    [Space]
    [Header("Charged Jump")]

    [SerializeField]
    private float jumpPressure;
    [SerializeField] private float minJumpPressure = 10f;
    [SerializeField] private float maxJumpPressure = 18f;
    [SerializeField] private float jumpChargeMultiplier = 12f;
    private bool shouldChargeJump;
    private bool hasReleasedJump=false;
    private bool hasJumpChargingStarted = false;
    private bool hasReachedMaxJump = false;
    
    [Space]
    [Header("Dashing")]
    [SerializeField] private int dashCounter;
    [SerializeField] private int maxDashCounter = 3;
    [SerializeField] private float maxDashTime = 0.2f, airDashTime = 0.3f, groundDashTime = 0.15f, braveDashSpeed = 24f, bombDashSpeed = 40f;
    [SerializeField] private float dashTimer;
    private bool isDashKeyDown = false, shouldChargeDash;
    private bool hasAirDashed, hasBraveDashed;
    [SerializeField]private bool isBombDashing, isDashing;
    private float savedGravity_UseProperty;

    [SerializeField]
    private float fadeTime = 0.5f, shortDashInterval = 0.05f,longDashInterval = 0.05f;
    [SerializeField]
    private Transform afterImageParent;
    [SerializeField]
    private Color bombTrailColor = new Vector4(50, 50, 50, 0.2f), bombTrailFadeColor,braveTrailColor, braveTrailFadeColor;

    [Space]
    [Header("Detect Bounds")]

    [SerializeField] Transform groundDetectPoint;
    [SerializeField] float groundDetectRadius = 0.25f;
    [SerializeField] LayerMask whatCountsAsGround, whatCountsAsWall;

    [Space]
    [Header("Damage and Invul")]

    private bool isInvulnerable_UseProperty = false;
    [SerializeField] private float damageCooldownInSeconds = 2f;
    public float knockbackDuration, knockbackForce, maxKnockbackDuration, damageScreenFreezeTime;
    public bool knockbackFromRight;

    [Space]
    [Header("Wall Jump")]

    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallSpeed = -1.7f;
    [SerializeField] float wallJumpDistance = 4f;
    [SerializeField] float wallJumpHeight = 16f;
    [SerializeField] float wallJumpMaxTimer = 0.2f;
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


    [Space]
    [Header("Swordmaster Attacking Stats")]
    [SerializeField] private float attackTimer;

    private bool isAttackKeyDown, isAttackKeyUp, isSecondAttackKeyDown, isThirdAttackKeyDown, isTransformKeyDown, isSpecialKeyDown, isNeutralSpecialKeyDown, isDownSpecialKeyDown, isUpSpecialKeyDown, isBurstKeyDown, isBraveStrikeKeyDown, isMeterBurnKeyDown;
    private bool hasMovementStarted,attackCoroutineStarted=false;

    [SerializeField] private float attackCooldown = 0.45f, secondSlashCooldown = 0.45f, thirdSlashCooldown = 0.6f, specialAttackCooldown = 0.8f, shootingCooldown = 0.1f, burstCooldown = 0.5f;
    [SerializeField] private float braveSlamSpeed = 12f, jumpStrengthBraveStrike = 12f, jumpDistBraveStrike = 8f, upVerticalDSSpeed = 18f, downVerticalDSSpeed = 12f, horizontalDashSlashSpeed = 20f, dashSlashMaxTime = 0.3f;
    private bool anySlashReady;
    private bool upSlashReady;
    private bool downSlashReady;

    [Space]
    [Header("Kinzecter")]
    [SerializeField] private Kinzecter kinzecter;
    private float recallTime;
    [SerializeField] private float kinzecterSpeed = 40f;

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


    [Space]
    [Header("Audio")]

    private AudioManager audioManager;
    [SerializeField] private string deathSound="Death", jumpSound="Jump", takeDamageSound="PlayerTakeDamage", wallStickSound="WallStick", 
        henshinSound="Henshin", recoveryPickupSound="PickupRecovery", healthPickupSound="PickupHitPoint", ammoRecoverySound="PickupBullet", 
        meterRecoverySound = "PickupEnergy", attackingSound="SwordSlash", comboA2Sound="SwordSlam", riderPunchSound="RiderPunch", 
        bulletSound="Shoot", busterSound = "Buster", critBusterSound = "CriticalBuster";

    [Space]
    [Header("Particles")]
    [SerializeField] private ParticleSystem deathParticle, damageParticle;
    public ParticleSystem chargeJumpParticle;

    public ParticleSystem bombDashParticle,braveDashParticle;
    public ParticleSystem landingParticles;
    public ParticleSystem wallJumpParticles;
    public ParticleSystem slideParticle;
    [Header("ParticleGroups")]
    public Transform gunChargeParticles;
    public Transform jumpChargeParticles;
    public Transform flashParticles;
    public Color[] turboColors;
    public List<ParticleSystem> primaryGunParticles = new List<ParticleSystem>();
    public List<ParticleSystem> primaryJumpParticles = new List<ParticleSystem>();
    public List<ParticleSystem> secondaryParticles = new List<ParticleSystem>();

    [SerializeField]
    private string braveDash = "BraveDash", braveDamaged = "BraveDamaged", braveHenshin = "Henshin", braveSlash1 = "BraveSlash1", braveSlash2 = "BraveSlash2", braveSlash3 = "BraveSlash3", braveASlash1 = "BraveASlash1", braveASlash2 = "BraveASlash2",
        braveStrike = "braveStrike", braveLauncher = "BraveLauncher", braveSlam = "BraveSlam", braveKickUp = "BraveKickUp", braveKickDown = "BraveKickDown", braveBurst = "BraveBurst", braveABurst = "BraveABurst", braveFlourish = "BraveKinsectFlourish";
    [SerializeField]
    private string bombGShot1 = "BombGShot1", bombGShot2 = "BombGShot2", bombAShot1 = "BombAShot1", bombAShot2 = "BombAShot2", bombRShot1 = "BombRShot1", bombRShot2 = "BombRShot2", bombDash = "BombDash", bombDamage = "BombDamage" ;

    //bools
    private static Player instance;
    private Rigidbody2D myRB;
    private Rigidbody2D kinzecterRB;
    private Animator currentAnim;
    [SerializeField] private SpriteRenderer currentSpriteRenderer_UseProperty;
    private float horizontalInput, verticalInput;
    [Space]
    [Header("Blinking Colors")]
    private Color defaultColor;
    [SerializeField] private Color damageColor, invulColor, healingColor, energyColor, ammoColor, powerColor, powerColor2;
    [SerializeField] private float blinkInterval = .1f;

    private bool isSwordmaster=true;
    bool isFacingRight_UseProperty = true;
    private bool canAimRight_UseProperty = true;
    private bool coroutineStarted;

    [SerializeField] private RespawnPlayer playerRespawn;
    [SerializeField] private PlayerAfterImage playerAfterImage;

    public InputBuffer inputBuffer = new InputBuffer();

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
    public string touchpadButtonName { get; private set; }
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
    //public Vector3 BulletMuzzle
    //{
    //    get { return bulletMuzzle.transform.position; }
    //    set { bulletMuzzle.transform.position = value; }
    //}
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
        _globalVars = GlobalVars.instance;
        CheckWhichControllersAreConnected();

        //GetComponents
        CurrentSpriteRenderer = GetComponent<SpriteRenderer>();
        myRB = GetComponent<Rigidbody2D>();
        currentAnim = GetComponent<Animator>();
        playerRespawn = GetComponent<RespawnPlayer>();
        //kinzecter = GetComponent<Kinzecter>();
        //defaults
        _state = PlayerState.STATE_IDLE_BR;
        RecoveryPoints = maxRecoveryPoints;
        currentSpecialEnergyMeter_UseProperty = maxSpecialEnergyMeter;
        defaultColor = CurrentSpriteRenderer.color;
        defaultGravityScale = myRB.gravityScale;
        IsSwordmaster = true;
        currentAnim.SetBool("IsSwordmaster", IsSwordmaster);

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
        UpdateInput();
        DebugCommands();
        OnDeath();
        GetMovementInput();
        GetJumpInput();
        GetDashInput();
        GetTransformInput();
        GetAttackInput();
        UpdateCoyoteTime();
        UpdateIsOnGround();
        UpdateIsOnWall();
        UpdateIsTargetReady();
        PassAnimationStats();
    }

    private void DebugCommands()
    {
#if UNITY_EDITOR
            if (Input.GetButtonDown(touchpadButtonName))
                FullHeal();
#endif

    }
    void UpdateInput()
    {
        //inputBuffer.Update();
        switch (_state)
        {
            #region SwordmasterStates
            #region MovementStates
            case PlayerState.STATE_IDLE_BR:
                currentAnim.SetFloat("Speed", Mathf.Abs(horizontalInput));
                IsSwordmaster = true;
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_IDLE_BMB;
                }
                if (shouldJump || !isOnGround)
                {
                    _state = PlayerState.STATE_JUMPING_BR;
                }
                if (horizontalInput != 0)
                {
                    _state = PlayerState.STATE_RUNNING_BR;
                }
                if (isDashKeyDown)
                {
                    _state = PlayerState.STATE_GROUND_DASHING_BR;
                }
                GroundAttackInputs();
                
                break;
            case PlayerState.STATE_RUNNING_BR:
                if (horizontalInput == 0)
                {
                    _state = PlayerState.STATE_IDLE_BR;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_RUNNING_BMB;
                }
                if (shouldJump || !isOnGround)
                {
                    _state = PlayerState.STATE_JUMPING_BR;
                }
                if (isDashKeyDown)
                {
                    _state = PlayerState.STATE_GROUND_DASHING_BR;
                }
                GroundAttackInputs();
                break;
            case PlayerState.STATE_JUMPING_BR:
                canWallSlide = true;
                if (isOnGround)
                {
                    if (horizontalInput != 0)
                    {
                        _state = PlayerState.STATE_RUNNING_BR;
                    }
                    if (horizontalInput == 0)
                    {
                        _state = PlayerState.STATE_IDLE_BR;
                    }
                }
                if (isOnWall && isJumpKeyDown)
                    _state = PlayerState.STATE_WALLJUMPING_BR;

                if (isWallSliding)
                    _state = PlayerState.STATE_WALLSLIDING_BR;

                if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
                {
                    SpendMeter(strikeCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    SpendMeter(burstCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_BURST_BR;
                }
                if (isNeutralSpecialKeyDown)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_KINZECTER_ACTION_BR;
                }
                else if (isDownSpecialKeyDown)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_BR;
                }
                if (upSlashReady)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_UP_KICK_READY_BR;
                }
                if (downSlashReady)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_DOWN_KICK_READY_BR;
                }
                if (isAttackKeyDown)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_FIRST_JUMPING_ATTACK_BR;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_JUMPING_BMB;
                }
                break;
            case PlayerState.STATE_WALLSLIDING_BR:
                if (isJumpKeyDown)
                {
                    wallJumpTimer = wallJumpMaxTimer;
                    _state = PlayerState.STATE_WALLJUMPING_BR;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_WALLSLIDING_BMB;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    isWallSliding = false;
                    SpendMeter(burstCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_BURST_BR;
                }
                if (!isWallSliding)
                    _state = PlayerState.STATE_JUMPING_BR;
                break;
            case PlayerState.STATE_WALLJUMPING_BR:
                if (wallJumpTimer > 0)
                {
                    wallJumpTimer -= Time.deltaTime;

                    if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
                    {
                        SpendMeter(strikeCost);
                        wallJumpTimer = 0;
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                    }
                    if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                    {
                        SpendMeter(burstCost);
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_BRAVE_BURST_BR;
                    }
                    if (upSlashReady)
                    {
                        wallJumpTimer = 0;
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_UP_KICK_READY_BR;
                    }
                    if (downSlashReady)
                    {
                        wallJumpTimer = 0;
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_DOWN_KICK_READY_BR;
                    }
                    if (isAttackKeyDown && !anySlashReady)
                    {
                        wallJumpTimer = 0;
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        attackTimer = attackCooldown;
                        _state = PlayerState.STATE_FIRST_JUMPING_ATTACK_BR;
                    }
                    else if (isDownSpecialKeyDown)
                    {
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_BRAVE_SLAM_BR;
                    }

                }
                else if (wallJumpTimer <= 0)
                {
                    wallJumpTimer = 0;
                    if (isWallSliding)
                    {
                        _state = PlayerState.STATE_WALLSLIDING_BR;
                    }
                    else
                        _state = PlayerState.STATE_JUMPING_BR;
                }
                break;
            case PlayerState.STATE_DAMAGED_BR:
                attackTimer = 0;
                if (knockbackDuration <= 0)
                {
                    knockbackDuration = 0;
                    ReturnToIdleState();
                }
                break;
            case PlayerState.STATE_GROUND_DASHING_BR:
                isDashKeyDown = false;
                
                if (!hasBraveDashed)
                {
                    StartCoroutine(InvulnerableTime(2));
                    ShowAfterImage(braveTrailColor, braveTrailFadeColor, longDashInterval);
                    audioManager.PlaySound("Dash");
                    currentAnim.Play(braveDash);
                    braveDashParticle.Play();
                    hasBraveDashed = true;
                }

                if (shouldChargeDash&&dashTimer<maxDashTime)
                {
                    isDashing = true;
                }
                else
                {
                    braveDashParticle.Stop();
                    isDashing = false;
                }

                if (shouldJump == true || !isOnGround)
                {
                    braveDashParticle.Stop();
                    isDashing = false;
                    hasBraveDashed = false;
                    dashTimer = 0;
                    _state = PlayerState.STATE_JUMPING_BR;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    braveDashParticle.Stop();
                    dashTimer = 0;
                    SpendMeter(burstCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_BURST_BR;
                }
                break;
            case PlayerState.STATE_GROUND_DASHING_CD_BR:
                if (dashTimer <= 0)
                {
                    dashTimer = 0;
                    _state = PlayerState.STATE_IDLE_BR;
                }
                if (shouldJump == true || !isOnGround)
                {
                    dashTimer = 0;
                    _state = PlayerState.STATE_JUMPING_BR;
                }
                break;
            #endregion
            #region AttackStates
            
            case PlayerState.STATE_FIRST_ATTACK_BR:
                if (attackTimer >= 0)
                {
                    if (!attackCoroutineStarted)
                        StartCoroutine(PlayAttackAnim(braveSlash1, attackingSound));
                    attackTimer -= Time.deltaTime;
                    if (isAttackKeyDown)
                    {
                        TurnAround();
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_SECOND_ATTACK_BR;
                    }
                    GroundSpeciaLinks();
                }
                else
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_SECOND_ATTACK_BR:
                if (attackTimer >= 0)
                {
                    if (!attackCoroutineStarted)
                        StartCoroutine(PlayAttackAnim(braveSlash2, attackingSound));
                    attackTimer -= Time.deltaTime;
                    if (isAttackKeyDown)
                    {
                        TurnAround();
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        attackTimer = thirdSlashCooldown;
                        _state = PlayerState.STATE_THIRD_ATTACK_BR;
                    }
                    GroundSpeciaLinks();
                }
                else
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_THIRD_ATTACK_BR:
                if (attackTimer >= 0)
                {

                    if (!attackCoroutineStarted)
                        StartCoroutine(PlayAttackAnim(braveSlash3, comboA2Sound));
                    attackTimer -= Time.deltaTime;
                    GroundSpeciaLinks();
                }
                else
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_FIRST_JUMPING_ATTACK_BR:
                if (attackTimer >= 0)
                {
                    if (!attackCoroutineStarted)
                        StartCoroutine(PlayAttackAnim(braveASlash1, attackingSound));

                    attackTimer -= Time.deltaTime;
                    if (isAttackKeyDown)
                    {
                        TurnAround();
                        attackTimer = secondSlashCooldown;
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_SECOND_JUMPING_ATTACK_BR;
                    }

                    AirComboSpecialLinks();
                }
                else
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_SECOND_JUMPING_ATTACK_BR:
                if (attackTimer >= 0)
                {
                    if (!attackCoroutineStarted)
                        StartCoroutine(PlayAttackAnim(braveASlash2, attackingSound));
                    attackTimer -= Time.deltaTime;
                    if (isAttackKeyDown)
                    {
                        TurnAround();
                        attackTimer = thirdSlashCooldown;
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_THIRD_JUMPING_ATTACK_BR;
                    }

                    AirComboSpecialLinks();
                }
                else
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_THIRD_JUMPING_ATTACK_BR:
                if (attackTimer >= 0)
                {
                    if (!attackCoroutineStarted)
                        StartCoroutine(PlayAttackAnim(braveSlam, comboA2Sound));
                    attackTimer -= Time.deltaTime;
                    AirComboSpecialLinks();
                }
                else
                    ReturnToIdleState();
                break;
            #endregion
            #region BraveAttacks
            case PlayerState.STATE_BRAVE_STRIKE_BR:
                if (attackTimer>=0)
                {
                    attackTimer -= Time.deltaTime;
                    if (!attackCoroutineStarted)
                    {
                        StartCoroutine(InvulnerableTime(2));
                        
                        isBraveStrikeKeyDown = false;
                        StartCoroutine(PlayAttackAnim(braveStrike, riderPunchSound));
                    }
                }
                break;
            case PlayerState.STATE_UP_KICK_READY_BR:
                if (isSpecialKeyDown)
                    _state = PlayerState.STATE_UP_KICK_ATTACK_BR;
                else if (isDownSpecialKeyDown)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_BR;
                }

                if (isAttackKeyDown)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_FIRST_JUMPING_ATTACK_BR;
                }
                if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
                {
                    TurnAround();
                    SpendMeter(strikeCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    attackTimer = specialAttackCooldown;
                    _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    attackTimer = 0;
                    SpendMeter(burstCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_BURST_BR;
                }

                if (!upSlashReady && downSlashReady)
                    _state = PlayerState.STATE_DOWN_KICK_READY_BR;

                if (!anySlashReady)
                    _state = PlayerState.STATE_JUMPING_BR;
                break;
            case PlayerState.STATE_UP_KICK_ATTACK_BR:
                if (attackTimer >= dashSlashMaxTime)
                {
                    currentAnim.Play(braveKickUp);
                    audioManager.PlaySound(attackingSound);

                    _state = PlayerState.STATE_BRAVE_KICK_CD_BR;
                }
                break;
            case PlayerState.STATE_DOWN_KICK_READY_BR:
                if (isSpecialKeyDown)
                    _state = PlayerState.STATE_DOWN_KICK_ATTACK_BR;
                else if (isDownSpecialKeyDown)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_BR;
                }

                if (isAttackKeyDown)
                {
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_FIRST_JUMPING_ATTACK_BR;
                }
                if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
                {
                    TurnAround();
                    SpendMeter(strikeCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    attackTimer = specialAttackCooldown;
                    _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    attackTimer = 0;
                    SpendMeter(burstCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_BURST_BR;
                }

                if (!downSlashReady && upSlashReady)
                {
                    _state = PlayerState.STATE_UP_KICK_READY_BR;
                }
                if (!anySlashReady)
                {
                    _state = PlayerState.STATE_JUMPING_BR;
                }
                break;
            case PlayerState.STATE_DOWN_KICK_ATTACK_BR:
                if (attackTimer >= dashSlashMaxTime)
                {
                    currentAnim.Play(braveKickDown);
                    audioManager.PlaySound(attackingSound);

                    _state = PlayerState.STATE_BRAVE_KICK_CD_BR;
                }
                break;
            case PlayerState.STATE_BRAVE_KICK_CD_BR:
                isAttackKeyDown = false;
                upSlashReady = false;
                downSlashReady = false;
                break;
            case PlayerState.STATE_BRAVE_LAUNCHER_BR:
                if (attackTimer >= 0)
                {
                    if (!attackCoroutineStarted)
                    {
                        StartCoroutine(PlayAttackAnim(braveLauncher, attackingSound));
                    }
                    if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                    {
                        attackTimer = 0;
                        SpendMeter(burstCost);
                        hasMovementStarted = false;
                        attackCoroutineStarted = false;
                        _state = PlayerState.STATE_BRAVE_BURST_BR;
                    }
                    if (shouldJump == true || !isOnGround)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_JUMPING_BR;
                    }
                }
                break;
            case PlayerState.STATE_BRAVE_SLAM_BR:
                if (!isOnGround)
                {
                    if (!attackCoroutineStarted)
                    {
                        StartCoroutine(PlayAttackAnim(braveSlam, comboA2Sound));
                    }
                }
                else if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    attackTimer = 0;
                    SpendMeter(burstCost);
                    hasMovementStarted = false;
                    attackCoroutineStarted = false;
                    _state = PlayerState.STATE_BRAVE_BURST_BR;
                }
                else
                {
                    attackTimer = 0;
                    ReturnToIdleState();
                }
                break;
            case PlayerState.STATE_BRAVE_BURST_BR:
                if (!attackCoroutineStarted)
                {
                    StartCoroutine(InvulnerableTime(2));
                    if (isOnGround)
                        StartCoroutine(PlayAttackAnim(braveBurst, deathSound));
                    else
                        StartCoroutine(PlayAttackAnim(braveABurst, deathSound));
                    PlayFlashParticle(energyColor);
                    Screenshake();
                }
                break;
            case PlayerState.STATE_KINZECTER_ACTION_BR:
                if(!attackCoroutineStarted)
                    StartCoroutine(PlayAttackAnim(braveFlourish, meterRecoverySound));
                
                ReturnToIdleState();
                break;
            case PlayerState.STATE_KINZECTER_RECALL_BR:
                break;
            #endregion
            #endregion
            #region BombadierStates
            case PlayerState.STATE_IDLE_BMB:
                HandleCharging();
                IsSwordmaster = false;
                if (shouldJump || !isOnGround)
                {
                    _state = PlayerState.STATE_JUMPING_BMB;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_IDLE_BR;
                }
                if (horizontalInput != 0)
                {
                    _state = PlayerState.STATE_RUNNING_BMB;
                }

                if (isAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_IDLE_BMB;
                }

                if (isDashKeyDown && !hasAirDashed)
                {
                    audioManager.PlaySound("Dash");
                    currentAnim.Play(bombDash);
                    _state = PlayerState.STATE_DASHING_BMB;
                }
                break;
            case PlayerState.STATE_RUNNING_BMB:
                if (horizontalInput == 0)
                {
                    _state = PlayerState.STATE_IDLE_BMB;
                }
                if (shouldJump || !isOnGround)
                {
                    _state = PlayerState.STATE_JUMPING_BMB;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_RUNNING_BR;
                }
                if (isAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_RUNNING_BMB;
                }
                if (isDashKeyDown && !hasAirDashed)
                {
                    audioManager.PlaySound("Dash");
                    currentAnim.Play(bombDash);
                    _state = PlayerState.STATE_DASHING_BMB;
                }
                HandleCharging();
                break;
            case PlayerState.STATE_JUMPING_BMB:
                hasReleasedJump = false;
                canWallSlide = true;
                if (isOnGround)
                {
                    if (horizontalInput != 0)
                    {
                        _state = PlayerState.STATE_RUNNING_BMB;
                    }
                    if (horizontalInput == 0)
                    {
                        _state = PlayerState.STATE_IDLE_BMB;
                    }
                }
                if (isWallSliding)
                {
                    _state = PlayerState.STATE_WALLSLIDING_BMB;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_JUMPING_BR;
                }
                if (isAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_JUMPING_BMB;
                }
                if (isDashKeyDown && !hasAirDashed)
                {
                    audioManager.PlaySound("Dash");
                    currentAnim.Play(bombDash);
                    _state = PlayerState.STATE_DASHING_BMB;
                }
                HandleCharging();
                break;
            case PlayerState.STATE_WALLSLIDING_BMB:
                if (isJumpKeyDown)
                {
                    wallJumpTimer = wallJumpMaxTimer;
                    _state = PlayerState.STATE_WALLJUMPING_BMB;
                }
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_WALLSLIDING_BR;
                }
                if (!isWallSliding)
                    _state = PlayerState.STATE_JUMPING_BMB;
                HandleCharging();
                break;
            case PlayerState.STATE_WALLJUMPING_BMB:
                if (wallJumpTimer > 0)
                {
                    wallJumpTimer -= Time.deltaTime;
                }
                else if (wallJumpTimer <= 0)
                {
                    wallJumpTimer = 0;
                    if (isWallSliding)
                    {
                        _state = PlayerState.STATE_WALLSLIDING_BMB;
                    }
                    else
                        _state = PlayerState.STATE_JUMPING_BMB;
                }
                HandleCharging();
                break;
            case PlayerState.STATE_DAMAGED_BMB:
                if (knockbackDuration <= 0)
                {
                    knockbackDuration = 0;
                    _state = PlayerState.STATE_IDLE_BMB;
                }
                HandleCharging();
                break;
            case PlayerState.STATE_DASHING_BMB:
                break;
            case PlayerState.STATE_SHOOTING_IDLE_BMB:
                FireBullet();
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;

                    if (isAttackKeyDown && horizontalInput != 0)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_RUNNING_BMB;
                    }
                    else if (isAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_BMB;
                    }
                    else if (!isAttackKeyDown && horizontalInput != 0)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_RUNNING_BMB;
                    }
                    else if (!isAttackKeyDown && shouldJump)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_JUMPING_BMB;
                    }
                }
                else if (attackTimer <= 0)
                {
                    attackTimer = 0;
                    _state = PlayerState.STATE_IDLE_BMB;
                }
                HandleCharging();
                break;
            case PlayerState.STATE_SHOOTING_RUNNING_BMB:
                FireBullet();
                HandleCharging();
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;

                    if (isAttackKeyDown && horizontalInput == 0)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_IDLE_BMB;
                    }
                    else if (isAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_BMB;
                    }
                    else if (!isAttackKeyDown && horizontalInput != 0)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_RUNNING_BMB;
                    }
                    else if (!isAttackKeyDown && shouldJump)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_JUMPING_BMB;
                    }
                }
                else if (attackTimer <= 0)
                {
                    attackTimer = 0;
                    _state = PlayerState.STATE_IDLE_BMB;
                }
                break;
            case PlayerState.STATE_SHOOTING_JUMPING_BMB:
                hasReleasedJump = true;
                if (attackTimer > 0)
                {
                    attackTimer -= Time.fixedDeltaTime;

                    if (isAttackKeyDown && isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_IDLE_BMB;
                    }
                    else if (isAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_BMB;
                    }
                    else if (!isAttackKeyDown && isOnGround)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_RUNNING_BMB;
                    }
                    else if (isWallSliding)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_WALLSLIDING_BMB;
                    }
                }
                else if (attackTimer <= 0)
                {
                    attackTimer = 0;
                    _state = PlayerState.STATE_JUMPING_BMB;
                }
                HandleCharging();
                break;
            default:
                HandleCharging();
                break;
                #endregion
        }
    }

    

    private void FixedUpdate()
    {
        if (!PauseManager.IsGamePaused)
            HandleStates();

        FixedUpdateJumpMemory();
        if (!isOnGround) return;
    }
    private void HandleStates()
    {
        switch (_state)
        {
            //Swordmaster Style
            #region SwordmasterStates
            #region SwordmasterMovementStates
            case PlayerState.STATE_IDLE_BR:              
                myRB.velocity = new Vector2(0, myRB.velocity.y);
                break;
            case PlayerState.STATE_RUNNING_BR:
                CheckMove();
                
                break;
            case PlayerState.STATE_JUMPING_BR:
                Jump();
                WallJump();
                BetterJump();
                CheckMove();
                break;
            case PlayerState.STATE_WALLSLIDING_BR:
                WallSliding();
                break;
            case PlayerState.STATE_WALLJUMPING_BR:
                WallJump();
                BetterJump();
                break;
            case PlayerState.STATE_DAMAGED_BR:
                CheckMove();
                break;
            case PlayerState.STATE_GROUND_DASHING_BR:
                DashingBrave();
                break;
            case PlayerState.STATE_GROUND_DASHING_CD_BR:
                break;
            #endregion
            #region AttackingStates
            case PlayerState.STATE_FIRST_ATTACK_BR:

                if (!hasMovementStarted)
                    StartCoroutine(AttackMovement(myRB.velocity.x / 2, myRB.velocity.y / 2, true, .1f, .6f));

                break;
            case PlayerState.STATE_SECOND_ATTACK_BR:

                if (!hasMovementStarted)
                    StartCoroutine(AttackMovement(myRB.velocity.x / 2, myRB.velocity.y / 2, true, .1f, .6f));

                break;
            case PlayerState.STATE_THIRD_ATTACK_BR:

                if (!hasMovementStarted)
                    StartCoroutine(AttackMovement(myRB.velocity.x / 4, myRB.velocity.y / 2, true, .1f, .6f));

                break;
            case PlayerState.STATE_FIRST_JUMPING_ATTACK_BR:
                CheckMove();
                BetterJump();

                if (!hasMovementStarted)
                    StartCoroutine(AttackMovement(myRB.velocity.x/2, myRB.velocity.y / 2, true, .1f, .6f));

                break;
            case PlayerState.STATE_SECOND_JUMPING_ATTACK_BR:
                CheckMove();
                BetterJump();

                if (!hasMovementStarted)
                    StartCoroutine(AttackMovement(myRB.velocity.x / 2, myRB.velocity.y / 2, true, .1f, .6f));

                break;
            case PlayerState.STATE_THIRD_JUMPING_ATTACK_BR:
                CheckMove();
                BetterJump();
                
                if (!hasMovementStarted)
                    StartCoroutine(AttackMovement(myRB.velocity.x / 2, myRB.velocity.y / 2, true, .1f, .6f));

                    ReturnToIdleState();
                break;
            #endregion
            #region BraveAttacks
            case PlayerState.STATE_BRAVE_STRIKE_BR:
                if (!hasMovementStarted)
                    StartCoroutine(BraveStrike());
                break;

            case PlayerState.STATE_BRAVE_LAUNCHER_BR:
                if(!hasMovementStarted)
                    StartCoroutine(AttackMovement(myRB.velocity.x/2, jumpStrength,true, .1f, .6f));

                    jumpInputMemory = 0;
                    coyoteTimer = 0;
                break;
            case PlayerState.STATE_BRAVE_SLAM_BR:
                if (!isOnGround)
                {
                    if (!hasMovementStarted)
                        StartCoroutine(AttackMovement(0f, -braveSlamSpeed, true, .1f, .6f));
                }
                break;
            case PlayerState.STATE_UP_KICK_READY_BR:
                Jump();
                BetterJump();
                CheckMove();
                
                break;
            case PlayerState.STATE_UP_KICK_ATTACK_BR:
                myRB.velocity = new Vector2(0, 0);
                attackTimer = dashSlashMaxTime;
                if (attackTimer >= dashSlashMaxTime)
                {
                    if (CanAimRight)
                    {
                        myRB.velocity = new Vector2(horizontalDashSlashSpeed, upVerticalDSSpeed);
                    }
                    else
                    {
                        myRB.velocity = new Vector2(-horizontalDashSlashSpeed, upVerticalDSSpeed);
                    }
                }
                break;
            case PlayerState.STATE_DOWN_KICK_READY_BR:
                Jump();
                BetterJump();
                CheckMove();
                break;
            case PlayerState.STATE_DOWN_KICK_ATTACK_BR:
                myRB.velocity = new Vector2(0, 0);
                attackTimer = dashSlashMaxTime;
                if (attackTimer >= dashSlashMaxTime)
                {
                    if (CanAimRight)
                    {
                        myRB.AddForce(new Vector2(horizontalDashSlashSpeed, -downVerticalDSSpeed), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRB.AddForce(new Vector2(-horizontalDashSlashSpeed, -downVerticalDSSpeed), ForceMode2D.Impulse);
                    }
                }
                break;
            case PlayerState.STATE_BRAVE_KICK_CD_BR:
                attackTimer -= Time.fixedDeltaTime;
                ReturnToIdleState();
                break;
            case PlayerState.STATE_BRAVE_BURST_BR:
                if (!hasMovementStarted)
                    StartCoroutine(BraveBurstTimer());
                break;
            case PlayerState.STATE_KINZECTER_ACTION_BR:
                KinzecterThrow();
                break;
            #endregion
            #endregion

            #region TricksterStates
            #region TricksterMovementStates
            case PlayerState.STATE_IDLE_BMB:
                CheckMove();
                break;
            case PlayerState.STATE_RUNNING_BMB:
                CheckMove();
                break;
            case PlayerState.STATE_JUMPING_BMB:
                Jump();
                WallJump();
                BetterJump();
                CheckMove();
               
                break;
            case PlayerState.STATE_WALLSLIDING_BMB:
                WallSliding();
                break;
            case PlayerState.STATE_WALLJUMPING_BMB:
                WallJump();
                BetterJump();
                break;
            case PlayerState.STATE_DAMAGED_BMB:
                CheckMove();
                break;
            case PlayerState.STATE_DASHING_BMB:
                NumberOfDashes--;
                if (!coroutineStarted)
                    Dash(horizontalInput, verticalInput);
                break;
            #endregion
            #region Trickster Shooting States
            case PlayerState.STATE_SHOOTING_IDLE_BMB:
                    
                break;
            case PlayerState.STATE_SHOOTING_RUNNING_BMB:
                CheckMove();
                break;
            case PlayerState.STATE_SHOOTING_JUMPING_BMB:
                CheckMove();
                FireBullet();
                break;
                #endregion
                #endregion
        }
    }
    #region Attacking Functions
    private void KinzecterThrow()
    {
        if (kinzecter.isWithPlayer())
        {
            Vector3 throwDir = (wallCheck.transform.position - transform.position).normalized;
            kinzecter.ThrowKinzecter(throwDir);
        }
        else
            KinzecterRecall();
    }
    private void KinzecterRecall()
    {
        kinzecter.Recall();
    }
    private void BraveBurst()
    {
        StartCoroutine(BraveBurstTimer());
    }
    private void ReturnToIdleState()
    {
        if (attackTimer <= 0)
        {
            attackTimer = 0;
            if (horizontalInput != 0)
                _state = PlayerState.STATE_RUNNING_BR;
            else
                _state = PlayerState.STATE_IDLE_BR;
        }
        braveDashParticle.Stop();
        isDashing = false;
        hasBraveDashed = false;
        upSlashReady = false;
        downSlashReady = false;
    }
    private void GroundAttackInputs()
    {
        if (isAttackKeyDown)
        {
            hasMovementStarted = false;
            attackCoroutineStarted = false;
            attackTimer = attackCooldown;
            _state = PlayerState.STATE_FIRST_ATTACK_BR;
        }
        if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
        {
            SpendMeter(strikeCost);

            hasMovementStarted = false;
            attackCoroutineStarted = false;
            _state = PlayerState.STATE_BRAVE_STRIKE_BR;
        }
        if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
        {
            SpendMeter(burstCost);
            hasMovementStarted = false;
            attackCoroutineStarted = false;
            _state = PlayerState.STATE_BRAVE_BURST_BR;
        }
        if (isNeutralSpecialKeyDown)
        {
            hasMovementStarted = false;
            attackCoroutineStarted = false;
            _state = PlayerState.STATE_KINZECTER_ACTION_BR;
        }
        if (isUpSpecialKeyDown || isDownSpecialKeyDown)
        {
            hasMovementStarted = false;
            attackCoroutineStarted = false;

            _state = PlayerState.STATE_BRAVE_LAUNCHER_BR;
        }
    }
    private void GroundSpeciaLinks()
    {
        if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
        {
            TurnAround();
            SpendMeter(strikeCost);
            hasMovementStarted = false;
            attackCoroutineStarted = false;
            _state = PlayerState.STATE_BRAVE_STRIKE_BR;
        }
        if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
        {

            SpendMeter(burstCost);
            hasMovementStarted = false;
            attackCoroutineStarted = false;
            _state = PlayerState.STATE_BRAVE_BURST_BR;
        }
        else if (isDownSpecialKeyDown)
        {
            TurnAround();
            hasMovementStarted = false;
            attackCoroutineStarted = false;

            _state = PlayerState.STATE_BRAVE_LAUNCHER_BR;
        }

        if (shouldJump == true || !isOnGround)
        {
            attackTimer = 0;
            _state = PlayerState.STATE_JUMPING_BR;
        }
    }
    private void AirComboSpecialLinks()
    {
        if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
        {
            TurnAround();
            SpendMeter(strikeCost);
            hasMovementStarted = false;
            attackCoroutineStarted = false;
            _state = PlayerState.STATE_BRAVE_STRIKE_BR;
        }
        if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
        {
            SpendMeter(burstCost);
            hasMovementStarted = false;
            attackCoroutineStarted = false;
            _state = PlayerState.STATE_BRAVE_BURST_BR;
        }
        else if (isDownSpecialKeyDown && !isOnGround)
        {
            TurnAround();
            hasMovementStarted = false;
            attackCoroutineStarted = false;
            _state = PlayerState.STATE_BRAVE_SLAM_BR;
        }
        if (isOnGround)
            ReturnToIdleState();
    }
    private void DashingBrave()
    {
        if (shouldChargeDash && dashTimer < maxDashTime)//shouldChargeDash uses GetButton && dash timer must be lower than maxTime
        {
            isDashing = true;
            
            dashTimer += Time.fixedDeltaTime;
            myRB.velocity = new Vector2(braveDashSpeed * direction, 0);
        }
        else //released dash button || exceeded time
        {
            dashTimer = 0;
            ReturnToIdleState();
        }
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

                if (isOnGround)
                {
                    currentAnim.Play(bombGShot1);
                }
                else
                {
                    currentAnim.Play(bombAShot1);
                }
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
            if (isOnGround)
            {
                currentAnim.Play(bombGShot2);
            }
            else
            {
                currentAnim.Play(bombAShot2);
            }
            SpendAmmo(1);
            audioManager.PlaySound(busterSound);
            Screenshake();
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
            if (isOnGround)
            {
                currentAnim.Play(bombGShot2);
            }
            else
            {
                currentAnim.Play(bombAShot2);
            }
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
        if (isOnGround||!isSwordmaster)
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

        isAtPeakJumpHeight = Input.GetButtonUp(bottomFaceButtonName);
        if (Input.GetButtonDown(bottomFaceButtonName))
        {
            jumpInputMemory = maxJumpInputMemory;
        }
        //if (Input.GetButtonDown(bottomFaceButtonName) && isWallSliding)
        //{
        //    shouldWallJump = true;
        //}

        if (verticalInput == -1 && isOnGround)
        {
            shouldChargeJump = true;
        }
        else
            shouldChargeJump = false;
        //shouldChargeJump = Input.GetButton(bottomFaceButtonName);
        
    }
    private void FixedUpdateJumpMemory()
    {
        if ((jumpInputMemory > 0) && (coyoteTimer > 0))// if the buffer has time remaining the jump will execute
        {
            jumpInputMemory = 0;
            coyoteTimer = 0;
            shouldJump = true;
        }
        else
            shouldJump = false;

        if ((jumpInputMemory > 0) && (wallCoyoteTimer > 0))// if the buffer has time remaining the jump will execute
        {
            jumpInputMemory = 0;
            wallCoyoteTimer = 0;
            shouldWallJump = true;
        }
        else
            shouldWallJump = false;

        coyoteTimer -= Time.fixedDeltaTime;
        if (isOnGround)
            coyoteTimer = maxCoyoteTime;

        wallCoyoteTimer -= Time.fixedDeltaTime;
        if(isOnWall)
            wallCoyoteTimer = maxCoyoteTime;
    }

    void UpdateCoyoteTime()// holds the jump input for .2 seconds before landing/after walking off platforms; use until we implement an actual input buffer
    {
        if (jumpInputMemory > 0)
            jumpInputMemory -= Time.deltaTime;
    }
    private void GetDashInput()
    {
        isDashKeyDown = (Input.GetButtonDown(rightFaceButtonName));
        if (Input.GetButton(rightFaceButtonName))
            shouldChargeDash = true;
        else if (Input.GetButtonUp(rightFaceButtonName))
            shouldChargeDash = false;
    }
    private void GetAttackInput()
    {
        if (Input.GetAxis(rightTriggerName) == 1 || Input.GetButtonDown(rightTriggerName))
            isMeterBurnKeyDown = true;
        else
            isMeterBurnKeyDown = false;

        if (isMeterBurnKeyDown)
        {
            CheckParticles();

            if (isSpecialKeyDown)
                isBurstKeyDown = true;
            else
                isBurstKeyDown = false;

            if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
                isBraveStrikeKeyDown = true;
            else
                isBraveStrikeKeyDown = false;
        }
        else
        {
            hasJumpChargingStarted = false;
            chargeJumpParticle.Stop();

            switch (myController)
            {
                case ControllerList.keyboard:
                    //isAttackKeyDown = (Input.GetMouseButtonDown(0));
                    //isSpecialKeyDown = (Input.GetMouseButtonDown(1));
                    //shouldChargeBuster = (Input.GetMouseButton(0));
                    //isAttackKeyUp = (Input.GetMouseButtonUp(0));

                    if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName) || Input.GetMouseButtonDown(0))
                    {
                        isAttackKeyDown = true;
                        shouldChargeBuster = true;
                    }
                    else
                    {
                        isAttackKeyDown = false;
                        shouldChargeBuster = false;
                    }
                    if (Input.GetButtonUp(leftFaceButtonName) || Input.GetButtonUp(rightBumperName) || Input.GetMouseButtonUp(0))
                        isAttackKeyUp = true;
                    else
                        isAttackKeyUp = false;
                    if (Input.GetButtonDown(topFaceButtonName) || Input.GetMouseButtonDown(1))
                        isSpecialKeyDown = true;
                    else
                        isSpecialKeyDown = false;
                    break;
                case ControllerList.ps4:
                    if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
                    {
                        isAttackKeyDown = true;
                        shouldChargeBuster = true;
                    }
                    else
                    {
                        isAttackKeyDown = false;
                        shouldChargeBuster = false;
                    }

                    if (Input.GetButtonUp(leftFaceButtonName) || Input.GetButtonUp(rightBumperName))
                        isAttackKeyUp = true;
                    else
                        isAttackKeyUp = false;

                    isSpecialKeyDown = (Input.GetButtonDown(topFaceButtonName));
                    break;
                case ControllerList.xbox:
                    if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
                    {
                        isAttackKeyDown = true;
                        shouldChargeBuster = true;
                    }
                    else
                    {
                        isAttackKeyDown = false;
                        shouldChargeBuster = false;
                    }

                    if (Input.GetButtonUp(leftFaceButtonName) || Input.GetButtonUp(rightBumperName))
                        isAttackKeyUp = true;
                    else
                        isAttackKeyUp = false;

                    isSpecialKeyDown = (Input.GetButtonDown(topFaceButtonName));
                    break;
                default:

                    break;
            }

            if (isSpecialKeyDown && verticalInput == 1)
            {
                isNeutralSpecialKeyDown = false;
                isDownSpecialKeyDown = false;
                isUpSpecialKeyDown = true;
            }
            else if (isSpecialKeyDown && verticalInput == -1)
            {
                isNeutralSpecialKeyDown = false;
                isDownSpecialKeyDown = true;
                isUpSpecialKeyDown = false;
            }
            else if (isSpecialKeyDown && verticalInput == 0)
            {
                isNeutralSpecialKeyDown = true;
                isDownSpecialKeyDown = false;
                isUpSpecialKeyDown = false;
            }
            else
            {
                isNeutralSpecialKeyDown = false;
                isDownSpecialKeyDown = false;
                isUpSpecialKeyDown = false;
            }
        }
    }
    private void GetTransformInput()
    {
        isTransformKeyDown = (Input.GetButtonDown(leftBumperName));
    }
    private void CheckWhichControllersAreConnected()
    {
        #region ControllerCheck

        //int joystickNumber = Input.GetJoystickNames().Length;//get joystick name
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
                    touchpadButtonName = "Ps4Touchpad";
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
                    touchpadButtonName = "XboxBack";
                }
                break;
            case 2:
                {
                    myController = ControllerList.keyboard;
                    horizontalAxisName = "KeyboardHorizontal";
                    altHorizontalAxisName = horizontalAxisName;//no alt button with keyboard
                    verticalAxisName = "KeyboardVertical";
                    altVerticalAxisName = verticalAxisName;
                    topFaceButtonName = "KeyboardE";
                    bottomFaceButtonName = "KeyboardSpace";
                    leftFaceButtonName = "KeyboardF";
                    rightFaceButtonName = "KeyboardLeftShift";
                    leftBumperName = "KeyboardQ";
                    rightBumperName = "KeyboardE";
                    leftTriggerName = "KeyboardC";
                    rightTriggerName = "KeyboardLeftCtrl";
                    startButtonName = "KeyboardEscape";
                    selectButtonName = "KeyboardBackspace";
                    touchpadButtonName = "KeyboardBackspace";
                }
                break;
            default:
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
                    touchpadButtonName = "Ps4Touchpad";
                }
                break;
        }
    #endregion
    }
    #endregion
    #region Movement Functions
    private void CheckMove()
    {
        if (knockbackDuration <= 0)
        {
            shouldMove = true;
            currentAnim.SetFloat("Speed", Mathf.Abs(horizontalInput));
            if (shouldMove)
            {
                Move(movementSpeed);
            }
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
    }

    private void Move(float moveSpeed)
    {
        float horiVelocity = myRB.velocity.x;
        horiVelocity += horizontalInput;

        if (Mathf.Abs(horizontalInput) < 0.01f)
            horiVelocity *= Mathf.Pow(1f - brakeDamping, Time.deltaTime * moveSpeed);//if not moving
        else if (Mathf.Sign(horizontalInput) != Mathf.Sign(horiVelocity))
            horiVelocity *= Mathf.Pow(1f - turnDamping, Time.deltaTime * moveSpeed); //if turning
        else
            horiVelocity *= Mathf.Pow(1f - horiDamping, Time.deltaTime * moveSpeed);

        myRB.velocity = new Vector2(horiVelocity, myRB.velocity.y);
        TurnAround();
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
            wallCoyoteTimer = 0;
            jumpInputMemory = 0;
            StopCoroutine(DisableMovement(0f));
            StartCoroutine(DisableMovement(.4f));
            canWallSlide = false;
            audioManager.PlaySound(jumpSound);
            landingParticles.Play();
            slideParticle.Stop();

            myRB.velocity = new Vector2(wallJumpDistance * -direction, wallJumpHeight);
            shouldWallJump = false;
        }

    }
    private void TurnAround()
    {
        if (horizontalInput > 0)
        {
            direction = 1;
            transform.localScale = new Vector3(1f, 1f, 1f);
            CanAimRight = true;
        }
        else if (horizontalInput < 0)
        {
            direction = -1;
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
            myRB.velocity = new Vector2(myRB.velocity.x, jumpStrength);
            jumpInputMemory = 0;
            coyoteTimer = 0;
            currentAnim.SetBool("Ground", false);
            currentAnim.SetFloat("vSpeed", myRB.velocity.y);
            audioManager.PlaySound(jumpSound);
            landingParticles.Play();

            isOnGround = false;
        }
    }
    private void HandleCharging()
    {
        //ChargeJump();
        ChargedShot();
    }

    
    private void CheckParticles()
    {
        if (!hasJumpChargingStarted)
        {
            chargeJumpParticle.Play();
            audioManager.PlaySound(meterRecoverySound);
            hasJumpChargingStarted = true;
        }
    }

    private void BetterJump()
    {
        if (myRB.velocity.y > 0 && isAtPeakJumpHeight)
        {
            myRB.velocity=new Vector2(myRB.velocity.x, myRB.velocity.y*fallAccelMultiplier);
        }
    }
    private void PassAnimationStats()
    {
        currentAnim.SetFloat("vSpeed", myRB.velocity.y);
        currentAnim.SetFloat("hSpeed", myRB.velocity.x);
        currentAnim.SetBool("IsSwordmaster", IsSwordmaster);
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
        DashReset();
        if (!hasLanded)
        {
            hasLanded = true;
            landingParticles.Play();
        }
    }
    private void WallTouch()
    {
        slideParticle.Play();
        DashReset();
        if (!hasLanded)
        {
            hasLanded = true;
            landingParticles.Play();
        }
    }

    private void DashReset()
    {
        hasAirDashed = false;
        isBombDashing = false;
        NumberOfDashes = minimumNumberOfDashes;
    }

    private void UpdateIsOnWall()
    {
        isOnWall = Physics2D.Linecast(transform.position, wallCheck.position, whatCountsAsWall);
        if (canWallSlide)
        {
            isWallSliding = (CanAimRight && horizontalInput > 0.1f || !CanAimRight && horizontalInput < -0.1f) && (isOnWall && !isOnGround && myRB.velocity.y<=0);
        }
        else
        {
            isWallSliding = false;
            slideParticle.Stop();
        }
        currentAnim.SetBool("WallSliding", isWallSliding);
    }

    private void Dash(float x, float y)
    {
        Screenshake();
        if (NumberOfDashes <= 0 && CurrentNumberOfBullets > 0)
        {
            if(!isOnGround)
                SpendAmmo(1);
            if (CurrentNumberOfBullets <= 0)
            {
                hasAirDashed = true;
            }
        }
        else
            hasAirDashed = true;

        if (x==0&&y==0)//if no directional input is used
            x = direction;//dash horizontally in direction player is facing

        myRB.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);
        myRB.velocity += dir.normalized * bombDashSpeed;
        StartCoroutine(DashWait());
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
    private void ChargeJump()
    {
        if (shouldChargeJump)
        {
            CheckParticles();
            if (jumpPressure < maxJumpPressure)
            {
                jumpPressure += Time.fixedDeltaTime * jumpChargeMultiplier;
                audioManager.PlaySound(henshinSound);
                hasJumpChargingStarted = true;
            }
            else
            {
                hasJumpChargingStarted = false;
                jumpPressure = maxJumpPressure;
            }

        }
        else if (isJumpKeyDown)
        {
            if (jumpPressure > 0f)
            {
                ShowAfterImage(invulColor, bombTrailFadeColor, shortDashInterval);
                currentAnim.SetBool("Ground", false);
                currentAnim.SetFloat("vSpeed", myRB.velocity.y);
                hasReleasedJump = true;
                jumpPressure = jumpPressure + minJumpPressure;
                myRB.AddForce(new Vector2(0f, jumpPressure), ForceMode2D.Impulse);
                jumpPressure = 0f;
                audioManager.PlaySound(jumpSound);
                hasReachedMaxJump = false;
                chargeJumpParticle.Stop();
                //chargeJumpMaxColor.Stop();
            }
            else if (!shouldChargeJump)
            {
                jumpPressure = 0;
                chargeJumpParticle.Stop();
                //chargeJumpMaxColor.Stop();
            }
            else
            {
                currentAnim.SetBool("Ground", false);
                currentAnim.SetFloat("vSpeed", myRB.velocity.y);
                //myRB.AddForce(jumpForce, ForceMode2D.Impulse);
                audioManager.PlaySound(jumpSound);

                isOnGround = false;
                shouldJump = false;
                jumpPressure = 0;
                chargeJumpParticle.Stop();
                //chargeJumpMaxColor.Stop();
            }
        }
        else
        {
            jumpPressure = 0;
            chargeJumpParticle.Stop();
            //chargeJumpMaxColor.Stop();
        }
    }
    #endregion

    private void Henshin()
    {
        audioManager.PlaySound(henshinSound);
        IsSwordmaster = !IsSwordmaster;
        currentAnim.SetBool("IsSwordmaster", IsSwordmaster);
        //TODO: Set box collider active false
    }
    private void FullHeal()
    {
        AddRecovery(maxRecoveryPoints);
        AddMeter(maxSpecialEnergyMeter);
        AddAmmo(maxNumberOfBullets);
    }
    private void OnDeath()
    {
        CurrentSpriteRenderer.color = defaultColor;
        if (RecoveryPoints <= 0)
        {
            audioManager.PlaySound(deathSound);
            deathParticle.Play();
            StartCoroutine(WaitWhileRespawningCoroutine());
        }
    }
    #region PolishRegion
    public void ShowAfterImage(Color afterimageColor, Color fadeColor, float interval)
    {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < afterImageParent.childCount; i++)
        {
            Transform currentGhost = afterImageParent.GetChild(i);
            s.AppendCallback(() => currentGhost.position = Player.Instance.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = !CanAimRight);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().sprite = CurrentSpriteRenderer.sprite);
            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(afterimageColor, 0));
            s.AppendCallback(() => FadeSprite(currentGhost, fadeColor));
            s.AppendInterval(interval);
        }
    }
    public void FadeSprite(Transform current, Color fadeColor)
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
    public void DoHitStop(float hitStop)
    {
        StartCoroutine(HitStopAttacker(hitStop));
    }
    IEnumerator HitStopAttacker(float hitStop)
    {
        Vector2 playerSpeed = myRB.velocity;//save speed ref
        myRB.velocity = Vector2.zero;////stop moving
        currentAnim.speed = 0;//stop animating
        shouldMove = false;//don't let player input movement
        yield return new WaitForSecondsRealtime(hitStop); //stop duration
        //free my boys
        myRB.velocity = playerSpeed;//return the saved speed reference
        currentAnim.speed = 1;//let animator play
        shouldMove = true;//let player input movement again
    }
    #endregion
    #region Damage
    public void TakeDamage(int damageToGive)
    {
        if (!IsInvulnerable)
        {
            if (IsSwordmaster)
            {
                currentAnim.Play(braveDamaged);
                _state = PlayerState.STATE_DAMAGED_BR;
            }
            else
            {
                currentAnim.Play(bombDamage);
                _state = PlayerState.STATE_DAMAGED_BMB;
            }
            Screenshake();
            DoHitStop(0.1f);
            RecoveryPoints -= damageToGive;
            audioManager.PlaySound(takeDamageSound);
            OnDeath();
            damageParticle.Play();
            //start damage cooldown
            StartCoroutine(InvulnerableTime(damageCooldownInSeconds));
        }
    }
    public void KillPlayer(int damageToGive)
    {
        if (IsSwordmaster)
            currentAnim.Play(braveDamaged);
        else
            currentAnim.Play(bombDamage);

        RecoveryPoints -= damageToGive;
        audioManager.PlaySound(takeDamageSound);

        OnDeath();
    }
    #endregion
    #region CoRoutines
    IEnumerator DisableMovement(float time)
    {
        shouldMove = false;
        yield return new WaitForSeconds(time);
        shouldMove = true;
    }
    IEnumerator DashWait()
    {
        coroutineStarted = true;
        ShowAfterImage(bombTrailColor, bombTrailFadeColor, shortDashInterval);
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        bombDashParticle.Play();
        isBombDashing = true;

        yield return new WaitForSeconds(airDashTime);

        bombDashParticle.Stop();
        BetterJump();
        isBombDashing = false;
        coroutineStarted = false;
        _state = PlayerState.STATE_JUMPING_BMB;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(groundDashTime);
        if (isOnGround)
            hasAirDashed = false;
    }
    private IEnumerator BlinkWhileChargingCoroutine()
    {
        Color chargingColor = turboColors[0]; ;

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
    IEnumerator PlayAttackAnim(string attackAnim, string soundName)
    {
        attackCoroutineStarted = true;

        audioManager.PlaySound(soundName);
        currentAnim.Play(attackAnim);
        yield return null;
    }
    IEnumerator AttackMovement(float velocityX, float velocityY, bool hasStartup, float startup, float cooldown)
    {
        hasMovementStarted = true;
        if (hasStartup)
        {
            myRB.velocity = Vector2.zero;
            yield return new WaitForSeconds(startup);
        }
        myRB.velocity = new Vector2(velocityX, velocityY);
        yield return new WaitForSeconds(cooldown);
        ReturnToIdleState();
    }
    IEnumerator BraveStrike()
    {
        hasMovementStarted = true;
        if (CanAimRight)
        {
            myRB.velocity=new Vector2(jumpDistBraveStrike, jumpStrengthBraveStrike);
            yield return new WaitForSeconds(.35f);
            myRB.velocity = new Vector2(jumpDistBraveStrike, -jumpStrengthBraveStrike/2);
        }
        else
        {
            myRB.velocity = new Vector2(-jumpDistBraveStrike, jumpStrengthBraveStrike);
            yield return new WaitForSeconds(.35f);
            myRB.velocity = new Vector2(-jumpDistBraveStrike, -jumpStrengthBraveStrike/2);
        }
        yield return new WaitForSeconds(specialAttackCooldown);
        IsInvulnerable = false;
        attackTimer = 0;
        ReturnToIdleState();
    }
    IEnumerator BraveBurstTimer()
    {
        
        hasMovementStarted = true;
        IsInvulnerable = true;
        myRB.velocity = Vector2.zero;

        yield return new WaitForSeconds(burstCooldown);
        IsInvulnerable = false;
        _state = PlayerState.STATE_JUMPING_BR;
    }

    private IEnumerator InvulnerableTime(float time)
    {
        IsInvulnerable=true;
        yield return new WaitForSeconds(time);
        IsInvulnerable=false;
    }
    private IEnumerator BlinkWhileInvulnerableCoroutine()
    {
        while(IsInvulnerable)
        {
            CurrentSpriteRenderer.color = invulColor;
            yield return new WaitForSeconds(blinkInterval);

            CurrentSpriteRenderer.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
    private IEnumerator BlinkWhileHealingCoroutine()
    {
            CurrentSpriteRenderer.color = healingColor;
            yield return new WaitForSeconds(blinkInterval);

            CurrentSpriteRenderer.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);
    }
    private IEnumerator BlinkWhileEnergyCoroutine()
    {
        CurrentSpriteRenderer.color = energyColor;
        yield return new WaitForSeconds(blinkInterval);

        CurrentSpriteRenderer.color = defaultColor;
        yield return new WaitForSeconds(blinkInterval);
    }
    private IEnumerator BlinkWhileAmmoCoroutine()
    {
        CurrentSpriteRenderer.color = ammoColor;
        yield return new WaitForSeconds(blinkInterval);

        CurrentSpriteRenderer.color = defaultColor;
        yield return new WaitForSeconds(blinkInterval);
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
            StartCoroutine(BlinkWhileHealingCoroutine());
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
        StartCoroutine(BlinkWhileAmmoCoroutine());
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
        //audioManager.PlaySound(meterRecoverySound);
        currentSpecialEnergyMeter_UseProperty += meterToRestore;
        StartCoroutine(BlinkWhileEnergyCoroutine());
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
        STATE_IDLE_BR,
        STATE_RUNNING_BR,
        STATE_JUMPING_BR,
        STATE_WALLSLIDING_BR,
        STATE_WALLJUMPING_BR,
        STATE_DAMAGED_BR,
        STATE_GROUND_DASHING_BR,
        STATE_GROUND_DASHING_CD_BR,
        STATE_BRAVE_STRIKE_BR,
        STATE_BRAVE_STRIKE_CDN_BR,
        STATE_FIRST_ATTACK_BR,
        STATE_SECOND_ATTACK_BR,
        STATE_THIRD_ATTACK_BR,
        STATE_FIRST_ATTACK_CD_SM,
        STATE_FIRST_JUMPING_ATTACK_BR,
        STATE_SECOND_JUMPING_ATTACK_BR,
        STATE_THIRD_JUMPING_ATTACK_BR,
        STATE_JUMPING_ATTACK_CD_BR,
        STATE_UP_KICK_READY_BR,
        STATE_UP_KICK_ATTACK_BR,
        STATE_DOWN_KICK_READY_BR,
        STATE_DOWN_KICK_ATTACK_BR,
        STATE_BRAVE_KICK_CD_BR,
        STATE_BRAVE_LAUNCHER_BR,
        STATE_BRAVE_SLAM_BR,
        STATE_BRAVE_BURST_BR,
        STATE_KINZECTER_ACTION_BR,
        STATE_KINZECTER_RECALL_BR,

        STATE_IDLE_BMB,
        STATE_RUNNING_BMB,
        STATE_JUMPING_BMB,
        STATE_WALLSLIDING_BMB,
        STATE_WALLJUMPING_BMB,
        STATE_DAMAGED_BMB,
        STATE_DASHING_BMB,
        STATE_SHOOTING_IDLE_BMB,
        STATE_SHOOTING_RUNNING_BMB,
        STATE_SHOOTING_JUMPING_BMB,
    }
}