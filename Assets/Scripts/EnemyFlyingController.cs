using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlyingController : MonoBehaviour
{
    private Player thePlayer;
    private EnemyHealthManager enemyHealth;
    private Rigidbody2D bubbleRB;

    private Animator bubbleAnim;
    [SerializeField] private string firingAnim;

    private AudioManager audioManager;
    [SerializeField] private string bulletSound;

    [SerializeField] private float flightSpeed;
    [SerializeField] private float detectionRange;
    [SerializeField] private float shootingRange;
    [SerializeField] private float bulletSpeed=20;

    [SerializeField] private LayerMask whatIsShootable;
    [SerializeField] private bool isPlayerInRange, isPlayerInShootingRange;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletMuzzle;
    [SerializeField] private float fireRate=5;
    [SerializeField] private float timeToNextFire = 0f;

    private enum FlyingState { idle, chase, firing, stunned };
    [SerializeField] private FlyingState _flyingState;


    // Use this for initialization
    void Start ()
    {
        thePlayer = FindObjectOfType<Player>();
        enemyHealth = GetComponent<EnemyHealthManager>();
        bubbleAnim = GetComponent<Animator>();
        bubbleRB = GetComponent<Rigidbody2D>();

        //enemy wont shoot yet
        timeToNextFire = fireRate;

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void Update()
    {
        switch (_flyingState)
        {
            case FlyingState.idle:
                transform.position = this.transform.position;
                isPlayerInRange = Physics2D.OverlapCircle(transform.position, detectionRange, whatIsShootable);
                if (isPlayerInRange)
                {
                    _flyingState = FlyingState.chase;
                }
                    break;
            case FlyingState.chase:
                isPlayerInShootingRange = Physics2D.OverlapCircle(transform.position, shootingRange, whatIsShootable);
                if (isPlayerInShootingRange)
                {
                    bubbleAnim.Play(firingAnim);
                    _flyingState = FlyingState.firing;
                }
                break;
            case FlyingState.firing:
                transform.position = this.transform.position;
                timeToNextFire -= Time.deltaTime;
                if (timeToNextFire < 0)
                {
                    ShootPlayer();
                }
                break;
            case FlyingState.stunned:
                bubbleAnim.SetBool("IsHurt", true);
                break;
            default:
                break;
        }
    }
    // Update is called once per frame
    void FixedUpdate ()
    {
        switch (_flyingState)
        {
            case FlyingState.idle:
                break;
            case FlyingState.chase:
                MoveTowardsPlayer();
                    break;
            case FlyingState.firing:
                break;
            case FlyingState.stunned:
                break;
            default:
                break;
        }
        
        
    }

    private void MoveTowardsPlayer()
    {
        //TODO: Add a state machine loop that counts down between standing still to shoot and then moving
        isPlayerInRange = Physics2D.OverlapCircle(transform.position, detectionRange, whatIsShootable);
        if (isPlayerInRange)
        {
            if (enemyHealth.CanMove)
            {
                transform.position = Vector2.MoveTowards(transform.position, thePlayer.transform.position, flightSpeed * Time.fixedDeltaTime);
            }
            else
            {
                //enemyHealth.Knockback();
            }
        }
    }
    private void ShootPlayer()
    {
        timeToNextFire = fireRate;
        audioManager.PlaySound(bulletSound);

        //Vector2 targetPos = new Vector2(thePlayer.transform.position.x, thePlayer.transform.position.y);
        Vector2 moveDirection = (thePlayer.transform.position - transform.position).normalized * bulletSpeed;

        GameObject newbullet = Instantiate(bulletPrefab, bulletMuzzle.position, Quaternion.identity);
        newbullet.GetComponent<HurtPlayerOnHit>().enemyHM = enemyHealth;
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(moveDirection.x, moveDirection.y);

        _flyingState = FlyingState.chase;
        //TODO: Flip Sprite Renderer during shooting
        //if player is to the right, shoot at the right side

    }
    public void OnDamaged()
    {
        _flyingState = FlyingState.stunned;
        bubbleAnim.SetBool("IsHurt", true);
    }
    public void OnRecovery()
    {
        bubbleRB.velocity = Vector2.zero;
        bubbleAnim.SetBool("IsHurt", false);
        _flyingState = FlyingState.idle;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawLine(new Vector3(transform.position.x - shootingRange, transform.position.y, transform.position.z), new Vector3(transform.position.x + shootingRange, transform.position.y, transform.position.z));
    }
}
