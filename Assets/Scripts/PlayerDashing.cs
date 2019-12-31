using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDashing : MonoBehaviour
{
    [SerializeField] private DashState dashState;
    [SerializeField] private float dashTimer;
    [SerializeField] private float maxDash = 0.2f;
    [SerializeField] private Vector2 savedVelocity;
    [SerializeField] private float dashSpeed = 24f;

    [SerializeField] private float horizontalInput /*, verticalInput*/;

    private bool isDashKeyDown=false;
    private float savedGravity_UseProperty;
    private Animator anim;
    private Rigidbody2D myRB;
    private Player thePlayer;

    [SerializeField] private Slider dashSlider;

    private AudioManager audioManager;
    [SerializeField] private string dashAnimation;

    public bool IsDashing()
    {
        return dashState == DashState.Dashing;
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

    private void Start()
    {
        //SavedGravity = myRB.gravityScale;
        myRB = GetComponentInParent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        thePlayer = GetComponentInParent<Player>();

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void Update()
    {
        GetMovementInput();
        GetDashInput();
    }

    private void GetDashInput()
    {
        if (Input.GetButtonDown("Dash") && thePlayer.NumberOfDashes>0 && (thePlayer.PlayerIsOnGround&&thePlayer.IsSwordmaster||!thePlayer.IsSwordmaster))
        {
            isDashKeyDown = true;
        }
        else
            isDashKeyDown = false;
    }
    private void GetMovementInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        /*verticalInput = Input.GetAxis("Vertical");*/
    }
    void FixedUpdate()
    {
        HandleDash();
    }


    private void HandleDash()
    {
        switch (dashState)
        {
            case DashState.Ready:
                if (isDashKeyDown)
                {
                    //Right
                    if (thePlayer.CanAimRight)
                    {
                        myRB.velocity = new Vector2(dashSpeed, 0);
                        audioManager.PlaySound("Dash");
                        anim.SetTrigger(dashAnimation);
                    }
                    //Left    
                    else if (!thePlayer.CanAimRight)
                    {
                        myRB.velocity = new Vector2(-dashSpeed, 0);
                        audioManager.PlaySound("Dash");
                        anim.SetTrigger(dashAnimation);
                    }

                    //savedVelocity = myRB.velocity;
                    thePlayer.NumberOfDashes--;
                    dashState = DashState.Dashing;
                }
                break;
            case DashState.Dashing:
                dashTimer += Time.fixedDeltaTime;
                if (dashTimer >= maxDash)
                {
                    
                    dashTimer = maxDash;
                    //myRB.velocity = savedVelocity;
                    dashState = DashState.Cooldown;
                }
                break;
            case DashState.Cooldown:
                dashTimer -= Time.fixedDeltaTime;
                if (dashTimer <= 0)
                {
                    dashTimer = 0;
                    dashState = DashState.Ready;
                }
                break;

        }
    }
}
#region
//bool isDashHorizontal = false;
//bool isDashDiagonal = false;
//bool isDashVertical = false;    
//bool horizontalInputDirection = Input.GetButtonDown("Horizontal");
//bool verticalInputDirection = Input.GetButtonDown("Vertical");

//Vector2 diagonalDirection = new Vector2(horizontalInputDirection, verticalInputDirection);
//diagonalDirection.Normalize();
//switch (dashState)
//{
//    case DashState.Ready:

//if (horizontalInputDirection&& Input.GetButtonDown("Dash"))
//{
//    isDashHorizontal = true;
//}
//else if (verticalInputDirection && Input.GetButtonDown("Dash"))
//{
//    isDashVertical = true;
//}
//else if (horizontalInputDirection&&verticalInputDirection && Input.GetButtonDown("Dash"))
//{

//}
//var isDashKeyDown = Input.GetButtonDown("Dash");
//if (isDashKeyDown)
//{
//    myRB.velocity = new Vector2(myRB.velocity.x * 3f, 0);
//    savedVelocity = myRB.velocity;
//    dashState = DashState.Dashing;
//}
//else if(isDashVertical)
//{
//    myRB.velocity = new Vector2(0, myRB.velocity.y * 3f);
//    savedVelocity = myRB.velocity;
//    dashState = DashState.Dashing;
//}
//else if (isDashDiagonal)
//{
//    myRB.velocity = new Vector2(myRB.velocity.x * 3f, myRB.velocity.y*3f);
//    savedVelocity = myRB.velocity;
//    dashState = DashState.Dashing;
//}
#endregion

public enum DashState
{
    Ready,
    Dashing,
    Cooldown
}
    #region
    //private class DashState
    //{
    //    public bool pressed;
    //    public float cooldownFrames;
    //    public int dashingFrames;
    //    public bool dashWithDirection;
    //    public Vector2 dashDir = Vector2.zero;
    //    public float distanceCalculated;
    //    public float distanceDashed;
    //    public bool force;
    //    public float gravityEnabledFrames;
    //}
    //private DashState _dashing = new DashState();
    ///// <summary>
    ///// Is dashing allowed?
    ///// </summary>
    //public bool enableDashes = true;

    ///// <summary>
    ///// How far the motor will dash.
    ///// </summary>
    //public float dashDistance = 3;

    ///// <summary>
    ///// How long the dash lasts in seconds.
    ///// </summary>
    //public float dashDuration = 0.2f;

    ///// <summary>
    ///// When the motor will be allowed to dash again after dashing. The cooldown begins at the end of a dash.
    ///// </summary>
    //public float dashCooldown = 0.76f;
    ///// <summary>
    ///// Delay (in seconds) before gravity is turned back on after a dash.
    ///// </summary>
    //public float endDashNoGravityDuration = 0.1f;

    //public Vector2 dashDirection
    //{
    //    get
    //    {
    //        return IsDashing() ? _dashing.dashDir : Vector2.zero;
    //    }
    //}

    ///// <summary>
    ///// Returns the amount of distance dashed. If not dashing then returns 0.
    ///// </summary>
    //public float distanceDashed
    //{
    //    get
    //    {
    //        return IsDashing() ? _dashing.distanceDashed : 0;
    //    }
    //}

    ///// <summary>
    ///// This is the distance calculated for dashed. Not be confused with distanceDashed. This doesn't care if the motor has
    ///// hit a wall.
    ///// </summary>
    //public float dashDistanceCalculated
    //{
    //    get
    //    {
    //        return IsDashing() ? _dashing.distanceCalculated : 0;
    //    }
    //}

    ///// <summary>
    ///// If the motor is currently able to dash.
    ///// </summary>
    //public bool canDash
    //{
    //    get { return _dashing.cooldownFrames < 0; }
    //}

    ///// <summary>
    ///// Delegate to attach to when the motor dashes.
    ///// </summary>
    //public Action onDash;

    ///// <summary>
    ///// Delegate to attach to when the motor's dash ends.
    ///// </summary>
    //public Action onDashEnd;

    ///// <summary>
    ///// Delegate to attach to when the motor jumps (ALL JUMPS!).
    ///// </summary>

    //public enum MotorState
    //{
    //    Dashing
    //}
    /////<summary>
    ///// Is the motor Dashing?
    /////</summary>
    //public bool IsDashing()
    //{
    //    return motorState == MotorState.Dashing;
    //}

    //public Vector2 velocity
    //{
    //    get
    //    {
    //        return IsDashing() ? _dashing.dashDir * GetDashSpeed() : _velocity;
    //    }
    //    set
    //    {
    //        _velocity = value;
    //    }
    //}

    ///// <summary>
    ///// Call this to get state information about the motor. This will be information such as if the object is in the air or on the
    ///// ground. This can be used to set the appropriate animations.
    ///// </summary>
    //public MotorState motorState { get; private set; }
    ///// <summary>
    ///// Reset the cooldown for dash.
    ///// </summary>
    //public void ResetDashCooldown()
    //{
    //    _dashing.cooldownFrames = -1;
    //}

    ///// <summary>
    ///// Call this to have the motor try to dash, once called it will be handled in the FixedUpdate tick.
    ///// This causes the object to dash along their facing (if left or right for side scrollers).
    ///// </summary>
    //public void Dash()
    //{
    //    _dashing.pressed = true;
    //    _dashing.dashWithDirection = false;
    //}

    ///// <summary>
    ///// Forces the motor to dash regardless if the motor thinks it is valid or not.
    ///// </summary>
    //public void ForceDash()
    //{
    //    Dash();
    //    _dashing.force = true;
    //}
    //private void ChangeState(MotorState newState)
    //{
    //    // no change...
    //    if (motorState == newState)
    //    {
    //        return;
    //    }
    //    if (IsDashing())
    //    {
    //        if (onDashEnd != null)
    //        {
    //            onDashEnd();
    //        }
    //    }

    //    // set
    //    motorState = newState;
    //    if (IsDashing())
    //    {
    //        if (onDash != null)
    //        {
    //            onDash();
    //        }
    //    }
    //}
    //private void StartDash()
    //{
    //    // Set facing now and it won't be set again during dash.
    //    SetFacing();

    //    if (!_dashing.dashWithDirection)
    //    {
    //        // We dash depending on our direction.
    //        _dashing.dashDir = facingLeft ? Vector2.left : Vector2.right;
    //    }

    //    _dashing.distanceDashed = 0;
    //    _dashing.distanceCalculated = 0;
    //    _previousLoc = _collider2D.bounds.center;

    //    // This will begin the dash this frame.
    //    _dashing.dashingFrames = GetFrameCount(dashDuration) - 1;
    //    _dashing.force = false;

    //    ChangeState(MotorState.Dashing);
    //}

    //private float GetDashSpeed()
    //{
    //    float normalizedTime = (float)(GetFrameCount(dashDuration) - _dashing.dashingFrames) /
    //        GetFrameCount(dashDuration);

    //    float speed = _dashDerivativeFunction(0, dashDistance, normalizedTime) / dashDuration;

    //    // Some of the easing functions may result in infinity, we'll uh, lower our expectations and make it maxfloat.
    //    // This will almost certainly be clamped.
    //    if (float.IsNegativeInfinity(speed))
    //    {
    //        speed = float.MinValue;
    //    }
    //    else if (float.IsPositiveInfinity(speed))
    //    {
    //        speed = float.MaxValue;
    //    }

    //    return speed;
    //}

    ///// <summary>
    ///// Send a direction vector to dash allow dashing in a specific direction.
    ///// </summary>
    ///// <param name="dir">The normalized direction of the dash.</param>
    //public void Dash(Vector2 dir)
    //{
    //    _dashing.pressed = true;
    //    _dashing.dashWithDirection = true;
    //    _dashing.dashDir = dir;
    //}

    ///// <summary>
    ///// Forces a dash along a specified direction.
    ///// </summary>
    ///// <param name="dir">The normalized direction of the dash.</param>
    //public void ForceDash(Vector2 dir)
    //{
    //    Dash(dir);
    //    _dashing.force = true;
    //}

    ///// <summary>
    ///// Call to end dash immediately.
    ///// </summary>
    //public void EndDash()
    //{
    //    // If dashing then end now.
    //    if (IsDashing())
    //    {
    //        _dashing.cooldownFrames = GetFrameCount(dashCooldown);
    //        _dashing.pressed = false;
    //        _dashing.gravityEnabledFrames = GetFrameCount(endDashNoGravityDuration);

    //        _velocity = _dashing.dashDir * GetDashSpeed();

    //        ChangeState(IsGrounded() ? MotorState.OnGround : MotorState.Falling);
    //    }
    //}
    //private void UpdateVelocity()
    //{
    //    // First, are we trying to dash?
    //    if (enableDashes &&
    //        (_dashing.pressed &&
    //        _dashing.cooldownFrames < 0 &&
    //        !IsDashing() ||
    //        _dashing.force))
    //    {
    //        StartDash();
    //    }

    //    _dashing.pressed = false;
    //}
    #endregion