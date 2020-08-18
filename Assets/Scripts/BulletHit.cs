using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BulletHit : MonoBehaviour
{
    private Rigidbody2D bulletRB;
    private AudioManager audioManager;
    [Tooltip("0 - straight, 1 - homing, 2 - target")]
    public int bulletType;
    public CharacterObject target;
    public EnemySpawn closestEnemy;
    public Vector3 targetPos, moveDirection;
    [SerializeField] private GameObject bulletHitEffect, blastBlightGO;
    [SerializeField] private string tagToHit = "Enemy", tagToCollide="Ground";
    [SerializeField] private float lifeTime = 2f, speed, targetRange=10f;
    [SerializeField] private int blastBlight = 0;
    [SerializeField] private LayerMask whatLayersToHit;

    [SerializeField] private string bulletCollisionSound;
    [SerializeField] private bool shouldScreenshakeOnHit=false, shouldStopOnHit = true, canReflect=true;
    public CharacterObject character;
    // Use this for initialization
    void Start()
    {
        bulletRB = GetComponent<Rigidbody2D>();
        if (bulletType!=0)
        {
            target = GameEngine.gameEngine.mainCharacter;
            targetPos = target.transform.position;
            moveDirection = (targetPos - transform.position).normalized * speed;
        }
        //audioManager = AudioManager.instance;
        //if (audioManager == null)
        //{
        //    Debug.LogError("No Audio Manager in Scene");
        //}
    }
    private void Update()
    {
        switch (bulletType)
        {
            case 0://fly straight
                break;
            case 1://home in
                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, speed);
                break;
            case 2://fly firectly at target
                transform.Translate(moveDirection* Time.deltaTime);
                break;
            case 3://home in on closest enemy
                closestEnemy = EnemySpawn.GetClosestEnemy(transform.position, targetRange);
                transform.position = Vector2.MoveTowards(transform.position, closestEnemy.transform.position, speed);
                break;
        }
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagToCollide) || collision.gameObject.CompareTag("Default"))
        {
            if (shouldStopOnHit)
            {
                RemoveForce();
                Destroy(gameObject, .2f);
            }
            if (canReflect)
            {
                Vector2 wallNormal = collision.contacts[0].normal;
                moveDirection = Vector2.Reflect(moveDirection, wallNormal).normalized;
            }
        }
    }
    public int projectileIndex;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagToHit)&&other.gameObject!=character.gameObject)
        {
            CharacterObject victim = other.transform.root.GetComponent<CharacterObject>();
            if (victim != null&&projectileIndex>0)
                victim.GetHit(character, projectileIndex);

            if (shouldScreenshakeOnHit)
                Screenshake();
            if (shouldStopOnHit)
            {
                RemoveForce();
                Destroy(gameObject,.2f);
                Instantiate(bulletHitEffect, transform.position, transform.rotation);
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
        }
    }
    public void ReverseForce()
    {
        if (canReflect)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            bulletRB.velocity = -bulletRB.velocity;
            speed = -speed;
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
