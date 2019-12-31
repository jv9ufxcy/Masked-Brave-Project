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

    [SerializeField] private int strikeCost = 5, launcherCost = 0, shootCost = 0, burstCost = 15;

    private int maxNumberOfBullets = 3;
    private int currentNumberOfBullets_UseProperty;
    private int numberOfDashes_UseProperty = 1;
    private int minimumNumberOfDashes = 1;

    [Space]
    [Header("Movement")]

    [SerializeField] private float movementSpeed = 8f;
    [SerializeField] float jumpStrength = 14f;
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
    [SerializeField] private float minJumpPressure = 10f;
    [SerializeField] private float maxJumpPressure = 18f;
    [SerializeField] private float jumpChargeMultiplier = 12f;
    private bool shouldChargeJump;
    private bool hasReleasedJump=false;
    private bool hasJumpChargingStarted = false;
    private bool hasReachedMaxJump = false;
    
    [Space]
    [Header("Dashing")]
    private float dashTimer;
    [SerializeField] private float maxDashTime = 0.2f, airDashTime = 0.3f, groundDashTime = 0.15f, braveDashSpeed = 24f, bombDashSpeed = 40f;
    [SerializeField] private int dashCounter;
    [SerializeField] private int maxDashCounter = 3;
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
    [Header("Swordmaster Attaacking Stats")]

    private bool isAttackKeyDown, isAttackKeyUp, isFirstAttackKeyDown, isSecondAttackKeyDown, isThirdAttackKeyDown, isTransformKeyDown, isSpecialKeyDown, isNeutralSpecialKeyDown, isDownSpecialKeyDown, isUpSpecialKeyDown, isBurstKeyDown, isBraveStrikeKeyDown, isMeterBurnKeyDown;
    private bool hasAttacked;

    [SerializeField] private float attackTimer;
    [SerializeField] private float attackCooldown = 0.45f, secondSlashCooldown = 0.45f, thirdSlashCooldown = 0.7f, specialAttackCooldown = 0.8f, shootingCooldown = 0.1f, burstCooldown = 0.5f;
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

    [SerializeField]
    private string braveDash = "BraveDash", braveDamaged = "BraveDamaged", braveHenshin = "Henshin", braveSlash1 = "BraveSlash1", braveSlash2 = "BraveSlash2", braveSlash3 = "BraveSlash3", braveASlash1 = "BraveASlash1",
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
        CheckWhichControllersAreConnected();
        _state = PlayerState.STATE_IDLE_BR;
        //GetComponents
        CurrentSpriteRenderer = GetComponent<SpriteRenderer>();
        myRB = GetComponent<Rigidbody2D>();
        currentAnim = GetComponent<Animator>();
        playerRespawn = GetComponent<RespawnPlayer>();
        _globalVars = GameObject.FindGameObjectWithTag("GV").GetComponent<GlobalVars>();
        //kinzecter = GetComponent<Kinzecter>();
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
        UpdateInput();
        DebugCommands();
        OnDeath();
        GetMovementInput();
        GetJumpInput();
        GetDashInput();
        GetTransformInput();
        GetAttackInput();
    }

    private void DebugCommands()
    {
#if UNITY_EDITOR

        if (Input.GetButtonDown(touchpadButtonName))
        {
            FullHeal();
        }
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

                GroundAttackInputs();
                DashingBrave();
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
                GroundAttackInputs();
                DashingBrave();
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
                    hasAttacked = false;
                    attackTimer = specialAttackCooldown;
                    _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    SpendMeter(burstCost);
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_BURST_BR;
                }
                if (isNeutralSpecialKeyDown)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_KINZECTER_ACTION_BR;
                }
                else if (isDownSpecialKeyDown)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_BR;
                }
                if (upSlashReady)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_UP_KICK_READY_BR;
                }
                if (downSlashReady)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_DOWN_KICK_READY_BR;
                }
                if (isFirstAttackKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_JUMPING_ATTACK_BR;
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
                    hasAttacked = false;
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
                        attackTimer = specialAttackCooldown;
                        hasAttacked = false;
                        _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                    }
                    if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                    {
                        SpendMeter(burstCost);
                        hasAttacked = false;
                        _state = PlayerState.STATE_BRAVE_BURST_BR;
                    }
                    if (upSlashReady)
                    {
                        wallJumpTimer = 0;
                        hasAttacked = false;
                        _state = PlayerState.STATE_UP_KICK_READY_BR;
                    }
                    if (downSlashReady)
                    {
                        wallJumpTimer = 0;
                        hasAttacked = false;
                        _state = PlayerState.STATE_DOWN_KICK_READY_BR;
                    }
                    if (isFirstAttackKeyDown && !anySlashReady)
                    {
                        wallJumpTimer = 0;
                        hasAttacked = false;
                        attackTimer = attackCooldown;
                        _state = PlayerState.STATE_JUMPING_ATTACK_BR;
                    }
                    else if (isDownSpecialKeyDown)
                    {
                        hasAttacked = false;
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
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_WALLSLIDING_BMB;
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
                if (shouldJump == true || !isOnGround)
                {
                    dashTimer = 0;
                    _state = PlayerState.STATE_JUMPING_BR;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    dashTimer = 0;
                    SpendMeter(burstCost);
                    hasAttacked = false;
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
                    if (!hasAttacked)
                    {
                        currentAnim.Play(braveSlash1);
                        audioManager.PlaySound(attackingSound);
                        hasAttacked = true;
                    }
                    if (isFirstAttackKeyDown)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = secondSlashCooldown;
                        _state = PlayerState.STATE_SECOND_ATTACK_BR;
                    }
                    GroundSpeciaLinks();
                }
                break;
            case PlayerState.STATE_SECOND_ATTACK_BR:
                if (attackTimer >= 0)
                {
                    if (!hasAttacked)
                    {
                        currentAnim.Play(braveSlash2);
                        audioManager.PlaySound(attackingSound);
                        hasAttacked = true;
                    }
                    if (isFirstAttackKeyDown)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = thirdSlashCooldown;
                        _state = PlayerState.STATE_THIRD_ATTACK_BR;
                    }
                    GroundSpeciaLinks();
                }
                break;
            case PlayerState.STATE_THIRD_ATTACK_BR:
                if (attackTimer>=0)
                {
                    if (!hasAttacked)
                    {
                        currentAnim.Play(braveSlash3);
                        audioManager.PlaySound(comboA2Sound);
                        hasAttacked = true;
                    }
                    GroundSpeciaLinks();
                }
                break;
            case PlayerState.STATE_JUMPING_ATTACK_BR:
                if (attackTimer >= 0)
                {
                    if (!hasAttacked)
                    {
                        currentAnim.Play(braveASlash1);
                        audioManager.PlaySound(attackingSound);
                        hasAttacked = true;
                    }
                    if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
                    {
                        TurnAround();
                        SpendMeter(strikeCost);
                        hasAttacked = false;
                        attackTimer = specialAttackCooldown;
                        _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                    }
                    if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                    {
                        attackTimer = 0;
                        SpendMeter(burstCost);
                        hasAttacked = false;
                        _state = PlayerState.STATE_BRAVE_BURST_BR;
                    }
                    else if (isDownSpecialKeyDown && !isOnGround)
                    {
                        TurnAround();
                        hasAttacked = false;
                        attackTimer = 0;
                        _state = PlayerState.STATE_BRAVE_SLAM_BR;
                    }
                }
                    break;
            #endregion
            #region BraveAttacks
            case PlayerState.STATE_BRAVE_STRIKE_BR:
                if (attackTimer>=0)
                {
                    if (!hasAttacked)
                    {
                        IsInvulnerable = true;
                        isBraveStrikeKeyDown = false;
                        currentAnim.Play(braveStrike);
                        audioManager.PlaySound(riderPunchSound);
                    }
                }
                break;
            case PlayerState.STATE_UP_KICK_READY_BR:
                if (isSpecialKeyDown)
                    _state = PlayerState.STATE_UP_KICK_ATTACK_BR;
                else if (isDownSpecialKeyDown)
                {
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_BR;
                }

                if (isFirstAttackKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_JUMPING_ATTACK_BR;
                }
                if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
                {
                    TurnAround();
                    SpendMeter(strikeCost);
                    hasAttacked = false;
                    attackTimer = specialAttackCooldown;
                    _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    attackTimer = 0;
                    SpendMeter(burstCost);
                    hasAttacked = false;
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
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_SLAM_BR;
                }

                if (isFirstAttackKeyDown)
                {
                    hasAttacked = false;
                    attackTimer = attackCooldown;
                    _state = PlayerState.STATE_JUMPING_ATTACK_BR;
                }
                if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
                {
                    TurnAround();
                    SpendMeter(strikeCost);
                    hasAttacked = false;
                    attackTimer = specialAttackCooldown;
                    _state = PlayerState.STATE_BRAVE_STRIKE_BR;
                }
                if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    attackTimer = 0;
                    SpendMeter(burstCost);
                    hasAttacked = false;
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
                isFirstAttackKeyDown = false;
                upSlashReady = false;
                downSlashReady = false;
                break;
            case PlayerState.STATE_BRAVE_LAUNCHER_BR:
                if (attackTimer >= 0)
                {
                    if (!hasAttacked)
                    {
                        if (!hasAttacked)
                        {
                            SpendMeter(launcherCost);
                            currentAnim.Play(braveLauncher);
                            audioManager.PlaySound(attackingSound);
                            hasAttacked = true;
                        }
                    }
                    if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                    {
                        attackTimer = 0;
                        SpendMeter(burstCost);
                        hasAttacked = false;
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
                    if (!hasAttacked)
                    {
                        SpendMeter(launcherCost);
                        currentAnim.Play(braveSlam);
                        audioManager.PlaySound(comboA2Sound);
                        hasAttacked = true;
                    }
                }
                else if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
                {
                    attackTimer = 0;
                    SpendMeter(burstCost);
                    hasAttacked = false;
                    _state = PlayerState.STATE_BRAVE_BURST_BR;
                }
                else
                {
                    attackTimer = 0;
                    ReturnToIdleState();
                }
                break;
            case PlayerState.STATE_BRAVE_BURST_BR:
                if (!coroutineStarted)
                {
                    audioManager.PlaySound(deathSound);

                    if (isOnGround)
                        currentAnim.Play(braveBurst);
                    else
                        currentAnim.Play(braveABurst);
                }
                break;
            case PlayerState.STATE_KINZECTER_ACTION_BR:
                currentAnim.Play(braveFlourish);
                KinzecterThrow();
                ReturnToIdleState();
                break;
            case PlayerState.STATE_KINZECTER_RECALL_BR:
                break;
            #endregion
            #endregion
            #region BombadierStates
            case PlayerState.STATE_IDLE_BMB:
                HandleCharging();
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

                if (isFirstAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_IDLE_BMB;
                }

                if (isDashKeyDown && !hasDashed)
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
                if (isFirstAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_RUNNING_BMB;
                }
                if (isDashKeyDown && !hasDashed)
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
                if (isFirstAttackKeyDown)
                {
                    attackTimer = shootingCooldown;
                    _state = PlayerState.STATE_SHOOTING_JUMPING_BMB;
                }
                if (isDashKeyDown && !hasDashed)
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
                if (isTransformKeyDown)
                {
                    Henshin();
                    _state = PlayerState.STATE_WALLJUMPING_BR;
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

                    if (isFirstAttackKeyDown && horizontalInput != 0)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_RUNNING_BMB;
                    }
                    else if (isFirstAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_BMB;
                    }
                    else if (!isFirstAttackKeyDown && horizontalInput != 0)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_RUNNING_BMB;
                    }
                    else if (!isFirstAttackKeyDown && shouldJump)
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

                    if (isFirstAttackKeyDown && horizontalInput == 0)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_IDLE_BMB;
                    }
                    else if (isFirstAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_BMB;
                    }
                    else if (!isFirstAttackKeyDown && horizontalInput != 0)
                    {
                        attackTimer = 0;
                        _state = PlayerState.STATE_RUNNING_BMB;
                    }
                    else if (!isFirstAttackKeyDown && shouldJump)
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

                    if (isFirstAttackKeyDown && isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_IDLE_BMB;
                    }
                    else if (isFirstAttackKeyDown && !isOnGround)
                    {
                        attackTimer = shootingCooldown;
                        _state = PlayerState.STATE_SHOOTING_JUMPING_BMB;
                    }
                    else if (!isFirstAttackKeyDown && isOnGround)
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

        UpdateIsOnGround();
        UpdateIsOnWall();
        UpdateIsTargetReady();
        PassAnimationStats();
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
                Move();
                
                break;
            case PlayerState.STATE_JUMPING_BR:
                Jump();
                BetterJump();
                Move();
                
                break;
            case PlayerState.STATE_WALLSLIDING_BR:
                WallSliding();
                break;
            case PlayerState.STATE_WALLJUMPING_BR:
                WallJump();
                BetterWallJump();
                break;
            case PlayerState.STATE_DAMAGED_BR:
                Move();
                break;
            case PlayerState.STATE_GROUND_DASHING_BR:
                dashTimer += Time.fixedDeltaTime;
                if (dashTimer >= maxDashTime)
                {
                    dashTimer = maxDashTime;
                    _state = PlayerState.STATE_GROUND_DASHING_CD_BR;
                }
                break;
            case PlayerState.STATE_GROUND_DASHING_CD_BR:
                dashTimer -= Time.fixedDeltaTime;
                break;
            #endregion
            #region AttackingStates
            case PlayerState.STATE_FIRST_ATTACK_BR:

                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    myRB.velocity = new Vector2(myRB.velocity.x / 2, myRB.velocity.y);
                }
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_SECOND_ATTACK_BR:

                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    myRB.velocity = new Vector2(myRB.velocity.x / 2, myRB.velocity.y);
                }
                    ReturnToIdleState();
                break;
            case PlayerState.STATE_THIRD_ATTACK_BR:
                    
                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    isFirstAttackKeyDown = false;
                }

                if (shouldJump == true || !isOnGround)
                {
                    attackTimer = 0;
                    _state = PlayerState.STATE_JUMPING_BR;
                }

                    ReturnToIdleState();
                break;
            case PlayerState.STATE_JUMPING_ATTACK_BR:
                Move();
                BetterJump();
                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    isFirstAttackKeyDown = false;
                    if (isOnGround)
                    {
                        myRB.velocity = new Vector2(0, myRB.velocity.y);
                    }
                    
                }

                    BetterJump();
                    ReturnToIdleState();

                break;
            #endregion
            #region BraveAttacks
            case PlayerState.STATE_BRAVE_STRIKE_BR:
                
                BetterJump();
                
                if (attackTimer >= 0)
                {
                    attackTimer -= Time.deltaTime;
                    if (!hasAttacked)
                    {
                        SpendMeter(strikeCost);
                        myRB.velocity = new Vector2(0, 0);
                        
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
                    IsInvulnerable = false;
                    attackTimer = 0;
                    ReturnToIdleState();
                }
                break;

            case PlayerState.STATE_BRAVE_LAUNCHER_BR:

                if (attackTimer >= 0)
                {
                    attackTimer -= Time.fixedDeltaTime;
                    myRB.velocity = new Vector2(myRB.velocity.x / 2, myRB.velocity.y);
                }
                ReturnToIdleState();
                break;
            case PlayerState.STATE_BRAVE_SLAM_BR:
                Move();
                if (!isOnGround)
                {
                    myRB.AddForce(-braveSlamSpeed, ForceMode2D.Impulse);
                }
                break;
            case PlayerState.STATE_UP_KICK_READY_BR:
                Jump();
                BetterJump();
                Move();
                
                break;
            case PlayerState.STATE_UP_KICK_ATTACK_BR:
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
                }
                break;
            case PlayerState.STATE_DOWN_KICK_READY_BR:
                Jump();
                BetterJump();
                Move();
                break;
            case PlayerState.STATE_DOWN_KICK_ATTACK_BR:
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
                }
                break;
            case PlayerState.STATE_BRAVE_KICK_CD_BR:
                attackTimer -= Time.fixedDeltaTime;
                myRB.gravityScale = SavedGravity;
                ReturnToIdleState();
                break;
            case PlayerState.STATE_BRAVE_BURST_BR:
                if (!coroutineStarted)
                    BraveBurst();
                break;
            case PlayerState.STATE_KINZECTER_ACTION_BR:
                break;
            #endregion
            #endregion

            #region TricksterStates
            #region TricksterMovementStates
            case PlayerState.STATE_IDLE_BMB:
                
                break;
            case PlayerState.STATE_RUNNING_BMB:
                Move();
                break;
            case PlayerState.STATE_JUMPING_BMB:
                Jump();
                BetterJump();
                Move();
               
                break;
            case PlayerState.STATE_WALLSLIDING_BMB:
                WallSliding();
                break;
            case PlayerState.STATE_WALLJUMPING_BMB:
                WallJump();
                BetterWallJump();
                break;
            case PlayerState.STATE_DAMAGED_BMB:
                Move();
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
                Move();
                break;
            case PlayerState.STATE_SHOOTING_JUMPING_BMB:
                Move();
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
            {
                _state = PlayerState.STATE_RUNNING_BR;
            }
            if (horizontalInput == 0)
            {
                _state = PlayerState.STATE_IDLE_BR;
            }
        }
        upSlashReady = false;
        downSlashReady = false;
    }
    private void GroundAttackInputs()
    {
        if (isFirstAttackKeyDown)
        {
            hasAttacked = false;
            attackTimer = attackCooldown;
            _state = PlayerState.STATE_FIRST_ATTACK_BR;
        }
        if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
        {
            SpendMeter(strikeCost);
            attackTimer = specialAttackCooldown;
            hasAttacked = false;
            _state = PlayerState.STATE_BRAVE_STRIKE_BR;
        }
        if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
        {
            SpendMeter(burstCost);
            hasAttacked = false;
            _state = PlayerState.STATE_BRAVE_BURST_BR;
        }
        if (isNeutralSpecialKeyDown)
        {
            hasAttacked = false;
            _state = PlayerState.STATE_KINZECTER_ACTION_BR;
        }
        if (isDownSpecialKeyDown)
        {
            hasAttacked = false;
            attackTimer = attackCooldown;
            _state = PlayerState.STATE_BRAVE_LAUNCHER_BR;
        }
    }
    private void GroundSpeciaLinks()
    {
        if (isBraveStrikeKeyDown && CurrentSpecialEnergyMeter >= strikeCost)
        {
            TurnAround();
            SpendMeter(strikeCost);
            hasAttacked = false;
            attackTimer = specialAttackCooldown;
            _state = PlayerState.STATE_BRAVE_STRIKE_BR;
        }
        if (isBurstKeyDown && CurrentSpecialEnergyMeter >= burstCost)
        {
            attackTimer = 0;
            SpendMeter(burstCost);
            hasAttacked = false;
            _state = PlayerState.STATE_BRAVE_BURST_BR;
        }
        else if (isDownSpecialKeyDown)
        {
            TurnAround();
            hasAttacked = false;
            attackTimer = attackCooldown;
            _state = PlayerState.STATE_BRAVE_LAUNCHER_BR;
        }

        if (shouldJump == true || !isOnGround)
        {
            attackTimer = 0;
            _state = PlayerState.STATE_JUMPING_BR;
        }
    }
    private void DashingBrave()
    {
        if (isDashKeyDown)
        {
            //Right
            if (CanAimRight)
            {
                myRB.velocity = new Vector2(braveDashSpeed, 0);
                audioManager.PlaySound("Dash");
                currentAnim.Play(braveDash);
            }
            //Left    
            else if (!CanAimRight)
            {
                myRB.velocity = new Vector2(-braveDashSpeed, 0);
                audioManager.PlaySound("Dash");
                currentAnim.Play(braveDash);
            }
            NumberOfDashes--;
            _state = PlayerState.STATE_GROUND_DASHING_BR;
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
        if (Input.GetButtonDown(bottomFaceButtonName) && isOnWall/* && isAtPeakJumpHeight*/)
        {
            shouldWallJump = true;
        }

        if (verticalInput == -1 && isOnGround)
        {
            shouldChargeJump = true;
        }
        else
            shouldChargeJump = false;
        //shouldChargeJump = Input.GetButton(bottomFaceButtonName);
        
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
        if (Input.GetAxis(rightTriggerName) == 1 || Input.GetButtonDown(rightTriggerName))
            isMeterBurnKeyDown = true;
        else
            isMeterBurnKeyDown = false;

        isSpecialKeyDown = (Input.GetButtonDown(topFaceButtonName));

        if (isMeterBurnKeyDown)
        {
            if (isSpecialKeyDown)
                isBurstKeyDown = true;
            else
                isBurstKeyDown = false;

            if (isAttackKeyDown)
                isBraveStrikeKeyDown = true;
            else
                isBraveStrikeKeyDown = false;
        }
        else
        {
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

            if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
                isFirstAttackKeyDown = true;
            else
                isFirstAttackKeyDown = false;

            if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName) && _state == PlayerState.STATE_FIRST_ATTACK_BR && isOnGround)
            {
                isSecondAttackKeyDown = true;
            }
            else
                isSecondAttackKeyDown = false;
            if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName) && _state == PlayerState.STATE_SECOND_ATTACK_BR && isOnGround)
            {
                isThirdAttackKeyDown = true;
            }
            else
                isThirdAttackKeyDown = false;
        }

        //Buster
        if (Input.GetButton(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
        {
            shouldChargeBuster = true;
        }
        else
            shouldChargeBuster = false;

        if (Input.GetButtonDown(leftFaceButtonName) || Input.GetButtonDown(rightBumperName))
            isAttackKeyDown = true;
        else
            isAttackKeyDown = false;

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
            currentAnim.SetFloat("Speed", Mathf.Abs(horizontalInput));
            if (shouldMove)
            {
                myRB.velocity = new Vector2(horizontalInput * movementSpeed, myRB.velocity.y);
                TurnAround();
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
            StopCoroutine(DisableMovement(0f));
            StartCoroutine(DisableMovement(blinkInterval));
            canWallSlide = false;
            myRB.gravityScale = SavedGravity;
            //shouldMove = false;
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
        //ChargeJump();
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
                ShowAfterImage();
                currentAnim.SetBool("Ground", false);
                currentAnim.SetFloat("vSpeed", myRB.velocity.y);
                hasReleasedJump = true;
                jumpPressure = jumpPressure + minJumpPressure;
                myRB.AddForce(new Vector2(0f, jumpPressure), ForceMode2D.Impulse);
                jumpPressure = 0f;
                audioManager.PlaySound(jumpSound);
                hasReachedMaxJump = false;
                chargeJumpParticle.Stop();
                chargeJumpMaxColor.Stop();
            }
            else if (!shouldChargeJump)
            {
                jumpPressure = 0;
                chargeJumpParticle.Stop();
                chargeJumpMaxColor.Stop();
            }
            else
            {
                currentAnim.SetBool("Ground", false);
                currentAnim.SetFloat("vSpeed", myRB.velocity.y);
                myRB.AddForce(jumpForce, ForceMode2D.Impulse);
                audioManager.PlaySound(jumpSound);

                isOnGround = false;
                shouldJump = false;
                jumpPressure = 0;
                chargeJumpParticle.Stop();
                chargeJumpMaxColor.Stop();
            }
        }
        else
        {
            jumpPressure = 0;
            chargeJumpParticle.Stop();
            chargeJumpMaxColor.Stop();
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
                chargeJumpMaxColor.Play();
                hasReachedMaxJump = true;
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
    private void PassAnimationStats()
    {
        currentAnim.SetFloat("vSpeed", myRB.velocity.y);
        currentAnim.SetFloat("hSpeed", myRB.velocity.x);
        currentAnim.SetBool("IsSwordmaster", isSwordmaster);
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
        if (NumberOfDashes <= 0 && CurrentNumberOfBullets > 0)
        {
            SpendAmmo(1);
            if (CurrentNumberOfBullets <= 0)
            {
                hasDashed = true;
            }
        }
        else
            hasDashed = true;

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
    #endregion

    private void Henshin()
    {
        audioManager.PlaySound(henshinSound);
        isSwordmaster = !isSwordmaster;
        currentAnim.SetBool("IsSwordmaster", isSwordmaster);
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
    private void FreezeTime()
    {
        Camera.main.transform.GetComponent<FreezeTime>().FreezeFrame(damageScreenFreezeTime);
    }
    #endregion
    #region Damage
    public void TakeDamage(int damageToGive)
    {
        if (!IsInvulnerable)
        {
            if (isSwordmaster)
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
            FreezeTime();
            RecoveryPoints -= damageToGive;
            audioManager.PlaySound(takeDamageSound);
            OnDeath();
            damageParticle.Play();
            //start damage cooldown
            StartCoroutine(DamageCooldownCoroutine());
        }
    }
    public void KillPlayer(int damageToGive)
    {
        if (isSwordmaster)
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
        _state = PlayerState.STATE_JUMPING_BMB;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(groundDashTime);
        if (isOnGround)
            hasDashed = false;
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
    private IEnumerator DamageCooldownCoroutine()
    {
        IsInvulnerable = true;
        StartCoroutine(BlinkWhileInvulnerableCoroutine());
        yield return new WaitForSeconds(damageCooldownInSeconds);
        IsInvulnerable = false;
    }

    IEnumerator BraveBurstTimer()
    {
        
        coroutineStarted = true;
        IsInvulnerable = true;
        myRB.velocity = Vector2.zero;
        PlayFlashParticle(energyColor);
        Screenshake();
        

        yield return new WaitForSeconds(burstCooldown);
        IsInvulnerable = false;

        coroutineStarted = false;
        _state = PlayerState.STATE_JUMPING_BR;
    }

    private IEnumerator BlinkWhileInvulnerableCoroutine()
    {
        while (IsInvulnerable)
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
        STATE_SECOND_ATTACK_CD_SM,
        STATE_THIRD_ATTACK_CD_SM,
        STATE_JUMPING_ATTACK_BR,
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