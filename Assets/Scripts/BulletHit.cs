using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BulletHit : MonoBehaviour
{
    private Rigidbody2D bulletRB;
    [SerializeField] private GameObject bulletHitEffect;

    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private LayerMask whatLayersToHit;

    private AudioManager audioManager;
    [SerializeField] private string bulletCollisionSound;
    [SerializeField] private bool shouldScreenshakeOnHit=false;

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
        if (shouldScreenshakeOnHit)
            Screenshake();
        RemoveForce();
        Instantiate(bulletHitEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
    private void RemoveForce()
    {
        //stops the bullet on collision
        bulletRB.velocity = new Vector2(0, 0);
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
}
