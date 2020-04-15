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

    private Animator characterAnim;
    private float direction;

    // Use this for initialization
    void Start()
    {
        characterAnim = GetComponent<Animator>();
        myRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //UpdateInputBuffer

        //Update Input
        UpdateInput();
        
        //Update State Machine
        UpdateState();

        //Update Physcis
        UpdatePhysics();
        //
        UpdateAnimator();
        UpdateIsOnGround();
    }
    void UpdateAnimator()
    {
        Vector3 latSpeed = new Vector3(velocity.x, 0, velocity.z);
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed) * 30f;
        animFallSpeed = velocity.y /** 30f*/;
        characterAnim.SetFloat("moveSpeed", aniMoveSpeed);
        characterAnim.SetFloat("aerialState", animAerialState);
        characterAnim.SetFloat("fallSpeed", animFallSpeed);
        FaceVelocity();
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
        UpdateStateEvents();

        prevStateTime = currentStateTime;
        currentStateTime++;

       
        if (currentStateTime >= myCurrentState.length)
        {
            if (myCurrentState.loop) { LoopState(); }
            else { EndState(); }
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
                
        }
    }

    private void FrontVelocity(float _pow)
    {
        velocity.x = _pow*direction;
    }

    void StickMove(float _pow)
    {
        float _mov = 0;
        if (Input.GetAxisRaw("Horizontal") > deadzone)
        {
            _mov = 1;
        }
        if (Input.GetAxisRaw("Horizontal") < -deadzone)
        {
            _mov = -1;
        }
            direction = _mov;

        //velocity.x += _mov * moveSpeed * _pow;
        velocity.x = _mov * moveSpeed * _pow;
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

        SetAnimation(GameEngine.coreData.characterStates[currentState].stateName);
    }
    void SetAnimation(string animName)
    {
        characterAnim.CrossFadeInFixedTime(animName, GameEngine.coreData.characterStates[currentState].blendRate);
        //characterAnim.CrossFadeInFixedTime(animName,0.13f);
        Debug.Log("Start " + animName);
        //characterAnim.Play(animName);
    }
    void UpdateInput()
    {
        foreach (InputCommand c in GameEngine.coreData.commands)
        {
            if (c.inputString != "")
            {
                if (Input.GetButtonDown(c.inputString))
                {
                    StartState(c.state);
                    break;
                    //Continue From Here!
                    //--> Hold state until out of command list and then check if you can Cancel or not
                }
            }
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

        //transform.position += velocity;
        //charact

    }
    void Move(Vector3 velocity)
    {
        myRB.velocity = velocity;
    }
    private void UpdateIsOnGround()
    {
        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, groundDetectRadius, whatCountsAsGround);
        isOnGround = groundObjects.Length > 0;

        //characterAnim.SetBool("Ground", isOnGround);
        //if (aerialFlag)
        //{
        //    GroundTouch();

        //}
        //else
        //    velocity.y += gravity * Time.deltaTime;
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
}
