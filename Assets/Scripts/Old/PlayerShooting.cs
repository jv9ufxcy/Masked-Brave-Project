using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{

    [SerializeField] private Vector3 bulletOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector2 velocity;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int meterToSpend = 5;
    [SerializeField] private float fireRate=3.3f;
    private float timeToNextFire = 0f;



    [SerializeField] private bool isMuzzleFacingRight;
    [SerializeField] private Transform bulletMuzzle;

    private Animator currentAnim;
    [SerializeField] private Player playerAmmo;

    private AudioManager audioManager;
    [SerializeField] private string bulletSound;
    // Use this for initialization
    private void Start()
    {
        currentAnim = GetComponent<Animator>();
        playerAmmo = GetComponentInParent<Player>();
        
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    // Update is called once per frame
    void Update ()
    {
        isMuzzleFacingRight = playerAmmo.CanAimRight;
        PlayerShoot();
	}
    private void PlayerShoot()
    {
        if (Input.GetButtonDown("Attack"))
            FireBullet();
    }
    void FireBullet()
    {
        if (Time.time > timeToNextFire && playerAmmo.CurrentSpecialEnergyMeter >= meterToSpend)
        {
            timeToNextFire = Time.time + 1 / fireRate;
            currentAnim.SetTrigger("Shooting");
            playerAmmo.SpendMeter(meterToSpend);
            if (isMuzzleFacingRight)
                ShootFromRightMuzzle();
            else
                ShootFromLeftMuzzle();            
        }
    }

    private void ShootFromRightMuzzle()
    {
        GameObject newbullet = Instantiate(bulletPrefab, bulletMuzzle.position, Quaternion.identity);
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(velocity.x * transform.localScale.x, velocity.y);
        audioManager.PlaySound(bulletSound);
    }
    private void ShootFromLeftMuzzle()
    {
        GameObject newbullet = Instantiate(bulletPrefab, bulletMuzzle.position, Quaternion.Euler(new Vector3(0, 0, -180)));
        newbullet.GetComponent<Rigidbody2D>().velocity = new Vector2(-velocity.x * transform.localScale.x, velocity.y);
        audioManager.PlaySound(bulletSound);
    }
}
