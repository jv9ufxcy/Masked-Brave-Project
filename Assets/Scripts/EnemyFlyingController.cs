using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlyingController : MonoBehaviour
{
    private Player thePlayer;
    private EnemyHealthManager enemyHealth;

    [SerializeField] private float flightSpeed;
    [SerializeField] private float detectionRange;
    [SerializeField] private float shootingRange;
    [SerializeField] private Vector2 bulletVelocity;

    [SerializeField] private LayerMask whatIsShootable;
    [SerializeField] private bool isPlayerInRange;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletMuzzle;
    [SerializeField] private float fireRate=5;
    [SerializeField] private float timeToNextFire = 0f;

    private AudioManager audioManager;
    [SerializeField] private string bulletSound;

    // Use this for initialization
    void Start ()
    {
        thePlayer = FindObjectOfType<Player>();
        enemyHealth = GetComponent<EnemyHealthManager>();

        //enemy wont shoot yet
        timeToNextFire = fireRate;

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        MoveTowardsPlayer();
        ShootPlayer();
    }

    private void MoveTowardsPlayer()
    {
        //TODO: Add a state machine loop that counts down between standing still to shoot and then moving
        isPlayerInRange = Physics2D.OverlapCircle(transform.position, detectionRange, whatIsShootable);
        if (isPlayerInRange)
        {
            if (enemyHealth.CanMove)
            {
                transform.position = Vector3.MoveTowards(transform.position, thePlayer.transform.position, flightSpeed * Time.fixedDeltaTime);
            }
            else
            {
                //enemyHealth.Knockback();
            }
        }
    }
    private void ShootPlayer()
    {
        timeToNextFire -= Time.fixedDeltaTime;
        if (timeToNextFire<0)
        {
            //TODO: set an anim for the tell before the trigger for shooting
            //anim.SetTrigger("Shooting");

            //TODO: Flip Sprite Renderer during shooting
            //if player is to the right, shoot at the right side
            if (thePlayer.transform.position.x > transform.position.x && thePlayer.transform.position.x < transform.position.x + shootingRange )
            {
                GameObject newbullet = Instantiate(bulletPrefab, bulletMuzzle.position, Quaternion.identity);
                Debug.Log("Right Enemy Bullet fired");
                newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(bulletVelocity.x * transform.localScale.x, bulletVelocity.y);
                audioManager.PlaySound(bulletSound);
                timeToNextFire = fireRate;
            }
            //if player is to the left, shoot at the left side
            if (transform.localScale.x > 0 && thePlayer.transform.position.x < transform.position.x && thePlayer.transform.position.x > transform.position.x - shootingRange)
            {
                GameObject newbullet = Instantiate(bulletPrefab, bulletMuzzle.position, Quaternion.Euler(new Vector3(0, 0, -180)));
                Debug.Log("Left Enemy Bullet fired");
                newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(-bulletVelocity.x * transform.localScale.x, bulletVelocity.y);
                audioManager.PlaySound(bulletSound);
                timeToNextFire = fireRate;
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawLine(new Vector3(transform.position.x - shootingRange, transform.position.y, transform.position.z), new Vector3(transform.position.x + shootingRange, transform.position.y, transform.position.z));
    }
    //public enum DashState
    //{
    //    Shooting,
    //    Moving
    //}
}
