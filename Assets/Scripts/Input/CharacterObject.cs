using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterObject : MonoBehaviour
{

    public Vector3 velocity;

    public float gravity = -0.01f;
    public float aniMoveSpeed;

    public Vector3 friction = new Vector3(0.95f, 0.99f, 0.95f);

    public Rigidbody2D myRB;

    public int currentState;
    public float currentStateTime;
    public float prevStateTime;

    public GameObject character;
    public GameObject draw;
    public Animator characterAnim;
    private float direction;
    public enum ControlType { AI, PLAYER };
    public ControlType controlType;

    public Hitbox hitbox;
    public bool canCancel;
    public int hitConfirm;

    public InputBuffer inputBuffer = new InputBuffer();

    // Use this for initialization
    void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
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
            UpdateAnimator();
            UpdateIsOnGround();
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

        if (hitStun<=0)
        {
            FaceVelocity();
        }
    }
    void CameraRelativeStickMove(float _val)
    {
        Vector3 velHelp = new Vector3(0, 0, 0);
        Vector3 velDir;

        //leftStick = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        if ((leftStick.x > deadzone || leftStick.x < -deadzone || leftStick.y > deadzone || leftStick.y < -deadzone))
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
    void FaceVelocity()
    {
        if (CheckVelocityDeadZone())
        {
            if (direction > 0)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (direction < 0)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
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
        foreach(StateEvent _ev in GameEngine.coreData.characterStates[currentState].events)
        {

            //if (prevStateTime <= _ev.start && currentStateTime == _ev.start)
            if (currentStateTime >= _ev.start && currentStateTime <= _ev.end)
            {
                DoEventScript(_ev.script, _ev.variable);
            }
        }
    }
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
    void GlobalPrefab(float _index)
    {
        GameEngine.GlobalPrefab((int)_index, gameObject);
    }
    private void FrontVelocity(float _pow)
    {
        velocity.x = _pow*direction;
    }
    public Vector2 leftStick;
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
            direction = _mov;
            //velocity.x += _mov * moveSpeed * _pow;
            velocity.x = _mov * moveSpeed * _pow;
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
    void UpdateInput()
    {
        leftStick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        inputBuffer.Update();

        bool startState = false;
        foreach (InputCommand c in GameEngine.coreData.commands)//go through each command
        {
            if (startState){break; }
            foreach (InputBufferItem bItem in inputBuffer.inputList)
            {
                if (startState){break; }
                foreach (InputStateItem bState in bItem.buffer)
                {
                    if (c.inputString == bItem.button)
                    {
                        if (bState.CanExecute())
                        {
                            if (canCancel)
                            {
                                startState = true;
                                bState.used = true;
                                StartState(c.state);
                                break;
                            }
                        }
                    }
                }
            }
                //if (c.inputString != "")
                //{
                //    //for (int b = 0; b < inputBuffer.inputList[c].buffer.Count; b++)
                //    if (Input.GetButtonDown(GameEngine.coreData.commands[c].inputString))
                //    {
                //        if (canCancel)
                //        {
                //            StartState(c.state);
                //            break;
                //            //Continue From Here!
                //            //--> Hold state until out of command list and then check if you can Cancel or not
                //        }
                //    }
                //}
            
        }
        
    }
    public bool CheckVelocityDeadZone()
    {
        if (velocity.x > 0.001f) { return true; }
        if (velocity.x < -0.001f) { return true; }
        if (velocity.z > 0.001f) { return true; }
        if (velocity.z < -0.001f) { return true; }
        return false;
    }

    private bool aerialFlag,isOnGround;
    [SerializeField] private float aerialTimer,groundDetectRadius, animAerialState,animFallSpeed;
    [SerializeField]
    private Transform groundDetectPoint;

    [SerializeField]
    private LayerMask whatCountsAsGround;
    void UpdatePhysics()
    {
        if (isOnGround)
        {
            aerialFlag = false;
            aerialTimer = 0;
            animAerialState = 0f;
            //velocity.y *= 0.3f;
        }
        else
        {
            if (!aerialFlag)
            {
                aerialTimer++;
            }
            if (aerialTimer >= 6)//coyote time
            {
                aerialFlag = true;
                if (animAerialState<=1f)
                {
                    animAerialState += 0.1f;
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
    private void UpdateIsOnGround()
    {
        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, groundDetectRadius, whatCountsAsGround);
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
        StartState(6);//hitstun state in coreData
        GlobalPrefab(0);
    }
    public float hitStun;
    public void GettingHit()
    {
        hitStun--;
        if (hitStun<=0)
        {
            EndState();
        }
        curHitAnim += (targetHitAnim - curHitAnim) * .1f;//blends for 3D games
    }
}
