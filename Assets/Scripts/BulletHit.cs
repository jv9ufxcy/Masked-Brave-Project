using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHit : MonoBehaviour
{
    private Rigidbody2D bulletRB;
    [SerializeField] private GameObject bulletHitEffect;

    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private LayerMask whatLayersToHit;

    private AudioManager audioManager;
    [SerializeField] private string bulletCollisionSound;

    // Use this for initialization
    void Start()
    {
        bulletRB = GetComponent<Rigidbody2D>();

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void FixedUpdate()
    {
        //Countdown to lifetime
        if (lifeTime > 0)
        {
            lifeTime -= Time.fixedDeltaTime;
        }
        else if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
            RemoveForce();
            //TODO: figure out the proper way to instantiate so that shooting on top of an enemy doesn't launch the enemy into the player.
            Instantiate(bulletHitEffect, transform.position, transform.rotation);
            //Debug.Log("Bullet has hit "+ other.name);
            Destroy(gameObject);
    }
    private void RemoveForce()
    {
        //stops the bullet on collision
        bulletRB.velocity = new Vector2(0, 0);
    }
}
