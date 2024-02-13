using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Experimental.XR;
using System;
using System.Linq;

public class CharacterObject : MonoBehaviour, IHittable
{
    [Header("Movement")]
    public Vector2 velocity;

    public float gravity = -0.01f;
    public float aniMoveSpeed;
    [SerializeField] private float _direction = 1;
    [SerializeField] private Vector2 lastDir;

    [HideInInspector] public Rigidbody2D myRB;
    [HideInInspector] public BoxCollider2D boxCollider2D;
    [HideInInspector] public Controller2D controller;
    public RadialMenuController henshin;
    [HideInInspector] public AfterImagePool[] afterImage;
    [HideInInspector] public HealthManager healthManager;
    [HideInInspector] public AudioManager audioManager;

    [Header("CurrentState")]
    public int currentState;
    public float currentStateTime;
    public float prevStateTime;

    [Header("CharacterModel")]
    public CharacterObject characterObject;
    public GameObject character;
    public GameObject draw;
    public Animator characterAnim;
    public RuntimeAnimatorController[] formAnims;
    [HideInInspector] public SpriteRenderer spriteRend;
    public Material defaultMat, whiteMat;
    private Color flashColor = new Color ( 0,0.5f,0.75f,1f);
    public GameObject kinzecter;
    public enum ControlType { AI, PLAYER, BOSS, DEAD, OBJECT, DUMMY };
    public ControlType controlType;
    private CharacterObject playerChar;

    [Header("HitCancel")]
    public Hitbox hitbox;
    [HideInInspector] public bool canCancel;
    [HideInInspector] public bool isHit;
    [HideInInspector] public int hitConfirm;

    public InputBuffer inputBuffer = new InputBuffer();

    // Use this for initialization
    void Awake()
    {
        myRB = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        controller = GetComponent<Controller2D>();
        spriteRend = characterAnim.gameObject.GetComponent<SpriteRenderer>();
        healthManager = GetComponent<HealthManager>();
    }
    void Start()
    {
        defaultMat = spriteRend.material;
        hasLanded = true;
        RendererCheck();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
        switch (controlType)
        {
            case ControlType.AI:
                break;
            case ControlType.PLAYER:
                chargeLoop = FMODUnity.RuntimeManager.CreateInstance(chargeEvent);
                break;
            case ControlType.BOSS:
                break;
            case ControlType.DEAD:
                break;
            case ControlType.OBJECT:
                break;
            case ControlType.DUMMY:
                break;
            default:
                break;
        }
    }

    private void RendererCheck()
    {
        if (lineRend != null)
        {
            lineRend.transform.SetParent(null);
            lineRend.transform.position = Vector3.zero;
            lineRend.enabled = false;
        }
        if (trailRend != null)
        {
            trailRend.time = 0;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        switch (controlType)
        {
            case ControlType.AI:
                isAggroRange = (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) <= aggroDistance) &&
                    (Mathf.Abs(transform.position.y - GameEngine.gameEngine.mainCharacter.transform.position.y) <= aggroHeight);

                isLongRange = (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) <= longAttackRange &&
                    (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x)) > shortAttackRange)&& 
                    (Mathf.Abs(transform.position.y - GameEngine.gameEngine.mainCharacter.transform.position.y) <= aggroHeight);

                isShortRange = (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) <= shortAttackRange) &&
                    (Mathf.Abs(transform.position.y - GameEngine.gameEngine.mainCharacter.transform.position.y) <= aggroHeight);

                break;
            case ControlType.BOSS:
                isAggroRange = Vector3.Distance(transform.position, GameEngine.gameEngine.mainCharacter.transform.position) <= aggroDistance;
                isLongRange = (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) <= longAttackRange &&
                    (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x)) > shortAttackRange);
                isShortRange = (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) <= shortAttackRange);
                break;
            case ControlType.PLAYER:

                if (!DialogueManager.instance.isDialogueActive)
                    PauseMenu();

                //if (CanUnCrouch() && GameEngine.gameEngine.globalMovelistIndex != 4)
                //    Henshin();

                if (!PauseManager.IsGamePaused && !DialogueManager.instance.isDialogueActive)
                {
                    JumpCut();
                    DashCut();
                    ChargeAttack();
                }
                else
                    StopChargeAttack();
                ChargeAttackAudio();
                //else
                //{
                //    chargeLoop.getPlaybackState(out chargeState);
                //    if (chargeState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
                //        chargeLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                //}

                leftStick = new Vector2(Input.GetAxis(GameEngine.coreData.rawInputs[13].name), Input.GetAxis(GameEngine.coreData.rawInputs[14].name));
                break;

            case ControlType.DUMMY:
                isAggroRange = Vector3.Distance(transform.position, GameEngine.gameEngine.mainCharacter.transform.position) <= aggroDistance;
                isLongRange = (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) <= longAttackRange &&
                    (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x)) > shortAttackRange);
                isShortRange = (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) <= shortAttackRange);
                break;
            default:
                break;
        }
    }
    void FixedUpdate()
    {
        if (!PauseManager.IsGamePaused && !DialogueManager.instance.isDialogueActive)
        {
            if (GameEngine.hitStop <= 0)
            {
                //UpdateInputBuffer

                //Update Input
                //HitCancel();
                switch (controlType)
                {
                    case ControlType.AI:
                        UpdateAI();
                        break;
                    case ControlType.BOSS:
                        UpdateAI();
                        break;
                    case ControlType.PLAYER:
                        UpdateInput();
                        break;
                    case ControlType.DUMMY:
                        UpdateAI();
                        break;
                    default:
                        break;
                }

                //Update State Machine
                UpdateState();
                //Update Physcis
                UpdatePhysics();
                //
            }
            UpdateTimers();
        }
        UpdateAnimator();
    }
    public void UpdateCharacter()
    {
        UpdateState();
        //Update Physcis
        UpdatePhysics();
        //
        UpdateTimers();
        UpdateAnimator();
    }
    void UpdateTimers()
    {
        if (dashCooldown > 0) { dashCooldown -= dashCooldownRate; }
        if (invulCooldown > 0) { invulCooldown --; }
        else { isInvulnerable = false; curComboValue = -1; }
    }

    [HideInInspector] public float animSpeed;
    void UpdateAnimator()
    {
        animSpeed = 1;
        if (GameEngine.hitStop > 0)
        {
            animSpeed = 0;
        }

        Vector2 latSpeed = new Vector2(velocity.x, 0);
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed);
        animFallSpeed = velocity.y /** 30f*/;
        characterAnim.SetFloat("moveSpeed", aniMoveSpeed);
        characterAnim.SetFloat("aerialState", animAerialState);
        //characterAnim.SetBool("wallState", wallFlag);
        characterAnim.SetFloat("fallSpeed", animFallSpeed);
        characterAnim.SetFloat("hitAnimX", curHitAnim.x);
        characterAnim.SetFloat("hitAnimY", curHitAnim.y);
        characterAnim.SetFloat("animSpeed", animSpeed);

    }
    void FaceStick()
    {
        //if (CheckVelocityDeadZone())
        {
            if (leftStick.x > 0) { Direction = 1; transform.localScale = new Vector3(1f, 1f, 1f); }
            else if (leftStick.x < 0) { Direction = -1; transform.localScale = new Vector3(-1f, 1f, 1f); }
            
            switch (controlType)
            {
                case ControlType.AI:
                    lastDir = (target.transform.position - transform.position).normalized;
                    break;
                case ControlType.PLAYER:
                    lastDir = leftStick.normalized;
                    break;
                case ControlType.BOSS:
                    lastDir = (target.transform.position - transform.position).normalized;
                    break;
                case ControlType.DEAD:
                    break;
                case ControlType.OBJECT:
                    break;
                case ControlType.DUMMY:
                    lastDir = (target.transform.position - transform.position).normalized;
                    break;
                default:
                    break;
            }
        }
        if (controlType==ControlType.AI || controlType == ControlType.BOSS || controlType == ControlType.DUMMY)
        {
            FacePlayer();
        }
    }
    void UpdateState()
    {
        CharacterState myCurrentState = GameEngine.coreData.characterStates[currentState];

        if (hitStun > 0 && controlType!=ControlType.DEAD)
        {
            GettingHit();
        }
        else
        {
            UpdateStateEvents();
            UpdateStateAttacks();

            prevStateTime = currentStateTime;
            currentStateTime++;

            if (currentStateTime >= myCurrentState.length)
            {
                if (myCurrentState.loop) { LoopState(); }
                else { EndState(); }
            }
        }
    }
    void LoopState()
    {
        currentStateTime = 0;
        //currentState = 0;
        prevStateTime = -1;
    }
    void EndState()
    {
        currentStateTime = 0;
        currentState = 0;
        prevStateTime = -1;
        StartState(currentState);
    }
    void UpdateStateEvents()
    {
        int _curEv = 0;
        foreach (StateEvent _ev in GameEngine.coreData.characterStates[currentState].events)
        {

            //if (prevStateTime <= _ev.start && currentStateTime == _ev.start)
            if (_ev.active)
            {
                if (currentStateTime >= _ev.start && currentStateTime <= _ev.end)
                {
                    DoEventScript(_ev.script, currentState, _curEv, _ev.parameters);
                }
            }
            _curEv++;
        }
    }
    [Header("CurrentAttack")]
    [HideInInspector] public float hitActive;
    [HideInInspector] public int currentAttackIndex;
    void UpdateStateAttacks()
    {
        int _cur = 0;
        foreach (AttackEvent _atk in GameEngine.coreData.characterStates[currentState].attacks)
        {
            if (currentStateTime == _atk.start)
            {
                hitbox.RestoreGetHitBools();
                hitActive = _atk.length;
                hitbox.transform.localScale = _atk.hitBoxScale;
                hitbox.transform.localPosition = _atk.hitBoxPos;
                currentAttackIndex = _cur;
            }
            if (currentStateTime == _atk.start + _atk.length)
            {
                hitActive = 0;
            }
            //HitCancel
            float cWindow = _atk.start + _atk.cancelWindow;
            if (currentStateTime >= cWindow)
                if (hitConfirm > 0)
                    canCancel = true;

            //Whiff Cancel
            if (currentStateTime >= cWindow + whiffWindow)
                canCancel = true;
            _cur++;
        }
    }
    public static float whiffWindow = 16f;
    void HitCancel()
    {
        //if (currentStateTime >= _ev.start && currentStateTime <= _ev.end)
        //{
        //foreach (AttackEvent _atk in GameEngine.coreData.characterStates[currentState].attacks)
        //{
        float cWindow = GameEngine.coreData.characterStates[currentState].attacks[currentAttackIndex].start +
            GameEngine.coreData.characterStates[currentState].attacks[currentAttackIndex].cancelWindow;

        if (currentStateTime == cWindow)
            if (hitConfirm > 0)
                canCancel = true;

        if (currentStateTime == cWindow + whiffWindow)
            canCancel = true;
    }
    void DoEventScript(int _index, int _actIndex, int _evIndex, List<ScriptParameters> _params)
    {
        if (_params == null) { return; }
        if (_params.Count <= 0) { return; }
        switch (_index)//index = element in characterscripts
        {
            case 0://Jump
                VelocityY(_params[0].val);
                break;
            case 1:
                FrontVelocity(_params[0].val);
                break;
            case 2:
                FacePlayer();
                break;
            case 3:
                StickMove(_params[0].val);
                break;
            case 4:
                GettingHit();
                break;
            case 5:
                GlobalPrefab(_params[10].val, _actIndex, _evIndex);
                break;
            case 6:
                CanCancel(_params[0].val);
                break;
            case 7:
                Jump(_params[0].val);
                break;
            case 8:
                FaceStick();
                break;
            case 9:
                AirMove(_params[0].val);
                break;
            case 10:
                FireBullet(_params[0].val, _params[1].val, _params[2].val, _params[3].val, _params[4].val, 0);
                break;
            case 11:
                Dash(_params[0].val);
                break;
            case 12:
                KinzectorActions(_params[0].val, _params[1].val, _params[2].val);
                break;
            case 13:
                AirStill(_params[0].val);
                break;
            case 14:
                PlayAudio(_params[0].name);
                break;
            case 15:
                SpawnTurret((int)_params[0].val, new Vector2(_params[1].val, _params[2].val));
                break;
            case 16:
                FireBullet(_params[0].val, _params[1].val, _params[2].val, _params[3].val, _params[4].val, _params[5].val);
                break;
            case 17:
                QuickChangeForm((int)_params[0].val);
                healthManager.lastChance = true;
                break;
            case 18:
                Screenshake(_params[0].val, _params[1].val);
                break;
            case 19:
                GrappleActions((int)_params[0].val, (int)_params[1].val);
                break;
            case 20:
                AirLoop();
                break;
            case 21:
                TargetAttack(_params[0].val, _params[1].val);
                break;
            case 22:
                MaintainVelocity();
                break;
            case 23:
                SummonSatellite((int)_params[0].val, (int)_params[1].val, _params[2].val, _params[3].val, _params[4].val);
                break;
            case 24:
                SetInvulCooldown(_params[0].val);
                break;
            case 25:
                OnSkillUsed();
                break;
            case 26:
                SelfDestruct((int)_params[0].val);
                break;
            case 27:
                JumpStateCut();
                break;
            case 28:
                DashJump();
                break;
            case 29:
                GameEngine.SetHitPause(_params[0].val);
                break;

        }
    }
    public void SelfDestruct(int index)
    {
        switch (index)
        {
            case 0://instantDeath
                Explode();
                break;
            case 1://onCollision
                if (IsGrounded() || HitCeiling() || IsOnWall())
                    Explode();
                break;
            case 2://hitConfirm
                if (hitConfirm >= 1)
                    Explode();
                break;
            case 3://hurt
                if (hitStun > 0)
                    Explode();
                break;
        }
    }
    void Explode()
    {
        GameObject bombGO = Instantiate(bullets[0], transform.position, transform.rotation);
        BombController bomb = bombGO.GetComponent<BombController>();
        bomb.character = GameEngine.gameEngine.mainCharacter;
        bomb.StartState();
        healthManager.FinisherDeath(false);
    }
    private GameObject satelliteInstance;
    public List<GameObject> satellitesCreated;
    public void SummonSatellite(int satteliteIndex, int numOfBullets, float bulletSpeed,float rotRadius, float attackIndex)
    {
        //CREATE ICE BLOCKS
        for (int i = 0; i < numOfBullets; i++)
        {
            satelliteInstance = Instantiate(bullets[satteliteIndex], transform.position, Quaternion.identity);//make prefab
            BulletHit bullet = satelliteInstance.GetComponent<BulletHit>();
            bullet.SetStartingAngle(i, numOfBullets);//tell it which angle to start at
            bullet.character = characterObject;//transform of kitty cat because why not do it the stupid way
            bullet.projectileIndex = (int)attackIndex;
            bullet.speed = bulletSpeed;
            bullet.radius = rotRadius;
            satellitesCreated.Add(satelliteInstance);//add to the list of references to ice blocks
        }
    }
    void EraseSatellites()
    {
        if (satellitesCreated!=null)
        {
            foreach (GameObject item in satellitesCreated)
            {
                satellitesCreated.Remove(item);
                Destroy(item);
            }
        }
    }
    public void MaintainVelocity()
    {
        SetVelocity(velocity);
    }
    public void AirLoop()
    {
        if (aerialFlag)
        {
            currentStateTime = prevStateTime;
            animSpeed = 0;
        }
    }
    public void GrappleActions(int actIndex, int impactState)
    {
        switch (actIndex)
        {
            case 0:
                lineRend.enabled = true;
                grappleVictim.Grappled();
                lineRend.SetPosition(0, transform.position);
                lineRend.SetPosition(1, grappleVictim.transform.position);
                break;
            case 1:
                lineRend.enabled = false;
                grappleVictim.GetHit(this, impactState, 0);
                break;
            case 2:
                lineRend.enabled = false;
                grappleVictim = null;
                break;
        }
    }
    public void DOChangeMovelist(int index)
    {
        PlayFlashParticle(henshinColors[index]);
        GameEngine.SetHitPause(5f);
        QuickChangeForm(index);
    }

    public void QuickChangeForm(int index)
    {
        GameEngine.gameEngine.ChangeMovelist(index);
        characterAnim.runtimeAnimatorController = formAnims[GameEngine.gameEngine.globalMovelistIndex];
        PlayFlashParticle(henshinColors[GameEngine.gameEngine.globalMovelistIndex]);
        if (GameEngine.gameEngine.globalMovelistIndex == 1)//bomb
            SetCrouchFlag(true);
        else
            SetCrouchFlag(false);
        if (isKinzecterOut)
        {
            kinzecter.GetComponent<Kinzecter>().RemoveKinzecter();
        }
    }

    public void ToggleMovelist()
    {
        GameEngine.SetHitPause(5f);
        GameEngine.gameEngine.ToggleMovelist();
        PlayFlashParticle(henshinColors[GameEngine.gameEngine.globalMovelistIndex]);
        characterAnim.runtimeAnimatorController = formAnims[GameEngine.gameEngine.globalMovelistIndex];

        if (GameEngine.gameEngine.globalMovelistIndex == 1)//bomb
            SetCrouchFlag(true);
        else
            SetCrouchFlag(false);
        if (isKinzecterOut)
        {
            kinzecter.GetComponent<Kinzecter>().RemoveKinzecter();
        }
    }
    private void PlayAudio(string audioName)
    {
        audioManager.PlaySound(audioName);
    }
    private void TargetAttack(float speed, float range)
    {
        switch (controlType)
        {
            case ControlType.AI:
                Transform player = GameEngine.gameEngine.mainCharacter.transform;
                Vector2 playerDir = (player.position - transform.position).normalized;
                velocity += (playerDir * speed);
                break;
            case ControlType.PLAYER:
                
                break;
            default:
                break;
        }
    }
    private void AirStill(float _pow)
    {
        if (IsGrounded())
        {
            if (_pow > 0)
            {
                VelocityY(_pow);
            }
        }
        else
            VelocityY(2f);
    }
    private void Dash(float dashSpeed)
    {
        //velocity = Vector2.zero;
        Vector2 dir = new Vector2(lastDir.x, lastDir.y);
        if (dir == Vector2.zero)
        {
            dir.x = Direction;
        }
        if (!isOnGround && dir.y == 0)
        {
            AirStill(0);
        }
        //velocity += dir.normalized * dashSpeed;
        FrontVelocity(Mathf.Abs(dir.x) * dashSpeed);
        VelocityY(dir.y * dashSpeed);
        characterAnim.SetFloat("dirAnimX", Mathf.Abs(dir.x));
        characterAnim.SetFloat("dirAnimY", dir.y);
        //StartCoroutine(DashWait());
    }
    IEnumerator DashWait()
    {
        DOVirtual.Float(velocity.x, 0, dashLength, HorizontalDrag);
        DOVirtual.Float(velocity.y, 0, dashLength, VerticalDrag);
        //ShowAfterImage(bombTrailColor, bombTrailFadeColor, shortDashInterval);
        bombDashParticle.Play();
        yield return new WaitForSeconds(.6f);
        bombDashParticle.Stop();
        //Debug.Log("Dash particle stopped");
    }
    void HorizontalDrag(float x) { velocity.x = x; }
    void VerticalDrag(float y) { velocity.y = y; }
    [Header("Wall Jumps")]
    public float maxJumpHeight = 4;
    public float minJumpHeight = .125f;
    public float timeToJumpApex = .4f;
    public float accelerationTimeAirborne = 0f, accelerationTimeGrounded = 0f;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 4;
    public float wallStickTime = .06f;
    float timeToWallUnstick;
    float maxJumpVelocity;
    float minJumpVelocity;
    bool wallSliding;
    int wallDirX;
    public void Jump(float _pow)
    {
        //velocity.y = _pow*jumpPow;
        //jumps--;

        landingParticle.Play();
        if (wallSliding)
        {
            if (wallDirX == leftStick.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (leftStick.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (!aerialFlag)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (leftStick.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
                { // not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity*_pow;
            }
        }
    }
    void CanCancel(float _val)
    {
        if (_val > 0)
        {
            canCancel = true;
        }
        else
            canCancel = false;
    }
    void GlobalPrefab(float _index, int _act, int _ev)
    {
        GameEngine.GlobalPrefab((int)_index, character, _act, _ev);
    }
    public void GlobalPrefab(float _prefab)
    {
        GlobalPrefab(_prefab, -1, -1);
    }
    public void FrontVelocity(float _pow)
    {
        velocity.x = _pow * Direction;
    }
    [Header("MovementVectors")]
    public Vector2 leftStick;
    public string horizontalAxis = "altPs4Horizontal", verticalAxis = "altPs4Vertical";
    void StickMove(float _pow)
    {
        if ((leftStick.x > deadzone || leftStick.x < -deadzone || leftStick.y > deadzone || leftStick.y < -deadzone))
        {
            float targetVelocityX;
            float _mov = 0;
            if (leftStick.x > deadzone)
            {
                _mov = 1;
            }
            if (leftStick.x < -deadzone)
            {
                _mov = -1;
            }
            if (aerialFlag)
            {
                targetVelocityX = leftStick.x * (moveSpeed*airMod) * _pow;
                velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

                //velocity.x = (airMod * _mov) * moveSpeed * _pow;
                if (Mathf.Abs(airMod) == 2)
                {
                    //ShowAfterImage();
                }
            }
            else
            {
                targetVelocityX = leftStick.x * moveSpeed * _pow;
                velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
                //velocity.x = (_mov) * moveSpeed * _pow;
            }
        }
        else
        {
            velocity.x = Mathf.SmoothDamp(velocity.x, 0, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        if (hitStun <= 0)
        {
            FaceStick();
        }
    }
    void AirMove(float _pow)
    {
        if (!IsGrounded())
        {
            if ((leftStick.x > deadzone || leftStick.x < -deadzone || leftStick.y > deadzone || leftStick.y < -deadzone))
            {
                float targetVelocityX;
                float _mov = 0;
                if (leftStick.x > deadzone)
                {
                    _mov = 1;
                }
                if (leftStick.x < -deadzone)
                {
                    _mov = -1;
                }
                //velocity.x = (airMod * _mov) * moveSpeed * _pow;
                targetVelocityX = leftStick.x * (moveSpeed * airMod) * _pow;
                velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

            }
        }
        else
        {
            velocity.x = Mathf.SmoothDamp(velocity.x, 0, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        }
        if (hitStun <= 0)
        {
            FaceStick();
        }
    }
    public void VelocityY(float _pow)
    {
        velocity.y = _pow;
    }

    public float deadzone = 0.2f;

    public float moveSpeed = 10f;
    public float airMod = 1f;
    public float jumpPow = 12;
    public void StartStateFromScript(int _newState)
    {
        StartState(_newState);
    }
    void StartState(int _newState)
    {
        currentState = _newState;
        prevStateTime = -1;
        currentStateTime = 0;
        canCancel = false;

        if (_newState == 0) { currentCommandStep = 0; }

        //Attacks
        hitActive = 0;
        hitConfirm = 0;

        UseMeter(nextSpecialMeterUse);
        nextSpecialMeterUse = 0;

        SetAnimation(GameEngine.coreData.characterStates[currentState].stateName);
        //Debug.Log("State Started: " + GameEngine.coreData.characterStates[currentState].stateName);
    }
    void SetAnimation(string animName)
    {
        characterAnim.CrossFadeInFixedTime(animName, GameEngine.coreData.characterStates[currentState].blendRate);
        //characterAnim.Play(animName);
    }

    public int currentCommandState;
    public int currentCommandStep;

    public void GetCommandState()
    {
        currentCommandState = 0;
        for (int c = 0; c < GameEngine.gameEngine.CurrentMoveList().commandStates.Count; c++)
        {
            CommandState s = GameEngine.gameEngine.CurrentMoveList().commandStates[c];
            if (s.aerial == aerialFlag)
            {
                currentCommandState = c;
                return;
            }
            //if (s.wall == wallFlag)
            //{
            //    currentCommandState = c;
            //    return;
            //}
        }
    }

    int[] cancelStepList = new int[2];

    void UpdateInput()
    {


        inputBuffer.Update();

        bool startState = false;

        GetCommandState();
        CommandState comState = GameEngine.gameEngine.CurrentMoveList().commandStates[currentCommandState];


        if (currentCommandStep >= comState.commandSteps.Count) { currentCommandStep = 0; }


        cancelStepList[0] = currentCommandStep;//base sub-state
        cancelStepList[1] = 0;
        int finalS = -1;
        int finalF = -1;
        int currentPriority = -1;
        for (int s = 0; s < cancelStepList.Length; s++)
        {
            if (comState.commandSteps[currentCommandStep].strict && s > 0) { break; }
            if (!comState.commandSteps[currentCommandStep].activated) { break; }

            for (int f = 0; f < comState.commandSteps[cancelStepList[s]].followUps.Count; f++)// (CommandStep cStep in comState.commandSteps[currentCommandStep])
            {
                CommandStep nextStep = comState.commandSteps[comState.commandSteps[cancelStepList[s]].followUps[f]];
                InputCommand nextCommand = nextStep.command;

                //if(inputBuffer.)
                if (CheckInputCommand(nextCommand))
                {
                    if (canCancel)
                    {
                        if (GameEngine.coreData.characterStates[nextCommand.state].ConditionsMet(this))
                        {
                            if (nextStep.priority > currentPriority)
                            {
                                currentPriority = nextStep.priority;
                                startState = true;
                                finalS = s;
                                finalF = f;

                            }
                        }
                    }
                }
            }
        }
        if (startState)
        {
            CommandStep nextStep = comState.commandSteps[comState.commandSteps[cancelStepList[finalS]].followUps[finalF]];
            InputCommand nextCommand = nextStep.command;
            inputBuffer.UseInput(nextCommand.input, nextCommand.motionCommand);
            if (nextStep.followUps.Count > 0) { currentCommandStep = nextStep.idIndex; }
            else { currentCommandStep = 0; }
            StartState(nextCommand.state);
        }
    }



    public bool CheckInputCommand(InputCommand _in)
    {
        if (inputBuffer.buttonCommandCheck[_in.input] < 0) { return false; }
        if (inputBuffer.motionCommandCheck[_in.motionCommand] < 0) { return false; }
        return true;
    }
    public bool CheckVelocityDeadZone()
    {
        if (velocity.x > 0.001f) { return true; }
        if (velocity.x < -0.001f) { return true; }
        if (velocity.y > 0.001f) { return true; }
        if (velocity.y < -0.001f) { return true; }
        return false;
    }
    [Space]
    [Header("Charged Slash")]

    private float shotPressure;
    [SerializeField] private float minShotPressure = 30f, maxShotPressure = 60f;
    private bool shouldChargeBuster;
    public int chargeAttackIndex = 15, chargeShotIndex = 21, critBusterIndex = 22, shotgunIndex = 40, missileIndex = 41;
    [SerializeField] private bool firstCharge, secondCharge;
    private Color c;

    public float chargeIncrement = 1f;

    private FMOD.Studio.EventInstance chargeLoop;
    private FMOD.Studio.PLAYBACK_STATE chargeState;
    [SerializeField] private FMODUnity.EventReference chargeEvent;
    void ChargeAttack()
    {
        switch (GameEngine.gameEngine.globalMovelistIndex)
        {
            case 0://Brave
                if (Input.GetButton(GameEngine.coreData.rawInputs[1].name))//name of charge Attack
                {
                    ColorCharge();
                    ChargeUp(chargeIncrement);
                }
                if (Input.GetButtonUp(GameEngine.coreData.rawInputs[1].name))
                {
                    ReleaseChargeSlash();
                }
                break;
            case 1://Bombadier
                if (Input.GetButton(GameEngine.coreData.rawInputs[1].name))//name of charge Attack
                {
                    ColorCharge();
                    ChargeUp(chargeIncrement);
                    if (shotPressure < maxShotPressure)
                    {
                        //hasShotChargingStarted = true;
                    }
                }
                if (Input.GetButtonUp(GameEngine.coreData.rawInputs[1].name))
                {
                    ReleaseChargedBuster();
                }
                break;
            case 2://Pursuer
                if (Input.GetButton(GameEngine.coreData.rawInputs[1].name))//name of charge Attack
                {
                    ColorCharge();
                    ChargeUp(chargeIncrement);
                    if (shotPressure < maxShotPressure)
                    {
                        //hasShotChargingStarted = true;
                    }
                }
                if (Input.GetButtonUp(GameEngine.coreData.rawInputs[1].name))
                {
                    ReleaseChargedArsenal();
                }
                break;
            case 3:
                break;
            case 4:
                break;
        }
    }

    private void ReleaseChargedArsenal()
    {
        if (shotPressure >= minShotPressure && shotPressure < maxShotPressure)
        {
            StartStateFromScript(shotgunIndex);
            shotPressure = 0f;
            firstCharge = false; secondCharge = false;
        }
        else if (shotPressure >= maxShotPressure)
        {
            StartStateFromScript(missileIndex);
            shotPressure = 0f;
            firstCharge = false; secondCharge = false;
        }
        StopChargeAttack();
    }
    private void ReleaseChargedBuster()
    {
        if (shotPressure >= minShotPressure && shotPressure < maxShotPressure)
        {
            BusterShooting();
            shotPressure = 0f;
            firstCharge = false; secondCharge = false;
        }
        else if (shotPressure >= maxShotPressure)
        {
            CriticalBusterShooting();
            shotPressure = 0f;
            firstCharge = false; secondCharge = false;
        }
        StopChargeAttack();
        //shotPressure = 0;
    }
    private void ReleaseChargeSlash()
    {
        if (shotPressure > maxShotPressure)
        {
            StartStateFromScript(chargeAttackIndex);
        }
        StopChargeAttack();
    }
    public void StopChargeAttack()
    {
        if (shotPressure != 0f)
        {
            //hasReleasedShot = true;
            shotPressure = 0f;
            firstCharge = false; secondCharge = false;
        }
        foreach (ParticleSystem p in primaryGunParticles)
        {
            p.startColor = Color.clear;
            p.Stop();
        }
        if (chargeState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            chargeLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }


    private void ChargeAttackAudio()
    {
        chargeLoop.getPlaybackState(out chargeState);
        if (shotPressure > minShotPressure)
        {
            if (chargeState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                chargeLoop.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                chargeLoop.start();
                //Debug.Log("Start Charge Loop");
            }
        }
        else
        {
            if (chargeState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                chargeLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    [Space]
    [Header("ParticleGroups")]
    public Transform gunChargeParticles;
    public Transform jumpChargeParticles;
    public Transform flashParticles;
    public Color[] turboColors;
    public Color[] henshinColors;
    public List<ParticleSystem> primaryGunParticles = new List<ParticleSystem>();
    public List<ParticleSystem> primaryJumpParticles = new List<ParticleSystem>();
    public List<ParticleSystem> secondaryParticles = new List<ParticleSystem>();
    public ParticleSystem bombDashParticle, landingParticle;
    [Space]
    [Header("Shooting Stats")]
    public List<GameObject> bulletsActiveInHierarchy = new List<GameObject>();
    public float shootAnim, shootAnimMax;
    public GameObject[] bullets;
    [HideInInspector] public bool isKinzecterOut;
    public void KinzectorActions(float action, float offsetX, float offsetY)
    {
        switch (action)
        {
            case 0://TryThrow
                TryKinzecterThrow(offsetX, offsetY);
                break;
            case 1://TryRecall
                TryKinzecterRecall();
                break;
            case 2://WarpSlash
                CharacterObject savedEnemy = kinzecter.GetComponent<Kinzecter>().SavedEnemy();
                Vector3 offsetDir = new Vector3(offsetX * Direction, offsetY,0) ;
                if (savedEnemy)
                {
                    FaceTarget(savedEnemy.transform.position);
                    if (trailRend != null)
                    {
                        TrailTime(.5f);
                        DOVirtual.Float(0.5f, 0, 0.5f, TrailTime).SetDelay(1f);
                    }
                    transform.DOMove(savedEnemy.transform.position + (offsetDir), 0.0833f);//5 frames
                    savedEnemy.Hit(this, currentState, 0);
                    jumps=jumpMax;
                }
                TryKinzecterRecall();
                break;
            case 3:
                break;
            case 4:
                break;
        }

    }
    void TrailTime(float x)
    {
        trailRend.time = x;
    }
    private void TryKinzecterThrow(float offsetX, float offsetY)
    {
        int onWall = wallFlag ? -1 : 1;
        if (!isKinzecterOut)
        {
            SpawnKinzecter(offsetX, offsetY);
        }
        else
        {
            if (kinzecter.GetComponent<Kinzecter>().isWithPlayer())
            {
                kinzecter.GetComponent<Kinzecter>().ThrowKinzecter(characterObject, new Vector3(Direction * onWall, 0, 0));
            }
            else
            kinzecter.GetComponent<Kinzecter>().AttackClosestEnemy();
        }
    }
    private int summonIndex = 0;
    private void SpawnTurret( int minionIndex, Vector3 pos)
    {
        minionIndex = summonIndex;
        if (spawners[minionIndex].IsSpawned)
        {
            if (spawners[minionIndex].GetComponentInChildren<MinionSpawner>().summonID==0)//already summoned
            {
                spawners[minionIndex].GetComponentInChildren<MinionSpawner>().UpgradeHouse();
            }
        }
        else
        {
            spawners[minionIndex].Spawn(0);
        }
        summonIndex++;
        if (summonIndex>spawners.Length-1)
        {
            summonIndex = 0;
        }
    }
    private void SpawnKinzecter(float offsetX, float offsetY)
    {
        var offset = new Vector3(offsetX * Direction, offsetY, 0);
        GameObject newbullet = Instantiate(bullets[5], transform.position + offset, Quaternion.identity);
        kinzecter = newbullet;

        int onWall = wallFlag ? -1 : 1;
        kinzecter.GetComponent<Kinzecter>().ThrowKinzecter(characterObject, new Vector3(Direction*onWall, 0, 0));

        //kinzecter.GetComponent<Hitbox>().character = characterObject;
        isKinzecterOut = true;
    }

    private void TryKinzecterRecall()
    {
        if (!isKinzecterOut)
        {
            SpawnKinzecter(1, 0);
        }
        else
        {
            kinzecter.GetComponent<Kinzecter>().ReturnToPlayer();
        }
    }

    public void FireBullet(float bulletType, float bulletSpeed, float offsetX, float offsetY, float attackIndex, float bulletRot)
    {
        shootAnim = shootAnimMax;
        var offset = new Vector3(offsetX * Direction, offsetY, 0);
        GameObject newbullet = Instantiate(bullets[(int)bulletType], transform.position + offset, Quaternion.identity);
        BulletHit bullet = newbullet.GetComponent<BulletHit>();
        bullet.character = characterObject;
        int onWall = wallFlag ? -1 : 1;
        bullet.direction.x = (Direction* onWall);

        bullet.velocity.x = (Direction * onWall) * Mathf.Sign(bulletSpeed);
        bullet.attackIndex = (int)attackIndex;
        bullet.speed = Mathf.Abs(bulletSpeed);
        bullet.rotation = bulletRot * (Direction * onWall);
        //newbullet.GetComponent<Hitbox>().character = characterObject;
        newbullet.transform.localScale = new Vector3((Direction * onWall) * Mathf.Sign(bulletSpeed), 1, 1);
        if (!bulletsActiveInHierarchy.Contains(newbullet))
        {
            bulletsActiveInHierarchy.Add(newbullet);
        }
    }
    public void SpecialFireCheck(float bulletType)
    {
        //if (CurrentNumberOfBullets >= 1)
        //{
        //    shotPressure = 0;
        //    FireBullet(2);

        //}
        //else
        //{
        //FireBullet(bulletType, critBusterVelocity.x,bulletSpawnPos.x, bulletSpawnPos.y);
        //}
        //TODO: add variable stored bullets to state execution check instead
    }
    public void BusterShooting()
    {
        StartStateFromScript(chargeShotIndex);
    }
    public void CriticalBusterShooting()
    {
        StartStateFromScript(critBusterIndex);
    }
    public void ColorCharge()
    {
        if (!firstCharge)
            c = Color.clear;

        if ((shotPressure >= minShotPressure && shotPressure < maxShotPressure) && !firstCharge)
        {
            foreach (ParticleSystem p in primaryGunParticles)
            {
                p.startColor = Color.clear;
                p.Play();
            }
            firstCharge = true;
            c = turboColors[0];

            PlayFlashParticle(c);
        }

        if (shotPressure >= maxShotPressure && !secondCharge)
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
        foreach (ParticleSystem p in secondaryParticles)
        {
            var pmain = p.main;
            pmain.startColor = c;
            p.Play();
        }
    }
    private int menuTimer;
    [SerializeField] private int menuDelay = 12, henshinInput=2;
    private void Henshin()
    {
        if (Input.GetButton(GameEngine.coreData.rawInputs[henshinInput].name))//open radial menu
        {
            if (menuTimer < menuDelay)
            {
                menuTimer++;
            }
        }
        if (menuTimer >= menuDelay)
        {
            //GameEngine.SetHitPause(5f);
            //henshin.ActivateMenu();
            menuTimer++;
        }
        if (Input.GetButtonUp(GameEngine.coreData.rawInputs[henshinInput].name))//open radial menu
        {
            if (menuTimer < menuDelay)
            {
                ToggleMovelist();
                menuTimer = 0;
            }
            else
            {
                //henshin.SelectForm();
                ToggleMovelist();
                menuTimer = 0;
            }
        }
    }
    public delegate void StartButtonPressed();
    public delegate void SelectButtonPressed();
    public delegate void TouchpadButtonPressed();
    public event StartButtonPressed OnStartPressed;
    public event SelectButtonPressed OnSelectPressed;
    public event SelectButtonPressed OnTouchpadPressed;
    private void PauseMenu()
    {
        if (Input.GetButtonDown(GameEngine.coreData.rawInputs[8].name))
        {
            OnStartPressed?.Invoke();
            PauseManager.pauseManager.PauseButtonPressed();
        }
        if (Input.GetButtonDown(GameEngine.coreData.rawInputs[9].name))
        {
            OnSelectPressed?.Invoke();
        }
        if (Input.GetButtonDown(GameEngine.coreData.rawInputs[10].name))
        {
            OnTouchpadPressed?.Invoke();
        }
    }
    private void Screenshake(float amp, float time)
    {
        CinemachineShake.instance.ShakeCamera(amp, time);
    }
    void JumpCut()
    {
        //if (currentState==1)//jump state
        if (aerialFlag)//jump state
        {
            if (velocity.y > 0 && Input.GetButtonUp(GameEngine.coreData.rawInputs[0].name))
            {
                VelocityY(-2);
                if (IsGrounded())
                    StartStateFromScript(0);
            }
        }
    }
    void JumpStateCut()
    {
        //if (currentState==1)//jump state
        if (currentState==1)//jump state
        {
            if (velocity.y > 0 && !Input.GetButton(GameEngine.coreData.rawInputs[0].name))
            {
                VelocityY(-2);
                if (IsGrounded())
                    StartStateFromScript(0);
            }
        }
    }
    [SerializeField] private int dashInput = 4;
    public BoxCollider2D headColl;
    public bool crouchFlag;

    private void SetCrouchFlag(bool crouch)
    {
        crouchFlag = crouch;
        headColl.enabled = !crouchFlag;
        if (headColl.enabled)
            controller.CalculateRaySpacing(headColl);
        else
            controller.CalculateRaySpacing(controller.defaultColl);
    }
    public bool CanUnCrouch()
    {
        bool hitAbove = Physics2D.OverlapCircle(transform.position + Vector3.up, 0.51f, whatCountsAsGround);
        bool hitBelow = Physics2D.OverlapCircle(transform.position + Vector3.down, 0.49f, whatCountsAsGround);
        if (hitAbove&&hitBelow)
        {
            return false;
        }
        else return true;
        //return !hitAbove || !hitBelow;
        
    }
    private void UnCrouch()
    {
        if (GameEngine.gameEngine.globalMovelistIndex != 1)//bomb
        {
            SetCrouchFlag(false);
        }
    }
    void DashCut()
    {
        if (currentState == 2 || currentState == 23||currentState==139)//dash and airdashState
        {
            //SetCrouchFlag(true);
            if (!Input.GetButton(GameEngine.coreData.rawInputs[dashInput].name))
            {
                dashCooldown = 0;
                StartStateFromScript(0);
            }
            //DashJump();
        }
        //if (currentState==0&&!aerialFlag)
        //{
        //    airMod = 1f;
        //}
    }


    private void DashJump()
    {
        if (!aerialFlag || wallFlag)//on ground
        {
            if (Input.GetButton(GameEngine.coreData.rawInputs[dashInput].name))
                airMod = 2f;
            if (Input.GetButtonUp(GameEngine.coreData.rawInputs[dashInput].name))
                airMod = 1f;
            if (!Input.GetButton(GameEngine.coreData.rawInputs[dashInput].name) && wallFlag)
                airMod = 1f;
        }
    }
    [SerializeField]
    private float fadeTime = 0.5f, shortDashInterval = 0.05f, dashLength = 0.45f;
    [SerializeField]
    private Transform afterImageParent;
    public void ShowAfterImage()
    {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < afterImageParent.childCount; i++)
        {
            Transform currentGhost = afterImageParent.GetChild(i);
            s.AppendCallback(() => currentGhost.position = draw.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = Direction != 1);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().sprite = spriteRend.sprite);
            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(henshinColors[GameEngine.gameEngine.globalMovelistIndex], 0));
            s.AppendCallback(() => FadeSprite(currentGhost, henshinColors[6]));
            s.AppendInterval(shortDashInterval);
        }
    }
    public void FadeSprite(Transform current, Color fadeColor)
    {
        current.GetComponent<SpriteRenderer>().material.DOKill();
        current.GetComponent<SpriteRenderer>().material.DOColor(fadeColor, fadeTime);
    }
    void ChargeUp(float _val)
    {
        shotPressure += _val;
    }
    [Header("Grounded Check")]
    [SerializeField]
    private LayerMask whatCountsAsGround;
    [HideInInspector] public bool aerialFlag, wallFlag, isOnGround, isOnWall;
    public float aerialTimer, groundDetectHeight, wallDetectWidth, animAerialState, animFallSpeed;

    public int jumps, jumpMax = 1;
    public int dashes, dashMax = 1;
    public void SpendDash(int dash)
    {
        dashes -= dash;
        Mathf.Clamp(dashes, 0, dashMax);
    }

    [Header("Timers")]
    public float coyoteTimer = 6f;
    public float dashCooldown, dashCooldownRate = 1f, invulCooldown, invulFlickerRate = 4f;
    [Header("Meter")]
    public float specialMeter, specialMeterMax = 100f, nextSpecialMeterUse;

    public void UpgradeMeter(float _val)
    {
        specialMeterMax += _val;
        healthManager.UpgradeMaxMeter(_val);
    }
    public void UseMeter(float _val)
    {
        ChangeMeter(-_val);
        //Debug.Log("Meter Spent");
    }
    public void BuildMeter(float _val)
    {
        ChangeMeter(_val);
    }
    public void ChangeMeter(float _val)
    {
        specialMeter += _val;
        specialMeter = Mathf.Clamp(specialMeter, 0f, specialMeterMax);
        healthManager.ChangeMeter((int)_val);
    }
    public void OnSkillUsed()
    {
        if (Mission.instance != null)
            Mission.instance.OnSkillUsed();
    }
    public void FullyHeal()
    {
        healthManager.AddHealth(1000);
        ChangeMeter(1000);
    }
    float velocityXSmoothing;
    void CalculateGravity()
    {
        //float targetVelocityX = leftStick.x * moveSpeed;
        //velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        if (enemyType == 3)//flying enemy type
        {
            
        }
        else
            velocity.y += gravity * Time.fixedDeltaTime;
    }
    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0 && leftStick.x == wallDirX)
        {
            wallSliding = true;
            animAerialState = -1f;
            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (leftStick.x != wallDirX && leftStick.x != 0)
                {
                    timeToWallUnstick -= Time.fixedDeltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                    //PlayAudio("Player/Wall Grip");
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
                PlayAudio("Player/Wall Grip");
            }

        }
        else
            timeToWallUnstick = 0;

    }
    void UpdatePhysics()
    {
        CalculateGravity();
        HandleWallSliding();
        controller.Move(velocity * Time.fixedDeltaTime, leftStick);
        wallFlag = wallSliding;
        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.fixedDeltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
        if (IsGrounded())
        {
            aerialFlag = false;
            //wallFlag = false;
            aerialTimer = 0;
            animAerialState = 0f;
            jumps = jumpMax;
            dashes = dashMax;
            //velocity.y = 0;
            GroundTouch();
        }
        else
        {
            if (!aerialFlag)
            {
                aerialTimer++;
            }
            if (aerialTimer >= coyoteTimer)//coyote time
            {
                aerialFlag = true;
                hasLanded = false;
                if (animAerialState <= 1f)
                {
                    animAerialState += 0.1f;
                }
                if (jumps == jumpMax)
                {
                    jumps--;
                }
            }

        }
    }
    public void CutsceneUpdatePhysics()
    {
        UpdatePhysics();
        UpdateAnimator();
    }
    public void Move(Vector2 velocity)
    {
        //myRB.velocity = velocity;
        controller.Move(velocity * Time.fixedDeltaTime, leftStick);
    }
    public bool HitCeiling()
    {
        return controller.collisions.above;
    }

    public bool IsGrounded()
    {
        //RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, groundDetectHeight, whatCountsAsGround);
        //Color rayColor;
        //if (rayCastHit.collider != null)
        //    rayColor = Color.green;
        //else
        //    rayColor = Color.red;
        //Debug.DrawRay(boxCollider2D.bounds.center + new Vector3(boxCollider2D.bounds.extents.x, 0), Vector2.down * (boxCollider2D.bounds.extents.y + groundDetectHeight), rayColor);
        //Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, 0), Vector2.down * (boxCollider2D.bounds.extents.y + groundDetectHeight), rayColor);
        //Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, boxCollider2D.bounds.extents.y + groundDetectHeight), Vector2.right * (boxCollider2D.bounds.extents.x), rayColor);

        //return rayCastHit.collider != null;

        return controller.collisions.below;
    }
    public bool IsOnWall()
    {
        //RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.right, groundDetectHeight, whatCountsAsGround);
        //if (rayCastHit.collider != null)
        //    Debug.Log("IsOnWall");

        //return rayCastHit.collider != null;
        return controller.collisions.left || controller.collisions.right;
    }
    private bool hasLanded=true;
    private void GroundTouch()
    {
        //DashReset();
        if (!hasLanded)
        {
            airMod = 1f;
            //velocity.y = 0f;
            animFallSpeed = 0f;
            //isDashing = false;
            hasLanded = true;
            landingParticle.Play();
            audioManager.PlaySound("Player/Landing");
        }
    }
    public void SetVelocity(Vector3 v)
    {
        velocity = v;
    }
    [Header("Hit Stun")]
    public Vector2 curHitAnim;
    public Vector2 targetHitAnim;
    private int curComboValue;
    public delegate void AttackCallback(int actionStateIndex, AttackEvent attackEvent);
    public delegate void ComboDropCallback();
    public event AttackCallback Attack;
    public event ComboDropCallback ComboDrop;
    public int GetComboValue()
    {
        return curComboValue;
    }
    public bool GetIsInvulnerable()
    {
        return isInvulnerable;
    }

    public bool CanBeHit(AttackEvent curAtk, bool isInvulnerable)
    {
        if (controlType == ControlType.DEAD||DialogueManager.instance.isDialogueActive)
            return false;
        if (invulCooldown > 0)
        {
            if (curComboValue < curAtk.comboValue)
                return true;
            else
                return false;
        }
        else
        {
            isInvulnerable = false;
            spriteRend.color = Color.white;
            return true;
        }
    }
    public static float grappleDampen = 20f;
    public LineRenderer lineRend;
    public TrailRenderer trailRend;
    public void Grappled()
    {
        hitStun = 4;
        Vector3 followVel = (grappleTarget.position - transform.position) * grappleDampen;
        SetVelocity(followVel);
    }
    public void Hit(CharacterObject attacker, int projectileIndex, int atkIndex)
    {
        GetHit(attacker, projectileIndex, atkIndex);
    }
    public CharacterObject grappleVictim;
    public Transform grapplePoint, grappleTarget;
    public void GetHit(CharacterObject attacker, int projectileIndex, int atkIndex)
    {
        AttackEvent curAtk;
        if (projectileIndex == 0)//not a projectile
        {
            curAtk = GameEngine.coreData.characterStates[attacker.currentState].attacks[attacker.currentAttackIndex];
        }
        else//projectiles
        {
            curAtk = GameEngine.coreData.characterStates[projectileIndex].attacks[atkIndex];
        }
        
        if (IsDefendingInState() && attacker.Direction==-Direction)
        {
            if (curAtk.poiseDamage < 20f)
            {
                //parry sound
                StartStateFromScript(defStateIndex);
                dashCooldown = 0;
                FaceTarget(target.transform.position);
                if (projectileIndex == 0) attacker.FrontVelocity(-10f);
            }
            else
            {
                PoiseBreak();
            }
        }
        else
        {
            if (healthManager.HasShield())
            {
                healthManager.ShieldDamage(curAtk.poiseDamage);
                GameEngine.SetHitPause(curAtk.hitStop);
                attacker.hitConfirm += 1;
            }
            else
            {
                if (CanBeHit(curAtk, GetIsInvulnerable()))
                {
                    Vector3 nextKnockback = curAtk.knockback;
                    Vector3 knockOrientation = transform.position - attacker.transform.position;
                    knockOrientation.Normalize();
                    nextKnockback.x *= knockOrientation.x;
                    curComboValue = curAtk.comboValue;
                    StartInvul(curAtk.hitStop);

                    bool shouldBreakPoise = false;
                    if (healthManager.currentPoise > 0 && curAtk.poiseDamage>healthManager.currentPoise){shouldBreakPoise = true;}

                    healthManager.PoiseDamage(curAtk.poiseDamage); 
                    if (healthManager.currentPoise <= 0)
                    {
                        SetVelocity(nextKnockback * 0.7f);//dampen a bit
                        targetHitAnim.x = curAtk.hitAnim.x;
                        targetHitAnim.y = curAtk.hitAnim.y;

                        //curHitAnim.x = UnityEngine.Random.Range(-1f, 1f);//randomized for fun
                        //curHitAnim.y = UnityEngine.Random.Range(-1f, 1f);
                        curHitAnim = targetHitAnim * .25f;
                        hitStun = curAtk.hitStun;

                        if (shouldBreakPoise)
                        {
                            PoiseBreak();
                        }
                        StartState(hitStunStateIndex);
                        if (curAtk.attackType == 10 && controlType != ControlType.OBJECT)//grappling code
                        {
                            grappleTarget = attacker.grapplePoint;
                            attacker.grappleVictim = this;
                            attacker.StartStateFromScript((int)curAtk.hitAnim.x);
                        }
                    }
                    if (curAtk.attackType == 9)//auto combo
                    {
                        attacker.StartStateFromScript((int)curAtk.hitAnim.x);
                    }
                    GameEngine.SetHitPause(curAtk.hitStop);
                    attacker.hitConfirm += 1;
                    attacker.BuildMeter(curAtk.meterGain);

                    
                    if (projectileIndex>0)
                    {
                        OnAttackEvent(projectileIndex, curAtk);//atk event
                    }
                    else
                        OnAttackEvent(attacker.currentState, curAtk);//atk event
                    if (controlType!=ControlType.DUMMY)
                        healthManager.RemoveHealth(curAtk.damage, curAtk);

                    PlayAudio(attackStrings[curAtk.attackType]);
                    switch (controlType)//damage calc
                    {
                        case ControlType.AI:
                            GlobalPrefab(curAtk.attackType);
                            break;
                        case ControlType.OBJECT:
                            GlobalPrefab(curAtk.attackType);
                            break;
                        case ControlType.DUMMY:
                            GlobalPrefab(curAtk.attackType);
                            break;
                        case ControlType.BOSS:
                            GlobalPrefab(curAtk.attackType);
                            break;
                        case ControlType.PLAYER:
                            GlobalPrefab(2);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    public void OnAttackEvent(int stateIndex,AttackEvent attackEvent)
    {
        Attack?.Invoke(stateIndex, attackEvent);
    }
    private void PoiseBreak()
    {
        float stunned = 30f;
        hitStun += stunned;
    }

    private bool isInvulnerable;
    public void SetInvulCooldown(float iFrames)
    {
        invulCooldown = iFrames;
        curComboValue = 99;
    }
    private void StartInvul(float hitFlash)
    {
        if (invulCooldown <= 0 && (controlType != ControlType.AI && controlType != ControlType.OBJECT))
        {
            invulCooldown = 90f;
            isInvulnerable = true;
        }
        StartCoroutine(FlashWhiteDamage(hitFlash));
    }

    private IEnumerator FlashWhiteDamage(float hitFlash)
    {
        spriteRend.material = defaultMat;
        spriteRend.material = whiteMat;
        for (int i = 0; i < hitFlash; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        spriteRend.material = defaultMat;
        StartCoroutine(BlinkWhileInvulnerableCoroutine());
    }
    private IEnumerator BlinkWhileInvulnerableCoroutine()
    {
        while (isInvulnerable)
        {
            //yield return new WaitForSeconds(blinkInterval);
            spriteRend.color = flashColor;
            for (int i = 0; i < invulFlickerRate; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            spriteRend.color = Color.white;
            for (int i = 0; i < invulFlickerRate; i++)
            {
                yield return new WaitForFixedUpdate();
            }

        }
    }

    [SerializeField] string[] attackStrings;

    [Tooltip("hitstun index in coreData")]
    public int hitStunStateIndex = 7, deathStateIndex = 36;//hitstun state in coreData
    public float hitStun;
    public void GettingHit()
    {
        hitStun--;
        if (hitStun <= 0) 
        { 
            EndState();
            healthManager.PoiseReset(); 
            OnComboEnded(); 
        }
        curHitAnim += (targetHitAnim - curHitAnim) * .1f;//blends for 3D games
    }
    public void OnComboEnded()
    {
        ComboDrop?.Invoke();
    }
    [Header("EnemyLogic")]
    public CharacterObject target;
    public InteractableObject[] spawners;
    private Vector2 visualAggroRange;
    public float aggroDistance = 30f, aggroHeight = 8f, longAttackRange = 10f, shortAttackRange = 5f, attackCooldown = 180f;
    
    //public float amplitude = 1f, frequency = 1f;
    [SerializeField] private bool isAggroRange, isLongRange, isShortRange;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int[] closeAttackState, rangedAttackState, desperationCAStates, desperationRAStates;

    [Tooltip("0 = MoveForward, 1 = MoveTowards, 2 = JumpAction, 3 = Float")]
    public int enemyType;
    [Header("Desperation State")]
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int desperationTransitionState;
    [Header("enemyType 3 Only")]
    public float floatHeight = 2, floatTime=0.25f;

    [Space]
    [Header("Blocking States")]
    public bool canDefend = false;
    public bool IsDefendingInState()
    {
        if (canDefend)//can block attacks
        {
            if (hitStun <= 0)//is not stunned
            {
                for (int i = 0; i < defStates.Length; i++)//go through blocking state array
                {
                    if (currentState == defStates[i])
                    {
                        return true;//is in a blocking state
                    }
                }
                return false;//has no blocking states
            }
            else return false;//is stunned
        }
        else return false;//cannot block
    }
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int defStateIndex;
    public int[] defStates;
    private int attackStep = 0;
    public float MaxJumpVelocity { get => maxJumpVelocity; set => maxJumpVelocity = value; }
    public float Direction { get => _direction; set => _direction = value; }

    private void UpdateAI()
    {
        if (target == null)
        {
            FindTarget();
        }
        if (currentState == 0)//Neutral
        {
            if (isAggroRange && (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) > longAttackRange))
            {
                FaceTarget(target.transform.position);
                switch (enemyType)
                {
                    case 0:
                        FrontVelocity(moveSpeed);
                        break;
                    case 1:
                        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, moveSpeed);
                        break;
                    case 2:
                        StartStateFromScript(rangedAttackState[0]);
                        break;
                    case 3:
                        //flying code
                        break;
                }
            }
            if (enemyType==3)
            {
                if (currentState != hitStunStateIndex)
                {
                    Vector3 newPos = transform.position;
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100, controller.collisionMask);
                    if (hit)
                    {
                        //newPos.y = (hit.point + Vector2.up * floatHeight).y;
                        newPos.y = Mathf.Lerp(newPos.y, (hit.point + Vector2.up * floatHeight).y, floatTime);
                    }
                    transform.position = newPos;
                }
                else
                    velocity.y += gravity * Time.fixedDeltaTime;
            }
            
            if (isAggroRange && dashCooldown <= 0 && (Mathf.Abs(transform.position.x - GameEngine.gameEngine.mainCharacter.transform.position.x) <= longAttackRange))
            {
                FaceTarget(target.transform.position);
                if (controlType!=ControlType.DUMMY)
                    velocity = Vector2.zero;
                if (isLongRange && rangedAttackState.Length > 0)
                {
                    //int randNum = UnityEngine.Random.Range(0, rangedAttackState.Length);
                    if (attackStep>=rangedAttackState.Length)
                    {
                        attackStep = 0;
                    }
                    StartStateFromScript(rangedAttackState[attackStep]);
                    attackStep++;
                }
                if (isShortRange && closeAttackState.Length > 0)
                {
                    //int randNum = UnityEngine.Random.Range(0, closeAttackState.Length);
                    if (attackStep >= closeAttackState.Length)
                    {
                        attackStep = 0;
                    }
                    StartStateFromScript(closeAttackState[attackStep]);
                    attackStep++;
                }
            }
        }
        if (currentState != 0 && currentState != defStateIndex)//Attack
        {
            dashCooldown = attackCooldown;
        }
    }
    public void OnDesperation()
    {
        closeAttackState = desperationCAStates;
        rangedAttackState = desperationRAStates;
        StartStateFromScript(desperationTransitionState);
        attackStep = 0;
        dashCooldown += 100;
        //attackCooldown *= 0.5f;
    }
    public void OnDeath()
    {
        hitStun = 0;
        StopChargeAttack();
        SetInvulCooldown(360f);
        StartStateFromScript(deathStateIndex);
        controlType = ControlType.DEAD;
        invulCooldown = 0f;
        spriteRend.color = Color.white;
        spriteRend.material = defaultMat;
        Screenshake(2, .4f);
        SetVelocity(Vector2.zero);
        KillMinions();
        EraseSatellites();
    }

    private void KillMinions()
    {
        if (spawners.Length > 0 && spawners[0] !=null)
        {
            foreach (InteractableObject spawner in spawners)
            {
                MinionSpawner mSpawn = spawner.gameObject.GetComponentInChildren<MinionSpawner>();

                if (mSpawn != null)
                    mSpawn.KillThemAll();

                spawner.DeSpawn();
            }
        }
    }

    public void OnEnemySpawn()
    {
        controlType = ControlType.AI;
        dashCooldown = 60;
        StartStateFromScript(0);
    }
    public void OnObjectSpawn()
    {
        controlType = ControlType.OBJECT;
        StartStateFromScript(0);
    }
    public void OnDummySpawn()
    {
        controlType = ControlType.DUMMY;
        StartStateFromScript(0);
    }
    public void OnBossSpawn()
    {
        controlType = ControlType.BOSS;
        StartStateFromScript(0);
    }
    void FindTarget()
    {
        target = GameEngine.gameEngine.mainCharacter;
    }
    public void FaceTarget(Vector3 tarPos)
    {
        Vector3 tarOffset = (tarPos - transform.position);
        Direction = Mathf.Sign(tarOffset.x);
        transform.localScale = new Vector3(Direction, 1f, 1f);
    }
    public void FaceDir(float dir)
    {
        Direction = Mathf.Sign(dir);
        transform.localScale = new Vector3(Direction, 1f, 1f);
    }
    public void FacePlayer()
    {
        FaceTarget(GameEngine.gameEngine.mainCharacter.transform.position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(aggroDistance*2, aggroHeight * 2, 1));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(longAttackRange * 2, aggroHeight * 2, 1));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(shortAttackRange * 2, aggroHeight * 2, 1));

        Gizmos.DrawWireSphere(transform.position+Vector3.down, .49f);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
