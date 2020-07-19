using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CharacterObject : MonoBehaviour
{
    [Header("Movement")]
    public Vector2 velocity;

    public float gravity = -0.01f;
    public float aniMoveSpeed;

    public Vector3 friction = new Vector3(0.95f, 0.99f, 0.95f);
    [SerializeField]private float direction=1;

    public Rigidbody2D myRB;
    public BoxCollider2D boxCollider2D;
    public Controller2D controller;
    public RadialMenuController henshin;
    public AfterImagePool[] afterImage;
    public HealthManager healthManager;

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
    public SpriteRenderer spriteRend;
    
    public enum ControlType { AI, PLAYER };
    public ControlType controlType;

    [Header("HitCancel")]
    public Hitbox hitbox;
    public bool canCancel;
    public bool isHit;
    public int hitConfirm;

    public InputBuffer inputBuffer = new InputBuffer();

    // Use this for initialization
    void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        controller = GetComponent<Controller2D>();
        spriteRend = draw.GetComponentInChildren<SpriteRenderer>();
        healthManager = GetComponent<HealthManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        switch (controlType)
        {
            case ControlType.AI:
                break;
            case ControlType.PLAYER:
                PauseMenu();
                Henshin();
                if (!PauseManager.IsGamePaused)
                {
                    JumpCut();
                    DashCut();
                    ChargeAttack();
                }
                leftStick = new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis));
                break;
            default:
                break;
        }
    }
    void FixedUpdate()
    {
        if (!PauseManager.IsGamePaused)
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
                    case ControlType.PLAYER:
                        UpdateInput();
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
            UpdateAnimator();
        }
    }

    void UpdateTimers()
    {
        if (dashCooldown > 0) { dashCooldown -= dashCooldownRate; }
    }

    private void UpdateAI()
    {

    }
    public float animSpeed;
    void UpdateAnimator()
    {
        animSpeed = 1;
        if (GameEngine.hitStop>0)
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
    /*public Vector2 leftStick;
     void CameraRelativeStickMove(float _val)
    {
        Vector3 velHelp = new Vector3(0, 0, 0);
        Vector3 velDir;

        //leftStick = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        if ((leftStick.x > GameEngine.gameEngine.deadZone || leftStick.x < -GameEngine.gameEngine.deadZone || leftStick.y > GameEngine.gameEngine.deadZone || leftStick.y < -GameEngine.gameEngine.deadZone))
        {

            //if (stickHelp.sqrMagnitude > 1) { stickHelp.Normalize(); }


            velDir = Camera.main.transform.forward;
            velDir.y = 0;
            velDir.Normalize();
            velHelp += velDir * leftStick.y;

            velHelp += Camera.main.transform.right * leftStick.x;
            velHelp.y = 0;



            velHelp *= _val;
            
            velocity += velHelp;
        }
    }
    */
    void FaceStick()
    {
        if (CheckVelocityDeadZone())
        {
            if (leftStick.x > 0) { direction = 1; transform.localScale = new Vector3(1f, 1f, 1f); }
            else if (leftStick.x < 0) { direction = -1; transform.localScale = new Vector3(-1f, 1f, 1f); }
            if (Mathf.Abs(airMod) == 2)
            {
                //ShowAfterImage();
            }
        }
    }
    void UpdateState()
    {
        CharacterState myCurrentState = GameEngine.coreData.characterStates[currentState];

        if (hitStun > 0)
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
        foreach(StateEvent _ev in GameEngine.coreData.characterStates[currentState].events)
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
    public float hitActive;
    public int currentAttackIndex;
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
            if (currentStateTime==_atk.start+_atk.length)
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
    public static float whiffWindow = 28f;
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
        if (_params==null){ return; }
        if (_params.Count<=0){ return; }
        switch(_index)//index = element in characterscripts
        {
            case 0://Jump
                VelocityY(_params[0].val);
                break;
            case 1:
                FrontVelocity(_params[0].val);
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
                FireBullet(_params[0].val, _params[1].val, _params[2].val, _params[3].val);
                break;
            case 11:
                Dash(_params[0].val);
                break;
            case 12:
                SpawnKinzecter(_params[0].val, _params[1].val, _params[2].val);
                break;
                
        }
    }
    public void DOChangeMovelist(int index)
    {
        PlayFlashParticle(henshinColors[index]);
        GameEngine.SetHitPause(10f);
        GameEngine.gameEngine.ChangeMovelist(index);
        characterAnim.runtimeAnimatorController = formAnims[GameEngine.gameEngine.globalMovelistIndex];
    }
    public void ToggleMovelist()
    {
        GameEngine.SetHitPause(10f);
        GameEngine.gameEngine.ToggleMovelist();
        PlayFlashParticle(henshinColors[GameEngine.gameEngine.globalMovelistIndex]);
        characterAnim.runtimeAnimatorController = formAnims[GameEngine.gameEngine.globalMovelistIndex];
    }
    private void Dash(float dashSpeed)
    {
        //Screenshake();
        //if (NumberOfDashes <= 0 && CurrentNumberOfBullets > 0)
        //{
        //    if (!isOnGround)
        //        SpendAmmo(1);
        //    if (CurrentNumberOfBullets <= 0)
        //    {
        //        hasAirDashed = true;
        //    }
        //}
        //else
        //    hasAirDashed = true;

        velocity = Vector2.zero;
        Vector2 dir = new Vector2(leftStick.x, leftStick.y);
        if (dir==Vector2.zero)
        {
            dir.x = direction;
        }
        velocity += dir.normalized * dashSpeed;
        characterAnim.SetFloat("dirAnimX", Mathf.Abs(dir.x));
        characterAnim.SetFloat("dirAnimY", dir.y);
        StartCoroutine(DashWait());
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
    void HorizontalDrag(float x){ velocity.x = x; }
    void VerticalDrag(float y){ velocity.y = y; }
    void Jump(float _pow)
    {
        velocity.y = _pow;
        jumps--;
        //hasLanded = false;
        landingParticle.Play();
        //    aerialTimer = coyoteTimer+1f;
        //    aerialFlag = true;
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
        GameEngine.GlobalPrefab((int)_index, character,_act,_ev);
    }
    void GlobalPrefab(float _prefab)
    {
        GlobalPrefab(_prefab, -1, -1);
    }
    private void FrontVelocity(float _pow)
    {
        velocity.x = _pow*direction;
    }
    [Header("MovementVectors")]
    public Vector2 leftStick;
    public string horizontalAxis = "altPs4Horizontal", verticalAxis = "altPs4Vertical";
    void StickMove(float _pow)
    {
        if ((leftStick.x > deadzone || leftStick.x < -deadzone || leftStick.y > deadzone || leftStick.y < -deadzone))
        {
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
                velocity.x = (airMod * _mov) * moveSpeed * _pow;
                if (Mathf.Abs(airMod)==2)
                {
                    //ShowAfterImage();
                }
            }
            else
                velocity.x = (_mov) * moveSpeed * _pow;
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
                float _mov = 0;
                if (leftStick.x > deadzone)
                {
                    _mov = 1;
                }
                if (leftStick.x < -deadzone)
                {
                    _mov = -1;
                }
                velocity.x = (airMod * _mov) * moveSpeed * _pow;
            }
        }
        if (hitStun <= 0)
        {
            FaceStick();
        }
    }
    void VelocityY(float _pow)
    {
        velocity.y = _pow;
    }

    public float deadzone = 0.2f;

    public float moveSpeed = 10f;
    public float airMod = 1f;
    public float jumpPow = 12;

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
        
        SetAnimation(GameEngine.coreData.characterStates[currentState].stateName);
        Debug.Log("State Started: " + GameEngine.coreData.characterStates[currentState].stateName);
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
            inputBuffer.UseInput(nextCommand.input);
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

    [SerializeField] private float shotPressure;
    [SerializeField] private float  minShotPressure=30f, maxShotPressure = 60f;
    private bool shouldChargeBuster;
    public int chargeAttackIndex = 15, chargeShotIndex=21, critBusterIndex=22;
    [SerializeField] private bool firstCharge, secondCharge;
    private Color c;

    public float chargeIncrement = 1f;
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
                    if (shotPressure > maxShotPressure)
                    {
                        StartState(chargeAttackIndex);
                    }
                    firstCharge = false; secondCharge = false;
                    foreach (ParticleSystem p in primaryGunParticles)
                    {
                        p.startColor = Color.clear;
                        p.Stop();
                    }
                    shotPressure = 0;
                }
                break;
            case 1://Bombadier
                if (Input.GetButton(GameEngine.coreData.rawInputs[1].name))//name of charge Attack
                {
                    ColorCharge();
                    ChargeUp(chargeIncrement);
                    if (shotPressure < maxShotPressure)
                    {
                        hasShotChargingStarted = true;
                    }
                }
                if (Input.GetButtonUp(GameEngine.coreData.rawInputs[1].name))
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
                    if (shotPressure != 0f)
                    {
                        hasReleasedShot = true;
                        shotPressure = 0f;
                        firstCharge = false; secondCharge = false;
                    }
                    foreach (ParticleSystem p in primaryGunParticles)
                    {
                        p.startColor = Color.clear;
                        p.Stop();
                    }
                    //shotPressure = 0;
                }
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
        }


        
    }
    [Space]
    [Header("Charged Buster")]

    private bool hasReleasedShot = false;
    private bool hasShotChargingStarted = false;
    private bool hasReachedMaxShotCharge = false;

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
    public GameObject blastBlightGO;
    public float shootAnim, shootAnimMax;
    public Vector2 bulletVelocity = new Vector2(20f, 0), busterVelocity = new Vector2(30f, 0), critBusterVelocity = new Vector2(35f, 0);
    public float fireRate = 10f;
    private float timeToNextFire = 0f;
    public GameObject[] bullets;
    [SerializeField] private Vector2 bulletSpawnPos = new Vector2(0.5f, 1f);
    public bool isKinzecterOut;
    public void SpawnKinzecter(float bulletType, float offsetX, float offsetY)
    {
        if (!isKinzecterOut)
        {
            var offset = new Vector3(offsetX * direction, offsetY, 0);
            GameObject newbullet = Instantiate(bullets[(int)bulletType], transform.position + offset, Quaternion.identity);
            newbullet.transform.localScale = new Vector3(direction, 1, 1);
            newbullet.GetComponent<Kinzecter>().ThrowKinzecter(characterObject);
            newbullet.GetComponent<BulletHit>().character = characterObject;
            newbullet.GetComponent<Hitbox>().character = characterObject;
            isKinzecterOut = true;
        }
    }
    public void FireBullet(float bulletType, float bulletSpeed, float offsetX, float offsetY)
    {
        shootAnim = shootAnimMax;
        var offset = new Vector3(offsetX*direction, offsetY, 0);
        GameObject newbullet = Instantiate(bullets[(int)bulletType], transform.position+offset, Quaternion.identity);
        newbullet.GetComponent<BulletHit>().character = characterObject;
        newbullet.GetComponent<Hitbox>().character = characterObject;
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(bulletSpeed * direction, 0);
        newbullet.transform.localScale = new Vector3(direction, 1, 1);
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
        FireBullet(bulletType, critBusterVelocity.x,bulletSpawnPos.x, bulletSpawnPos.y);
        //}

    }
    public void BusterShooting()
    {
        StartState(chargeShotIndex);
    }
    public void CriticalBusterShooting()
    {
        StartState(critBusterIndex);
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
    [SerializeField] private int menuTimer, menuDelay=12;
    private void Henshin()
    {
        if (Input.GetButton(GameEngine.coreData.rawInputs[4].name))//open radial menu
        {
            if (menuTimer<menuDelay)
            {
                menuTimer++;
            }
        }
        if (menuTimer>=menuDelay)
        {
            GameEngine.SetHitPause(10f);
            henshin.ActivateMenu();
            menuTimer++;
        }
        if (Input.GetButtonUp(GameEngine.coreData.rawInputs[4].name))//open radial menu
        {
            if (menuTimer<menuDelay)
            {
                ToggleMovelist();
                menuTimer = 0;
            }
            else
            {
                henshin.SelectForm();
                menuTimer = 0;
            }
        }
    }
    private void PauseMenu()
    {
        if (Input.GetButtonDown(GameEngine.coreData.rawInputs[8].name))
            PauseManager.pauseManager.PauseButtonPressed();
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
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
                StartState(0);
            }
        }
    }
    void DashCut()
    {
        if (currentState == 2 || currentState == 18)//dash and airdashState
        {
            if (Input.GetButtonUp(GameEngine.coreData.rawInputs[2].name))
            {
                dashCooldown = 0;
                StartState(0);
            }
            //if (Mathf.Abs(Input.GetAxis(GameEngine.coreData.rawInputs[13].name))==1)
            //{
            //    if ((Input.GetAxis(GameEngine.coreData.rawInputs[13].name))==0)
            //    {
            //        dashCooldown = 0;
            //        StartState(0);
            //    }
            //}
            DashJump();
        }
        
    }

    private void DashJump()
    {
        if (!aerialFlag || wallFlag)//on ground
        {
            if (Input.GetButton(GameEngine.coreData.rawInputs[2].name))
                airMod = 2f;
            if (Input.GetButtonUp(GameEngine.coreData.rawInputs[2].name))
                airMod = 1f;
            if (!Input.GetButton(GameEngine.coreData.rawInputs[2].name) && isOnWall)
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
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = direction!=1);
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
    public bool aerialFlag,wallFlag,isOnGround,isOnWall;
    [SerializeField] private float aerialTimer, groundDetectHeight,wallDetectWidth, animAerialState, animFallSpeed;

    public int jumps, jumpMax = 1;

    [Header("Timers")]
    public float coyoteTimer = 3f;
    public float dashCooldown, dashCooldownRate = 1f;
    public float specialMeter, specialMeterMax = 100f;

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
        healthManager.ChangeHealth((int)_val);
    }
    void UpdatePhysics()
    {
        if (IsGrounded())
        {
            aerialFlag = false;
            //wallFlag = false;
            aerialTimer = 0;
            animAerialState = 0f;
            jumps = jumpMax;
            //velocity.y = 0;
            GroundTouch();
        }
        else
        {
            if (!aerialFlag)
            {
                aerialTimer++;
            }
            if (aerialTimer >= coyoteTimer )//coyote time
            {
                aerialFlag = true;
                if (animAerialState<=1f)
                {
                    animAerialState += 0.1f;
                }
                if (jumps==jumpMax)
                {
                    jumps--;
                }
                if (IsOnWall() && leftStick.x == direction && velocity.y < 0)//wall
                {
                    velocity.y = -1.7f;
                    animAerialState = -1f;
                    wallFlag = true;

                }
                else
                {
                    if (controller.collisions.above || controller.collisions.below)
                        velocity.y = 0;
                    else
                    {
                        velocity.y += gravity;
                        hasLanded = false;
                    }

                    wallFlag = false;
                }
            }
            
        }
        Move(velocity);
        velocity.Scale(friction);
    }
    void Move(Vector2 velocity)
    {
        //myRB.velocity = velocity;
        controller.Move(velocity*Time.fixedDeltaTime, leftStick);
    }
    public bool HitCeiling()
    {
        return controller.collisions.above;
    }

    public bool IsGrounded()
    {
        RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, groundDetectHeight, whatCountsAsGround);
        Color rayColor;
        if (rayCastHit.collider != null)
            rayColor = Color.green;
        else
            rayColor = Color.red;
        Debug.DrawRay(boxCollider2D.bounds.center + new Vector3(boxCollider2D.bounds.extents.x, 0), Vector2.down * (boxCollider2D.bounds.extents.y + groundDetectHeight), rayColor);
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, 0), Vector2.down * (boxCollider2D.bounds.extents.y + groundDetectHeight), rayColor);
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, boxCollider2D.bounds.extents.y + groundDetectHeight), Vector2.right * (boxCollider2D.bounds.extents.x), rayColor);

        return rayCastHit.collider != null;

        //return controller.collisions.below;
    }
    public bool IsOnWall()
    {
        //RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.right, groundDetectHeight, whatCountsAsGround);
        //if (rayCastHit.collider != null)
        //    Debug.Log("IsOnWall");

        //return rayCastHit.collider != null;
        return controller.collisions.left || controller.collisions.right;
    }
    private bool hasLanded;
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
        }
    }
    public void SetVelocity(Vector3 v)
    {
        velocity = v;
    }
    [Header("Hit Stun")]
    public Vector2 curHitAnim;
    public Vector2 targetHitAnim;

    public void GetHit(CharacterObject attacker, int projectileIndex)
    {
        if (projectileIndex==0)//not a projectile
        {
            AttackEvent curAtk = GameEngine.coreData.characterStates[attacker.currentState].attacks[attacker.currentAttackIndex];
            Vector3 nextKnockback = curAtk.knockback;

            Vector3 knockOrientation = character.transform.position - attacker.character.transform.position;
            knockOrientation.Normalize();
            nextKnockback.x *= knockOrientation.x;


            SetVelocity(nextKnockback * 0.7f);//dampen a bit
            targetHitAnim.x = curAtk.hitAnim.x;
            targetHitAnim.y = curAtk.hitAnim.y;

            //curHitAnim.x = UnityEngine.Random.Range(-1f, 1f);//randomized for fun
            //curHitAnim.y = UnityEngine.Random.Range(-1f, 1f);
            if (curAtk.blastBlight > 0)
            {
                ActivateBlastblight(curAtk);
            }

            curHitAnim = targetHitAnim * .25f;

            GameEngine.SetHitPause(curAtk.hitStop);
            hitStun = curAtk.hitStun;
            attacker.hitConfirm += 1;
            attacker.BuildMeter(curAtk.meterGain);
            switch (controlType)//damage Calc
            {
                case ControlType.AI:
                    break;
                case ControlType.PLAYER:
                    HealthVisualManager.healthSystemStatic.Damage(curAtk.damage);
                    break;
                default:
                    break;
            }
        }
        else//projectiles
        {
            AttackEvent curAtk = GameEngine.coreData.characterStates[projectileIndex].attacks[0];
            Vector3 nextKnockback = curAtk.knockback;

            Vector3 knockOrientation = character.transform.position - attacker.character.transform.position;
            knockOrientation.Normalize();
            nextKnockback.x *= knockOrientation.x;


            SetVelocity(nextKnockback * 0.7f);//dampen a bit
            targetHitAnim.x = curAtk.hitAnim.x;
            targetHitAnim.y = curAtk.hitAnim.y;

            //curHitAnim.x = UnityEngine.Random.Range(-1f, 1f);//randomized for fun
            //curHitAnim.y = UnityEngine.Random.Range(-1f, 1f);
            curHitAnim = targetHitAnim * .25f;

            GameEngine.SetHitPause(curAtk.hitStop);
            hitStun = curAtk.hitStun;
            attacker.hitConfirm += 1;
            attacker.BuildMeter(curAtk.meterGain);
            switch (controlType)//damage calc
            {
                case ControlType.AI:
                    break;
                case ControlType.PLAYER:
                    HealthVisualManager.healthSystemStatic.Damage(curAtk.damage);
                    break;
                default:
                    break;
            }
        }

       
        StartState(hitStunStateIndex);
        GlobalPrefab(0);
    }

    private void ActivateBlastblight(AttackEvent curAtk)
    {
        if (GetComponentInChildren<Blastblight>() != null)//if blastblight is already childed to you
        {
            GetComponentInChildren<Blastblight>().AddBlast(curAtk.blastBlight);
        }
        else
        {
            GameObject newBlight = Instantiate(blastBlightGO, transform);//if not create a new blastblight go and child it to you
            newBlight.transform.parent = transform;
            newBlight.GetComponent<Blastblight>().AddBlast(curAtk.blastBlight);
        }
    }

    [Tooltip("hitstun index in coreData")]
    public int hitStunStateIndex = 7;//hitstun state in coreData
    public float hitStun;
    public void GettingHit()
    {
        hitStun--;
        if (hitStun <= 0){ EndState(); }
        curHitAnim += (targetHitAnim - curHitAnim) * .1f;//blends for 3D games
    }
}
