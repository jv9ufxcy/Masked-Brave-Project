﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BulletHit : MonoBehaviour
{
    private Rigidbody2D bulletRB;
    private SpriteRenderer[] bulletSprites;
    private BoxCollider2D[] bulletColls;
    private AudioManager audioManager;
    [Tooltip("0 - straight, 1 - homing, 2 - target")]
    public int bulletType;
    public Vector2 direction;
    public CharacterObject target;
    public EnemySpawn closestEnemy;
    public Vector3 targetPos, velocity;
    [SerializeField] private GameObject bulletHitEffect, bulletChild;
    [SerializeField] private string tagToHit = "Enemy", tagToCollide="Ground";
    public float lifeTime = 2f, speed, rotation, gravity=-2f, targetRange=10f;
    [SerializeField] private int attackState = 0;
    [SerializeField] private LayerMask whatLayersToHit;

    [SerializeField] private string bulletCollisionSound;
    [SerializeField] private bool shouldScreenshakeOnHit = false, shouldStopOnHit = true, canReflect = true, parabolicArc = false;
    public int bulletChain = 0, newBulletSpeed = 0;
    public CharacterObject character, thisBullet;
    // Use this for initialization
    void Awake()
    {
        bulletSprites = GetComponentsInChildren<SpriteRenderer>();
        bulletColls = GetComponents<BoxCollider2D>();
        bulletRB = GetComponent<Rigidbody2D>();

        if (bulletType==4)
        {
            thisBullet = GetComponent<CharacterObject>();
            thisBullet.FaceDir(direction.x);
        }
    }
    void Start()
    {
        if (bulletType!=0)
        {
            target = GameEngine.gameEngine.mainCharacter;
            targetPos = target.transform.position;
            velocity = (targetPos - transform.position).normalized * speed;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
        if (parabolicArc)
        {
            thisBullet.Jump(1);
        }
    }
    private void FixedUpdate()
    {
        switch (bulletType)
        {
            case 0://fly straight
                transform.Translate(velocity * speed * Time.fixedDeltaTime);
                break;
            case 1://home in
                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, (speed/80));
                break;
            case 2://fly firectly at target
                transform.Translate(velocity * Time.fixedDeltaTime);
                break;
            case 3://home in on closest enemy
                closestEnemy = EnemySpawn.GetClosestEnemy(transform.position, targetRange);
                if (closestEnemy!=null)
                    transform.position = closestEnemy.transform.position;
                break;
            case 4: //follow ground

                thisBullet.FrontVelocity( speed * transform.localScale.x);
                break;
            case 5:
                if (boomerangStartTime > 0)
                {
                    transform.rotation = Quaternion.Euler(0, 0, rotation);
                    boomerangStartTime--;
                    direction.y += ((target.transform.position.y + target.transform.position.normalized.y) - transform.position.y) * speedDampen;
                    transform.Translate(direction * speed * Time.fixedDeltaTime);
                }
                else
                { 
                    Boomerang(); 
                }
                break;
        }
        //Countdown to lifetime
        if (lifeTime > 0)
        {
            lifeTime -= Time.fixedDeltaTime;
        }
        else if (lifeTime <= 0)
        {
            OnDestroyGO();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(tagToCollide))
        {
            if (shouldStopOnHit)
            {
                OnDestroyGO();
            }
            if (canReflect)
            {
                Vector2 wallNormal = collision.contacts[0].normal;
                velocity = Vector2.Reflect(velocity, wallNormal).normalized;
            }
        }
    }
    public int projectileIndex, attackIndex = 0;
    public float shakeAmp = 1f, shakeTime = .2f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagToHit)&&other.gameObject!=character.gameObject)
        {
            IHittable victim = other.transform.root.GetComponent<IHittable>();
            if (victim != null&&projectileIndex>0)
                victim.Hit(character, projectileIndex, attackIndex);

            if (shouldScreenshakeOnHit)
                Screenshake(shakeAmp,shakeTime);
            if (shouldStopOnHit)
            {
                OnDestroyGO();
            }
        }
        if (bulletType==5)
        {
            if (boomerangStartTime <=0 && other.CompareTag(tagToCollide))
            {
                OnDestroyGO();
            }
        }
    }
    private bool isDestroyed = false;
    [SerializeField] private float destroyTimer = .1f;
    private void OnDestroyGO()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            foreach (SpriteRenderer s in bulletSprites)
            {
                s.color = Color.clear;
            }
            foreach (BoxCollider2D s in bulletColls)
            {
                s.enabled = false;
            }
            bulletRB.isKinematic = true;
            RemoveForce();
            Instantiate(bulletHitEffect, transform.position, transform.rotation);
            if (thisBullet !=null)
            {
                thisBullet.StartStateFromScript(attackState);
            }
            Destroy(gameObject, destroyTimer);
        }
    }
    public float speedDampen = 0.01f, boomerangStartTime = 30f, angle, boomerSpeed = 10f/*(2 * Mathf.PI) / 1*/, radius=1f;
    public Vector3 boomerangDist;
    public Vector3 followVel;
    private void Boomerang()
    {
        //if (Mathf.Abs(target.transform.position.x - transform.position.x)>=boomerangDist.x)
        //{
        //    followVel.x += (target.transform.position.x - transform.position.x) * speedDampen;
        //    Mathf.Clamp(followVel.x, -.25f, .25f);
        //}
        //if (Mathf.Abs(target.transform.position.y - transform.position.y)>=boomerangDist.y)
        //{
        //    followVel.y += ((target.transform.position.y + target.transform.position.normalized.y) - transform.position.y ) * speedDampen;
        //    Mathf.Clamp(followVel.y, -.25f, .25f);
        //}
        angle += boomerSpeed * Time.fixedDeltaTime; //if you want to switch direction, use -= instead of +=
        followVel.x = Mathf.Cos(angle) * radius;
        followVel.y = Mathf.Sin(angle) * radius;
        SetVelocity(followVel);
    }
    private void SetVelocity(Vector3 vel)
    {
        transform.Translate(vel);
    }
    public void ReverseForce()
    {
        if (canReflect)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            
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
    private void Screenshake(float amp, float time)
    {
        CinemachineShake.instance.ShakeCamera(amp, time);
    }
}