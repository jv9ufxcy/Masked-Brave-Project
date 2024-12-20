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
    [Tooltip("0 - Straight, 1 - Homing, 2 - Target, 3 - Nearest Enemy, 4 - Follow Ground, 5 - Boomerang, 6 - Satellite, 7 - Curvy")]
    public int bulletType;
    public Vector2 direction, bulletVel;
    public CharacterObject target;
    public EnemySpawn closestEnemy;
    public Vector3 targetPos, velocity;
    [SerializeField] private GameObject bulletHitEffect, explosionEffect;
    [SerializeField] private string tagToHit = "Enemy", tagToCollide="Ground";
    public float lifeTime = 2f, speed, rotation,maxJumpHeight = 4f, timeToJumpApex=0.125f,maxJumpVelocity, gravity,velocityXSmoothing, targetRange=10f;
    private float aliveTimerCounting;
    //[SerializeField] private int attackState = 0;
    //[SerializeField] private LayerMask whatLayersToHit;

    //[SerializeField] private string bulletCollisionSound;
    [SerializeField] private bool shouldScreenshakeOnHit = false, shouldStopOnHit = true, canReflect = true, parabolicArc = false, cannotDeflect = false;
    public int bulletChain = 0, newBulletSpeed = 0;
    public CharacterObject character;
    private Controller2D thisBullet;
    private LaserHazard laser;
    // Use this for initialization
    void Awake()
    {
        bulletSprites = GetComponentsInChildren<SpriteRenderer>();
        bulletColls = GetComponents<BoxCollider2D>();
        bulletRB = GetComponent<Rigidbody2D>();
        laser = GetComponentInChildren<LaserHazard>();
        
        if (explosionEffect==null)
        {
            explosionEffect = bulletHitEffect;
        }
        if (bulletType==4)
        {
            thisBullet = GetComponent<Controller2D>();
            gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
            //minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);//thisBullet.FaceDir(direction.x);
        }
    }
    void Start()
    {
        audioManager = AudioManager.instance;
        if (bulletType!=0)
        {
            target = GameEngine.gameEngine.mainCharacter;
            targetPos = target.transform.position;
            velocity = (targetPos - transform.position).normalized * speed;
            if (bulletType==5)
            {
                boomerangTime = boomerangFlightTime;
                transform.rotation = Quaternion.Euler(0, 0, rotation);
            }
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
        if (parabolicArc)
        {
            bulletVel.y = maxJumpHeight;
        }
        if (laser != null && character != null)
        {
            laser.character = this.character;
        }
    }
    private void FixedUpdate()
    {
        if (!PauseManager.IsGamePaused && GameEngine.hitStop <= 0)
        {
            switch (bulletType)
            {
                case 0://fly straight
                    transform.Translate(velocity * speed * Time.fixedDeltaTime);
                    break;
                case 1://home in
                    transform.position = Vector2.MoveTowards(transform.position, target.transform.position, (speed / 80));
                    break;
                case 2://fly firectly at target
                    transform.Translate(velocity * Time.fixedDeltaTime);
                    break;
                case 3://home in on closest enemy
                    closestEnemy = EnemySpawn.GetClosestEnemy(transform.position, targetRange);
                    if (closestEnemy != null)
                        transform.position = closestEnemy.transform.position;
                    break;
                case 4: //follow ground
                    Controller2DMovement();
                    //thisBullet.FrontVelocity( speed * transform.localScale.x);
                    break;
                case 5:
                    Boomerang();
                    break;
                case 6:
                    Satellite();
                    break;
                case 7:
                    CurvyBullet();
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
    }

    private void Controller2DMovement()
    {
        float targetVelocityX = (speed/100) * transform.localScale.x;
        bulletVel.x = Mathf.SmoothDamp(bulletVel.x, targetVelocityX, ref velocityXSmoothing, 0);
        bulletVel.y += gravity * Time.fixedDeltaTime;
        thisBullet.Move(bulletVel, false);
        if (thisBullet.collisions.above || thisBullet.collisions.below)
        {
            if (thisBullet.collisions.slidingDownMaxSlope)
            {
                velocity.y += thisBullet.collisions.slopeNormal.y * -gravity * Time.fixedDeltaTime;
            }
            else
            {
                velocity.y = 0;
            }
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
        if ((other.CompareTag(tagToHit) || other.CompareTag(tagToCollide)) && other.gameObject!=character.gameObject)//if tag matches and character is not spawner of bullet
        {
            CharacterObject victimCO;
            IHittable victim;
            victim = other.GetComponent<IHittable>();
            if (victim == null)
                victim = other.transform.root.GetComponent<IHittable>();
            if (victim != null)
            {

                GameObject victimGO = victim.GetGameObject();
                victimCO = victimGO.GetComponent<CharacterObject>();

                if (victimGO != null)
                {
                    if (CheckReflect(victimCO))
                    {
                        ReflectBullet();
                    }
                    else
                    {
                        victim.Hit(character, projectileIndex, attackIndex);
                        HitEffects();
                    }
                }
                else
                    HitEffects();
            }
            else
                HitEffects();
        }
        if (bulletType == 5)//if boomer
        {
            if (boomerangStallTime <=0 && other.CompareTag(tagToHit))
            {
                OnDestroyGO();
            }
        }
    }

    private bool CheckReflect(CharacterObject victimCO)
    {
        if (victimCO != null)
        {
            if (victimCO.IsDefendingInState())//if enemy blocking
            {
                return true;
            }
            else
            {
                if (victimCO.GetIsInvulnerable())//if invul
                {
                    if (victimCO.GetComboValue() >= GameEngine.coreData.characterStates[projectileIndex].attacks[attackIndex].comboValue)//check invul level
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
        }
        else
            return false;
    }

    private bool CheckInvul(CharacterObject victimCO)
    {
        return victimCO.GetIsInvulnerable() && 
            victimCO.GetComboValue() >= GameEngine.coreData.characterStates[projectileIndex].attacks[attackIndex].comboValue;            
    }

    private void HitEffects()
    {
        if (shouldScreenshakeOnHit)
            Screenshake(shakeAmp, shakeTime);
        if (shouldStopOnHit)
        {
            OnDestroyGO();
        }
    }

    private bool isDestroyed = false;
    [SerializeField] private float destroyTimer = .1f;
    public bool isExplosion = false;
    public void OnDestroyGO()
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
            if (bulletHitEffect != null)
            {
                if (isExplosion)
                {
                    GameObject hitEffect = Instantiate(explosionEffect, transform.position, transform.rotation);
                    BombController bomb = hitEffect.GetComponent<BombController>();
                    bomb.character = character;
                    bomb.StartState();
                }
                else
                {
                    GameObject dissipateEffect = Instantiate(bulletHitEffect, transform.position, transform.rotation);
                }   
            }
            if (character!=null)
            {
                if (character.bulletsActiveInHierarchy.Contains(gameObject))
                {
                    character.bulletsActiveInHierarchy.Remove(gameObject);
                }
            }
            Destroy(gameObject, destroyTimer);
        }
    }
    public float boomerangFlightTime = .5f, boomerangStallTime = .5f, boomerangTime, angle, boomerSpeed = 10f/*(2 * Mathf.PI) / 1*/, radius = 1f, radX, radY;
    public Vector3 boomerangDist;
    public Vector3 followVel;
    private void Boomerang()
    {
        if (boomerangTime>0)
        {
            boomerangTime -= Time.fixedDeltaTime;
            transform.Translate(direction * speed * Time.fixedDeltaTime);
        }
        else
        {
            if (boomerangStallTime>0)
            {
                boomerangStallTime -= Time.fixedDeltaTime;
            }
            else
            {
                transform.Translate(direction * -boomerSpeed * Time.fixedDeltaTime);
            }
        }
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
        
    }
    private void CurvyBullet()
    {
        angle += speed * Time.fixedDeltaTime; //if you want to switch direction, use -= instead of +=
        followVel.x = Mathf.Cos(angle) * radX;
        followVel.y = Mathf.Sin(angle) * radY;
        SetVelocity(followVel);
    }
    private void Satellite()
    {
        angle += speed * Time.deltaTime;

        var offset = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad)) * radius;
        transform.position = (Vector2)character.transform.position + offset;
    }
    public void SetStartingAngle(float iceIndex, float totalSatellites)
    {
        float spacing = 360 / totalSatellites;//how much space should be between each block?
        angle = iceIndex * spacing;//set the angle to be evenly spaced based on which number it is and where it should be on the circle
    }
    private void SetVelocity(Vector3 vel)
    {
        transform.Translate(vel);
    }
    [SerializeField] private string deflectSound = "Enemy/Roy/Block";
    private void ReflectBullet()
    {
        if (cannotDeflect)
        {
            return;
        }
        //Debug.Log("bulletReflected");
        float newRotation = 135*Mathf.Sign(transform.localScale.x);
        transform.rotation = Quaternion.Euler(0, 0, rotation+newRotation);
        //velocity *= -1;
        //velocity += Vector3.up;
        //bulletVel*= -1;
        isExplosion = false;
        if (audioManager!=null)
        {
            audioManager.PlaySound(deflectSound);
        }

    }
    public void ReverseForce()
    {
        if (canReflect)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

            gameObject.layer = LayerMask.NameToLayer("Projectile");
        }
        else
            OnDestroyGO();
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
