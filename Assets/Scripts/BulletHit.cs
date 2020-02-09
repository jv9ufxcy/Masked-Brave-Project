using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BulletHit : MonoBehaviour
{
    private Rigidbody2D bulletRB;
    private AudioManager audioManager;
    private HurtEnemyOnHit hurtEnemy;
    private HurtPlayerOnHit hurtPlayer;

    [SerializeField] private GameObject bulletHitEffect;
    [SerializeField] private string tagToHit = "Enemy";
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private LayerMask whatLayersToHit;

    [SerializeField] private string bulletCollisionSound;
    [SerializeField] private bool shouldScreenshakeOnHit=false, shouldStopOnHit = true;

    // Use this for initialization
    void Start()
    {
        bulletRB = GetComponent<Rigidbody2D>();
        hurtEnemy = GetComponent<HurtEnemyOnHit>();
        hurtPlayer = GetComponent<HurtPlayerOnHit>();

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void Update()
    {
        //Countdown to lifetime
        if (lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;
        }
        else if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(tagToHit))
        {
            if (shouldScreenshakeOnHit)
                Screenshake();
            if (shouldStopOnHit)
            {
                RemoveForce();
                Instantiate(bulletHitEffect, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
    public void ReverseForce()
    {
        tagToHit = "Enemy";
        hurtPlayer.enabled = false;
        hurtEnemy.enabled = true;
        bulletRB.velocity = -bulletRB.velocity;
        gameObject.layer = LayerMask.NameToLayer("Projectile");
    }
    private void RemoveForce()
    {
        //stops the bullet on collision
        bulletRB.velocity = Vector2.zero;
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
}
