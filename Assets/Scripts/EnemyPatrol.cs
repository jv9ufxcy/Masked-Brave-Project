using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    private Player thePlayer;
    private EnemyHealthManager enemyHealth;
    private Animator enemyAnim;

    [SerializeField] Transform wallDetectPoint;
    [SerializeField] Transform groundDetectPoint;
    [SerializeField] float DetectRadius = 0.2f;
    [SerializeField] LayerMask whatCountsAsWall;

    [SerializeField] Transform edgeDetectPoint;

    private AudioManager audioManager;

    private bool isFacingRight;
    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;

    [SerializeField] private LayerMask whatIsShootable;
    [SerializeField] private bool isShootableInRange;

    //grounded
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpStrength;
    [SerializeField] private bool moveRight;
    [SerializeField] private bool shouldEnemyWalkOffEdge;
    private bool isOnGround;
    private Vector2 jumpForce;

    // Use this for initialization
    void Start ()
    {
        jumpForce = new Vector2(0, jumpStrength);
        thePlayer = FindObjectOfType<Player>();
        enemyRB = GetComponent<Rigidbody2D>();
        enemyRend = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealthManager>();
        enemyAnim = GetComponent<Animator>();
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void Update()
    {
        
    }
    // Update is called once per frame
    void FixedUpdate ()
    {
        if (enemyHealth.CanMove)
            TurnEnemyAround();
        EnemyMovement();
        PassAnimationSpeed();
	}

    private void PassAnimationSpeed()
    {
        enemyAnim.SetFloat("vSpeed", enemyRB.velocity.y);
        enemyAnim.SetFloat("hSpeed", enemyRB.velocity.x);

        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, DetectRadius, whatCountsAsWall);
        isOnGround = groundObjects.Length > 0;

        enemyAnim.SetBool("Ground", isOnGround);
        if (isOnGround)
        {
            enemyAnim.SetBool("Ground", true);
        }
        if (enemyRB.velocity.y < 0)
        {
            enemyAnim.SetBool("Ground", false);
        }
    }
    private void EnemyMovement()
    {
        if (enemyHealth.CanMove)
        {
            if (moveRight)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
                enemyRB.velocity = new Vector2(moveSpeed, enemyRB.velocity.y);
            }
            else
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
                enemyRB.velocity = new Vector2(-moveSpeed, enemyRB.velocity.y);
            }
        }
        else
        {
            //enemyHealth.Knockback();
            enemyAnim.SetTrigger("Damaged");
        }
        
    }
    public void Jump()
    {
        if (isOnGround)
        {
            enemyAnim.SetBool("Ground", false);
            enemyAnim.SetFloat("vSpeed", enemyRB.velocity.y);
            enemyRB.AddForce(jumpForce, ForceMode2D.Impulse);
            isOnGround = false;
        }
    }

    private void Flip()
    {
        moveRight = !moveRight;
        isFacingRight = !isFacingRight;
    }
    private void TurnEnemyAround()
    {
        bool notAtEdge = Physics2D.OverlapCircle(edgeDetectPoint.position, DetectRadius, whatCountsAsWall);
        bool hittingWall = Physics2D.OverlapCircle(wallDetectPoint.position, DetectRadius, whatCountsAsWall);
        if (hittingWall||(!notAtEdge&&!shouldEnemyWalkOffEdge))
            Flip();

    }
    public enum EnemyType
    {
        Beetle,
        Dragonfly,
        Roy
    }
}
