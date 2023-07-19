using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BulletHit : MonoBehaviour
{
    private Rigidbody2D bulletRB;
    private SpriteRenderer[] bulletSprites;
    private BoxCollider2D[] bulletColls;
    private AudioManager audioManager;
    [Tooltip("0 - straight, 1 - homing, 2 - target, 3 - nearest enemy, 4 - follow ground, 5 - boomerang, 6 - satellite")]
    public int bulletType;
    public Vector2 direction, bulletVel;
    public CharacterObject target;
    public EnemySpawn closestEnemy;
    public Vector3 targetPos, velocity;
    [SerializeField] private GameObject bulletHitEffect, bulletChild;
    [SerializeField] private string tagToHit = "Enemy", tagToCollide="Ground";
    public float lifeTime = 2f, speed, rotation,maxJumpHeight = 4f, timeToJumpApex=0.125f,maxJumpVelocity, gravity,velocityXSmoothing, targetRange=10f;
    //[SerializeField] private int attackState = 0;
    //[SerializeField] private LayerMask whatLayersToHit;

    //[SerializeField] private string bulletCollisionSound;
    [SerializeField] private bool shouldScreenshakeOnHit = false, shouldStopOnHit = true, canReflect = true, parabolicArc = false;
    public int bulletChain = 0, newBulletSpeed = 0;
    public CharacterObject character;
    private Controller2D thisBullet;
    // Use this for initialization
    void Awake()
    {
        bulletSprites = GetComponentsInChildren<SpriteRenderer>();
        bulletColls = GetComponents<BoxCollider2D>();
        bulletRB = GetComponent<Rigidbody2D>();

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
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
        if (parabolicArc)
        {
            bulletVel.y = maxJumpHeight;
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
                case 6:
                    Satellite();
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
        if (other.CompareTag(tagToHit)&&other.gameObject!=character.gameObject)
        {
            CharacterObject victimCO;
            IHittable victim;
            victim = other.GetComponent<IHittable>();
            if (victim == null)
                victim = other.transform.root.GetComponent<IHittable>();

            GameObject victimGO = victim.GetGameObject();
            victimCO = victimGO.GetComponent<CharacterObject>();
            if (victimCO != null)
            {
                if (victimCO.IsDefendingInState())//if enemy blocking
                {
                    ReflectBullet();
                }
                else
                {
                    if (victimCO.GetIsInvulnerable())//if invul
                    {
                        if (victimCO.GetComboValue() >= GameEngine.coreData.characterStates[projectileIndex].attacks[attackIndex].comboValue)//check invul level
                        {
                            ReflectBullet();
                        }                        
                    }
                    else
                    {
                        HitEffects();
                    }
                }
            }
            if (victimGO != null)
            {
                victim.Hit(character, projectileIndex, attackIndex);
                if (victimCO==null) HitEffects();
            }
        }
        if (other.gameObject.CompareTag(tagToCollide))
        {
            if (shouldStopOnHit)
            {
                OnDestroyGO();
            }
        }
        if (bulletType == 5)//if boomer
        {
            if (boomerangStartTime <=0 && other.CompareTag(tagToCollide))
            {
                OnDestroyGO();
            }
        }
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
            if (bulletHitEffect != null)
            {
                GameObject hitEffect = Instantiate(bulletHitEffect, transform.position, transform.rotation);
                if (isExplosion)
                {
                    BombController bomb = hitEffect.GetComponent<BombController>();
                    bomb.character = character;
                    bomb.StartState();
                }
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
        //Debug.Log("bulletReflected");
        transform.rotation = Quaternion.Euler(0, 0, rotation+135);
        //velocity *= -1;
        //velocity += Vector3.up;
        //bulletVel*= -1;
        audioManager.PlaySound(deflectSound);
        //play sound effect
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
