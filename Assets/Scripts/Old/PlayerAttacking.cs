using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacking : MonoBehaviour
{
    [SerializeField] private Player thePlayer;
    [SerializeField] private AttackState attackState;
    private EnemyHealthManager enemyHP;
    private Rigidbody2D myRB;
    private Animator currentAnim;

    private bool isOnGround;
    private int numberOfPresses = 0;
    private bool canAttack;
    [SerializeField] private int damageToGive;

    private bool isAttackKeyDown;
    private bool isComboA1KeyDown;
    private bool isComboA2KeyDown;

    [SerializeField] private float comboA1Movement = 2f;
    [SerializeField] private float comboA2Movement = 5f;

    [SerializeField] private float attackTimer = 1f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float comboA1Cooldown = 0.4f;
    [SerializeField] private float comboA2Cooldown = 0.85f;


    //DashSlash
    [SerializeField] private float UpVerticalDSSpeed = 18f;
    [SerializeField] private float DownVerticalDSSpeed = 12f;
    [SerializeField] private float horizontalDashSlashSpeed = 20f;
    [SerializeField] private float dashSlashMaxTime = 0.3f;
    private bool anySlashReady;
    private bool upSlashReady;
    private bool downSlashReady;
    private float savedGravity_UseProperty;

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

    private AudioManager audioManager;
    [SerializeField] private string attackingSound;
    //[SerializeField] private string comboA1Sound;
    [SerializeField] private string comboA2Sound;

    public bool IsAttacking()
    {
        return attackState != AttackState.Ready;
    }
    // Use this for initialization
    void Start ()
    {
        thePlayer = GetComponentInParent<Player>();
        myRB = GetComponentInParent<Rigidbody2D>();
        currentAnim = GetComponent<Animator>();

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        GetAttackInput();
        GetComboA1Input();
        GetComboA2Input();
    }
    
    private void GetAttackInput()
    {
        if (Input.GetButtonDown("Attack"))
        {
            isAttackKeyDown = true;
        }
    }
    private void GetComboA1Input()
    {
        if (Input.GetButtonDown("Attack") && attackState == AttackState.Cooldown && isOnGround)
        {
            isComboA1KeyDown = true;
        }
    }
    private void GetComboA2Input()
    {
        if (Input.GetButtonDown("Attack") && attackState == AttackState.CooldownA1 && isOnGround)
        {
            isComboA2KeyDown = true;
        }
    }
    private void FixedUpdate()
    {
        HandleAttacks();
        HandleBools();
    }

    private void HandleBools()
    {
        isOnGround = thePlayer.PlayerIsOnGround;
        anySlashReady = (upSlashReady || downSlashReady);
        if (isOnGround)
        {
            upSlashReady = false;
            downSlashReady = false;
            anySlashReady = false;
        }
    }

    private void HandleAttacks()
    {
        switch(attackState)
        {
            case AttackState.Ready:
                if (isAttackKeyDown&&!anySlashReady)
                {
                    attackState = AttackState.Attacking;
                }
                else if (isAttackKeyDown&&upSlashReady)
                {
                    attackState = AttackState.UpSlashAttack;
                }
                else if (isAttackKeyDown && downSlashReady)
                {
                    attackState = AttackState.DownSlashAttack;
                }
                break;

            case AttackState.Attacking:
                attackTimer = attackCooldown;
                if (attackTimer >= attackCooldown)
                {
                    currentAnim.SetTrigger("Attack");
                    audioManager.PlaySound(attackingSound);
                    attackTimer = attackCooldown;
                    attackState = AttackState.Cooldown;
                }
                break;

            case AttackState.Cooldown:
                isAttackKeyDown = false;
                attackTimer -= Time.fixedDeltaTime;
                if (isOnGround)
                {
                    myRB.velocity = new Vector2(myRB.velocity.x / 2f, myRB.velocity.y);
                }
                if (isComboA1KeyDown == true)
                    attackState = AttackState.AttackComboA1;

                ReturnToReady();
                break;

                //up dash slash
            case AttackState.UpSlashAttack:
                myRB.velocity = new Vector2(0,0);
                attackTimer = dashSlashMaxTime;
                if (attackTimer >= dashSlashMaxTime)
                {
                    if (thePlayer.CanAimRight)
                    {
                        myRB.AddForce(new Vector2(horizontalDashSlashSpeed, UpVerticalDSSpeed), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRB.AddForce(new Vector2(-horizontalDashSlashSpeed, UpVerticalDSSpeed), ForceMode2D.Impulse);
                    }

                    currentAnim.SetTrigger("UpKick");
                    audioManager.PlaySound(attackingSound);

                    attackState = AttackState.CooldownDashSlash;
                }
                break;
            //down dash slash
            case AttackState.DownSlashAttack:
                myRB.velocity = new Vector2(0, 0);
                attackTimer = dashSlashMaxTime;
                if (attackTimer >= dashSlashMaxTime)
                {
                    if (thePlayer.CanAimRight)
                    {
                        myRB.AddForce(new Vector2(horizontalDashSlashSpeed, -DownVerticalDSSpeed), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRB.AddForce(new Vector2(-horizontalDashSlashSpeed, -DownVerticalDSSpeed), ForceMode2D.Impulse);
                    }

                    currentAnim.SetTrigger("DownKick");
                    audioManager.PlaySound(attackingSound);

                    attackState = AttackState.CooldownDashSlash;
                }
                break;
            case AttackState.CooldownDashSlash:
                isAttackKeyDown = false;
                upSlashReady = false;
                downSlashReady = false;
                attackTimer -= Time.fixedDeltaTime;
                myRB.gravityScale = SavedGravity;
                ReturnToReady();
                break;

            //combo
            case AttackState.AttackComboA1:
                attackTimer = comboA1Cooldown;
                if (attackTimer >= comboA1Cooldown)
                {
                    if (thePlayer.CanAimRight)
                    {
                        myRB.AddForce(new Vector2(comboA1Movement, 0), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRB.AddForce(new Vector2(-comboA1Movement, 0), ForceMode2D.Impulse);
                    }

                    currentAnim.SetTrigger("AttackComboA1");
                    audioManager.PlaySound(attackingSound);

                    attackState = AttackState.CooldownA1;
                }
                break;

            case AttackState.CooldownA1:
                isAttackKeyDown = false;
                isComboA1KeyDown = false;
                attackTimer -= Time.fixedDeltaTime;

                myRB.velocity = new Vector2(myRB.velocity.x/2, myRB.velocity.y);

                if (isComboA2KeyDown == true)
                    attackState = AttackState.AttackComboA2;

                ReturnToReady();
                break;

            case AttackState.AttackComboA2:
                attackTimer = comboA2Cooldown;
                if (attackTimer >= comboA2Cooldown)
                {
                    if (thePlayer.CanAimRight)
                    {
                        myRB.AddForce(new Vector2(comboA2Movement, 0), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRB.AddForce(new Vector2(-comboA2Movement, 0), ForceMode2D.Impulse);
                    }
                    currentAnim.SetTrigger("AttackComboA2");
                    audioManager.PlaySound(comboA2Sound);

                    attackState = AttackState.CooldownA2;
                }
                break;
            case AttackState.CooldownA2:
                isAttackKeyDown = false;
                isComboA1KeyDown = false;
                isComboA2KeyDown = false;
                attackTimer -= Time.fixedDeltaTime;

                ReturnToReady();
                
                break;
        }
    }
    private void ReturnToReady()
    {
        if (attackTimer <= 0)
        {
            attackTimer = 0;
            attackState = AttackState.Ready;
        }
        upSlashReady = false;
        downSlashReady = false;
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
    public enum AttackState
    {
        Ready,
        Attacking,
        UpSlashAttack,
        DownSlashAttack,
        AttackComboA1,
        AttackComboA2,
        Cooldown,
        CooldownA1,
        CooldownA2,
        CooldownDashSlash,
    }
}
