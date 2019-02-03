using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRiderPunch : MonoBehaviour
{
    [SerializeField] private Player thePlayer;
    [SerializeField] private SpecialAttackState specialState;
    private Rigidbody2D myRB;
    private Animator currentAnim;
    private bool isSpecialKeyDown;
    private float savedGravity_UseProperty;

    [SerializeField] private int energyMeterCost = 5;

    [SerializeField] float jumpStrengthBraveStrike = 10f;
    [SerializeField] private float jumpDistBraveStrike=3f;
    private Vector2 jumpForceBraveStrike;

    [SerializeField] private float specialAttackCooldown = 0.8f;
    [SerializeField] private float attackTimer = 1f;

    private AudioManager audioManager;
    [SerializeField] private string riderPunchSound;


    public bool IsSpecialAttacking()
    {
        return specialState != SpecialAttackState.Ready;

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
    // Use this for initialization
    void Start()
    {
        myRB = GetComponentInParent<Rigidbody2D>();
        currentAnim = GetComponent<Animator>();
        jumpForceBraveStrike = new Vector2(jumpDistBraveStrike, jumpStrengthBraveStrike);

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetSpecialAttackInput();
        //specialSlider.value = specialAttackTimer;
    }
    private void GetSpecialAttackInput()
    {
        if (Input.GetButtonDown("Special") && thePlayer.CurrentSpecialEnergyMeter >= energyMeterCost)
        {
            isSpecialKeyDown = true;
        }
    }
    private void FixedUpdate()
    {
        HandleAttacks();
    }

    private void HandleAttacks()
    {
        switch (specialState)
        {
            case SpecialAttackState.Ready:
                if (isSpecialKeyDown)
                {
                    specialState = SpecialAttackState.Attacking;
                }
                break;

            case SpecialAttackState.Attacking:
                myRB.velocity = new Vector2(0, 0);
                attackTimer = specialAttackCooldown;
                thePlayer.SpendMeter(energyMeterCost);
                if (attackTimer >= specialAttackCooldown)
                {
                    if(thePlayer.CanAimRight)
                    {
                        myRB.AddForce(jumpForceBraveStrike, ForceMode2D.Impulse);
                        myRB.AddForce(new Vector2(jumpDistBraveStrike, jumpDistBraveStrike), ForceMode2D.Impulse);
                    }
                    else
                    {
                        myRB.AddForce(new Vector2(-jumpDistBraveStrike, jumpStrengthBraveStrike), ForceMode2D.Impulse);
                        myRB.AddForce(new Vector2(-jumpDistBraveStrike, jumpDistBraveStrike), ForceMode2D.Impulse);
                    }
                    
                    currentAnim.SetTrigger("RiderPunch");
                    audioManager.PlaySound(riderPunchSound);
                    specialState = SpecialAttackState.Cooldown;
                }
                break;

            case SpecialAttackState.Cooldown:
                ResetValues();
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    attackTimer = 0;
                    specialState = SpecialAttackState.Ready;
                }
                break;
        }


    }
    private void ResetValues()
    {
        isSpecialKeyDown = false;
    }
    public enum SpecialAttackState
    {
        Ready,
        Attacking,
        Cooldown
    }
}