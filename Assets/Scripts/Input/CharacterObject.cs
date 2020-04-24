using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterObject : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 velocity;

    public float gravity = -0.01f;
    public float aniMoveSpeed;

    public Vector3 friction = new Vector3(0.95f, 0.99f, 0.95f);
    [SerializeField]private float direction=1;

    public Rigidbody2D myRB;
    public BoxCollider2D boxCollider2D;

    [Header("CurrentState")]
    public int currentState;
    public float currentStateTime;
    public float prevStateTime;

    [Header("CharacterModel")]
    public GameObject character;
    public GameObject draw;
    public Animator characterAnim;
    public enum ControlType { AI, PLAYER };
    public ControlType controlType;

    [Header("HitCancel")]
    public Hitbox hitbox;
    public bool canCancel;
    public int hitConfirm;

    public InputBuffer inputBuffer = new InputBuffer();

    // Use this for initialization
    void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameEngine.hitStop<=0)
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
        UpdateIsOnGround();
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

        Vector3 latSpeed = new Vector3(velocity.x, 0, velocity.z);
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed) * 30f;
        animFallSpeed = velocity.y /** 30f*/;
        characterAnim.SetFloat("moveSpeed", aniMoveSpeed);
        characterAnim.SetFloat("aerialState", animAerialState);
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
        foreach(StateEvent _ev in GameEngine.coreData.characterStates[currentState].events)
        {

            //if (prevStateTime <= _ev.start && currentStateTime == _ev.start)
            if (currentStateTime >= _ev.start && currentStateTime <= _ev.end)
            {
                DoEventScript(_ev.script, _ev.variable);
            }
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

            if (currentStateTime >= cWindow + whiffWindow)
                canCancel = true;

            _cur++;
        }
    }
    public static float whiffWindow = 8f;
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
    void DoEventScript(int _index, float _var)
    {
        switch(_index)//index = element in characterscripts
        {
            case 0://Jump
                VelocityY(_var);
                break;
            case 1:
                FrontVelocity(_var);
                break;
            case 3:
                StickMove(_var);
                break;
            case 4:
                GettingHit();
                break;
            case 5:
                GlobalPrefab(_var);
                break;
            case 6:
                CanCancel(_var);
                break;
            case 7:
                Jump(_var);
                break;
            case 8:
                FaceStick();
                break;
            case 9:
                AirMove(_var);
                break;
                
        }
    }
    void Jump(float _pow)
    {
        velocity.y = _pow;
        jumps--;
        isOnGround = false;
        aerialTimer = coyoteTimer+1f;
        aerialFlag = true;
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
            velocity.x = _mov * moveSpeed * _pow;
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
                velocity.x = _mov * moveSpeed * _pow;
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

    public float moveSpeed = 0.01f;
    public float jumpPow = 1;

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
    }
    void SetAnimation(string animName)
    {
        characterAnim.CrossFadeInFixedTime(animName, GameEngine.coreData.characterStates[currentState].blendRate);

        Debug.Log("Start " + animName);
        //characterAnim.Play(animName);
    }

    public int currentCommandState;
    public int currentCommandStep;

    public void GetCommandState()
    {
        currentCommandState = 0;
        for (int c = 0; c < GameEngine.coreData.commandStates.Count; c++)
        {
            CommandState s = GameEngine.coreData.commandStates[c];
            if (s.aerial==aerialFlag)
            {
                currentCommandState = c;
                return;
            }
        }
    }

    int[] cancelStepList = new int[2];

    void UpdateInput()
    {
        JumpCut();
        ChargeAttack();
        leftStick = new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis));

        inputBuffer.Update();

        bool startState = false;

        GetCommandState();
        CommandState comState = GameEngine.coreData.commandStates[currentCommandState];


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
    [SerializeField] private float maxShotPressure = 60f;
    private bool shouldChargeBuster;
    public int chargeAttackIndex = 15;
    [SerializeField] private bool firstCharge, secondCharge;
    private Color c;

    public float chargeIncrement = 1f;
    void ChargeAttack()
    {
        if (Input.GetButton(GameEngine.coreData.rawInputs[1].name))//name of charge Attack
        {
            ShargeUp(chargeIncrement);
        }
        if (Input.GetButtonUp(GameEngine.coreData.rawInputs[1].name))
        {
            if (shotPressure > maxShotPressure)
            {
                StartState(chargeAttackIndex);
            }
            shotPressure = 0;
        }
    }
    void JumpCut()
    {
        if (currentState==1)
        {
            if (velocity.y > 0 && Input.GetButtonUp(GameEngine.coreData.rawInputs[0].name))
            {
                VelocityY(-2);
            if (IsGrounded())
                StartState(0);
            }
        }
    }
    void ShargeUp(float _val)
    {
        shotPressure += _val;
    }
    [Header("Grounded Check")]
    public bool aerialFlag,isOnGround;
    [SerializeField] private float aerialTimer,groundDetectHeight, animAerialState,animFallSpeed;
    [SerializeField]
    private Transform groundDetectPoint;
    [SerializeField]
    private LayerMask whatCountsAsGround;

    public int jumps, jumpMax = 1;

    [Header("Timers")]
    public float coyoteTimer = 3f;
    public float dashCooldown, dashCooldownRate = 1f;
    public float specialMeter, specialMeterMax = 100f;

    public void UseMeter(float _val)
    {
        ChangeMeter(-_val);
    }
    public void BuildMeter(float _val)
    {
        ChangeMeter(_val);
    }
    public void ChangeMeter(float _val)
    {
        specialMeter += _val;
        specialMeter = Mathf.Clamp(specialMeter, 0f, specialMeterMax);
    }
    void UpdatePhysics()
    {
        if (IsGrounded())
        {
            aerialFlag = false;
            aerialTimer = 0;
            animAerialState = 0f;
            jumps = jumpMax;
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
            }
            velocity.y += gravity;
        }

        Move(velocity);

        velocity.Scale(friction);

    }
    void Move(Vector3 velocity)
    {
        myRB.velocity = velocity;
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
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, boxCollider2D.bounds.extents.y+groundDetectHeight), Vector2.right * (boxCollider2D.bounds.extents.x), rayColor);

        return rayCastHit.collider != null;
    }
    private void UpdateIsOnGround()
    {
        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, groundDetectHeight, whatCountsAsGround);
        isOnGround = groundObjects.Length > 0;
        if (velocity.y < 0)
        {
            hasLanded = false;
        }
    }
    private bool hasLanded;
    private void GroundTouch()
    {
        //DashReset();
        if (!hasLanded)
        {
            Debug.Log("hasLanded");
            velocity.y = 0f;
            animFallSpeed = 0f;
            //isDashing = false;
            hasLanded = true;
            //landingParticles.Play();
        }
    }
    public void SetVelocity(Vector3 v)
    {
        velocity = v;
    }
    [Header("Hit Stun")]
    public Vector2 curHitAnim;
    public Vector2 targetHitAnim;

    public void GetHit(CharacterObject attacker)
    {
        AttackEvent curAtk = GameEngine.coreData.characterStates[attacker.currentState].attacks[attacker.currentAttackIndex];

        attacker.hitActive = 0;//careful cant hit 2 enemies
        //Vector3 targetOffset = transform.position;
        Vector3 nextKnockback = curAtk.knockback;
        //Vector3 knockOrientation = attacker.character.transform.forward;
        Vector3 knockOrientation = character.transform.position - attacker.character.transform.position;
        knockOrientation.Normalize();
        nextKnockback.x *= knockOrientation.x;
        Debug.Log("Knockback is: " + nextKnockback);
        //nextKnockback = knockOrientation*nextKnockback;
        SetVelocity(nextKnockback*0.7f);//dampen a bit
        targetHitAnim.x = curAtk.hitAnim.x;
        targetHitAnim.y = curAtk.hitAnim.y;

        Debug.Log("Combined Knockback is: " + nextKnockback + " Post Dampening is: " + nextKnockback * 0.7f);
        
        //curHitAnim.x = UnityEngine.Random.Range(-1f, 1f);//randomized for fun
        //curHitAnim.y = UnityEngine.Random.Range(-1f, 1f);

        curHitAnim = targetHitAnim * .25f;

        GameEngine.SetHitPause(curAtk.hitStop);
        hitStun = curAtk.hitStun;
        attacker.hitConfirm += 1;
        attacker.BuildMeter(10f);
        StartState(hitStunStateIndex);
        GlobalPrefab(0);
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
