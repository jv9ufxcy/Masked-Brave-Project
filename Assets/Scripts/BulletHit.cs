using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BulletHit : MonoBehaviour
{
    private Rigidbody2D bulletRB;
    private AudioManager audioManager;

    [SerializeField] private GameObject bulletHitEffect, blastBlightGO;
    [SerializeField] private string tagToHit = "Enemy";
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int blastBlight = 0;
    [SerializeField] private LayerMask whatLayersToHit;

    [SerializeField] private string bulletCollisionSound;
    [SerializeField] private bool shouldScreenshakeOnHit=false, shouldStopOnHit = true, canReflect=true;
    public CharacterObject character;
    // Use this for initialization
    void Start()
    {
        bulletRB = GetComponent<Rigidbody2D>();

        //audioManager = AudioManager.instance;
        //if (audioManager == null)
        //{
        //    Debug.LogError("No Audio Manager in Scene");
        //}
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagToHit))
        {
            //CharacterObject victim = other.transform.root.GetComponent<CharacterObject>();
            //if (victim != null)
            //    victim.GetHit(character);

            if (shouldScreenshakeOnHit)
                Screenshake();
            if (shouldStopOnHit)
            {
                RemoveForce();
                Destroy(gameObject,.2f);
            }
            if (blastBlight>0)
            {
                if (other.GetComponentInChildren<Blastblight>() != null)
                {
                    other.GetComponentInChildren<Blastblight>().AddBlast(blastBlight);
                }
                else
                {
                    GameObject bomb = Instantiate(blastBlightGO, other.transform);
                    bomb.GetComponent<Blastblight>().AddBlast(blastBlight);
                    bomb.transform.parent = other.transform;
                }
            }
            //Instantiate(bulletHitEffect, transform.position, transform.rotation);
        }
    }
    public void ReverseForce()
    {
        if (canReflect)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            bulletRB.velocity = -bulletRB.velocity;
            gameObject.layer = LayerMask.NameToLayer("Projectile");
        }
        else
            Destroy(gameObject);
    }
    private void RemoveForce()
    {
        //stops the bullet on collision
        if (bulletRB!=null)
            bulletRB.velocity = Vector2.zero;
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
}
