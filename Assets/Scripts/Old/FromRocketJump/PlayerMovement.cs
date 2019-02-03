using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float jumpStrength = 1f;

    [SerializeField]
    float movementSpeed = 1f;
    [SerializeField]
    float airControl;

    [SerializeField]
    Transform groundDetectPoint;
    [SerializeField]
    Transform wallDetectPoint;

    [SerializeField]
    float groundDetectRadius = 0.2f;

    [SerializeField]
    LayerMask whatCountsAsGround;

    bool isFacingRight = true;

    private Animator anim;

    private float originalMovementSpeed;
    public float maxspeed=10;
    private bool isOnGround;
    public bool wallSliding;
    AudioSource audioSource;
    Rigidbody2D myRigidbody;
    private float horizontalInput;
    private bool shouldJump;
    private Vector2 jumpForce;


    //for shooting
    //public Transform muzzleRotateAnchor;
    //public Transform rocketMuzzle;
    //public GameObject bulletprefab;

    //float fireRate;
    //float nextFire = 0f;
    //[SerializeField]
    // private float bulletspeed=15;
    

    // Use this for initialization
    void Start ()
    {
        originalMovementSpeed = movementSpeed;
        myRigidbody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jumpForce = new Vector2(0, jumpStrength);
        audioSource = GetComponent<AudioSource>();

        
    }

    // Update is called once per frame
    void Update()
    {
        GetMovementInput();
        GetJumpInput();
        
        //player shooting
        //PlayerShoot();
    }
    //private void PlayerShoot()
    //{
    //    if (Input.GetAxisRaw("Fire1") > 0) fireBullet();
    //}
    //void fireBullet()
    //{
    //    if (Time.time > nextFire)
    //    {
    //        nextFire = Time.time + fireRate;
            //if (isFacingRight)
            //{
            //GameObject newbullet = Instantiate(bulletprefab, rocketMuzzle.position, muzzleRotateAnchor.rotation);
    //Rigidbody2D rb = newbullet.GetComponent<Rigidbody2D>();
    //newbullet.GetComponent<Rigidbody2D>().AddForce(rocketMuzzle.forward * bulletspeed, ForceMode2D.Impulse);

    // }
    //else if (!isFacingRight)
    //{
    //    Instantiate(bullet, rocketMuzzle.position, Quaternion.Euler(new Vector3(0, 0, 180f)));
    //}
    //      }
    //  }
    private void GetJumpInput()
    {
        if (Input.GetButtonDown("Jump") && isOnGround)
        {
            shouldJump = true;
        }
    }

    private void GetMovementInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        Move();
        Jump();
        UpdateIsOnGround();
        PassVSpeed();
        if (!isOnGround) return;
    }

    private void PassVSpeed()
    {
        anim.SetFloat("vSpeed", myRigidbody.velocity.y);
    }

    private void UpdateIsOnGround()
    {
        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, groundDetectRadius, whatCountsAsGround);
        anim.SetBool("Ground", isOnGround);
        //isOnGround = groundObjects.Length > 0;
        if (isOnGround)
        {
            anim.SetBool("Ground", true);
        }
        if (myRigidbody.velocity.y <0)
        {
            anim.SetBool("Ground", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isOnGround = true;
    }
    private void Jump()
    {
        if (shouldJump)
        {
            anim.SetBool("Ground", false);
            anim.SetFloat("vSpeed", myRigidbody.velocity.y);
            myRigidbody.AddForce(jumpForce, ForceMode2D.Impulse);
            //myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpStrength);
            isOnGround = false;
            shouldJump = false;
            audioSource.Play();
            //audioSource.clip = ;
        }
    }
    private void AdjustAirControl()
    {
        if (!isOnGround)
        {
            movementSpeed = originalMovementSpeed * airControl;//0-1
        }
        else
        {
            movementSpeed = originalMovementSpeed;
        }
    }
    private void Move()
    {
        if (Mathf.Abs(myRigidbody.velocity.x) < maxspeed )
        {
            myRigidbody.AddForce(new Vector2(horizontalInput * movementSpeed, 0), ForceMode2D.Impulse);
        }
        
        //myRigidbody.velocity = new Vector2(horizontalInput * movementSpeed, myRigidbody.velocity.y);
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        if (horizontalInput > 0 && !isFacingRight)
            Flip();
        else if (horizontalInput < 0 && isFacingRight)
            Flip();
    }
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        //Vector3 theScale = transform.localScale;
        GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
        //theScale.x *= -1;
        //transform.localScale = theScale;
    }
}
