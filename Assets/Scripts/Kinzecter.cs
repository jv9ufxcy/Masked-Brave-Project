using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class Kinzecter : MonoBehaviour
{
    private Rigidbody2D kzRB;
    private SpriteRenderer kzSprite;
    private Collider2D kzColl;
    [SerializeField] private Player player;

    [Header("Kinzecter Stats")]
    [SerializeField] private int kzDamage = 2;
    [SerializeField] private float kzSpeed = 20f, kzRecallSpeed = 30f, returnDistance = 5f, targetNextEnemyDistance = 15f, minLethalSpeed = 3f;
    [SerializeField] private ParticleSystem kinzecterParticles, hpPS, ammoPS, energyPS;
    [SerializeField] private bool shouldScreenshakeOnHit;

    private AudioManager audioManager;
    private BossPatrolManager boss;
    private EnemyHealthManager enemy;

[SerializeField] private ThrowingState kState;
    private bool shouldFly, essenceAdded=false;
    private float startScale, flightSpeed;
    [SerializeField] private int hpStock, eStock, ammoStock;

    private bool isTooSlow;
    private bool newEnemyTargeted=false;
    private bool coroutineStarted=false;
    Vector3 nextTargetDir;

    public DamageEffect _effect;
    public enum DamageEffect { stun, knockback, launch }

    private enum ThrowingState 
    {
        WithPlayer, Thrown, Recalling,
    }
    [Header("Boomerang")]
    public bool hasBeenThrown = false;//collision
    public bool doCollision = false;//collision
    public bool boomerangFlag = false;//ai on
    public float boomerangTime = 20;//time spent turning back to the player
    public float orient = 0; //direction boomerang turns  in the boomerang state
    public float turnAmount = 0; //how much boomerang has turned, used to end the state
    public float direction; //
    public int state;
    public Vector3 velocity;
    public CharacterObject thrower;
    public Vector3 friction = new Vector3(0.95f, 0.99f, 0.95f);
    private void Awake()
    {
        kzSprite = GetComponent<SpriteRenderer>();
        kzRB = GetComponent<Rigidbody2D>();
        kzColl = GetComponent<Collider2D>();
        //player = GetComponent<Player>();
        //audioManager = GetComponent<AudioManager>();
        //audioManager = AudioManager.instance;
        //if (audioManager == null)
        //{
        //    Debug.LogError("No Audio Manager in Scene");
        //}

        kState = ThrowingState.Recalling;
    }
    private void Start()
    {
        startScale = transform.localScale.x;
    }
    private void Update()
    {
        if (hasBeenThrown==false)
        {
            //reset vars
            boomerangFlag = false;
            boomerangTime = 10;
            turnAmount = 0;
            state = 0;
            direction = 0;
            doCollision = true;
        }
        else
        {
            if (boomerangFlag)
            {
                doCollision = false;
                switch (state)
                {
                    case 0://spin state
                        var rspd = 50;//rotation speed
                        velocity += transform.right.normalized;
                        transform.Translate(velocity);
                        //scale
                        direction += rspd * orient * Time.deltaTime;//gradually change direction to create path of boomerang
                        if (turnAmount >= 190)//once boomerange rotates 190 degrees it goes to the returning state
                        {
                            state = 1;
                        }
                        else
                            turnAmount += rspd * Time.deltaTime;//increment degrees we have rotated
                        break;
                    case 1://throw state
                        var pdir = transform.position - (thrower.transform.position + Vector3.down * 16);//direction to face towards the thrower
                        transform.forward = Vector3.RotateTowards(transform.forward, pdir, 3f * Time.deltaTime,0f); //smoothly shift current dir to previous dir
                        velocity += transform.right.normalized;//move
                        transform.Translate(velocity);
                        //check to see if spin state is needed
                        if (Vector2.Distance(transform.position, thrower.transform.position) <=8)//is boomerang within 8 units of thrower
                            if (orient==1 && transform.position.x>thrower.transform.position.x+16|| orient == -1 && transform.position.x < thrower.transform.position.x - 16)//if 16 units past thrower
                            {
                                //set orient relative to current dir
                                if (direction > 90 && direction < 270)
                                    orient = -1;
                                else
                                    orient = 1;

                                turnAmount = 0;//reset rot timer
                                //set initial dir for spin based on orient
                                if (orient==1)
                                    direction=0;
                                else
                                    direction=180;

                                state = 0;
                            }
                        if (Vector2.Distance(transform.position, thrower.transform.position) <= returnDistance){ TryGrabKinzecter(); }
                        break;

                }
            }
            else
            {
                if (boomerangTime>0)
                {
                    boomerangTime -= 4*Time.deltaTime;
                    orient = Mathf.Sign(velocity.x);
                }
                else
                {
                    boomerangFlag = true;
                    //set initial dir for spin based on orient
                    if (orient == 1)
                        direction = 0;
                    else
                        direction = 180;
                }
            }
            
        }
    }
    public void ThrowKinzecter(CharacterObject player)
    {
        thrower = player;
        hasBeenThrown = true;
        //this.transform.position = player.transform.position + throwDir * returnDistance;
        //kzRB.isKinematic = false;
        //kzRB.AddForce(throwDir * kzSpeed, ForceMode2D.Impulse);
        //kState = ThrowingState.Thrown;
    }
    private void TryGrabKinzecter()
    {
        if (Vector3.Distance(transform.position, thrower.transform.position) <= returnDistance)
        {
            kState = ThrowingState.WithPlayer;
            velocity = Vector2.zero;
            //kzRB.isKinematic = true;
            hasBeenThrown = false;
            thrower.isKinzecterOut = false;
            Destroy(gameObject,.2f);
        }
    }
    private void OLDUpdate()
    {
        if (kzRB.velocity.x >= 0)
        {
            transform.localScale = new Vector2(startScale, transform.localScale.y);
        }
        else
        {
            transform.localScale = new Vector2(-startScale, transform.localScale.y);
        }
        flightSpeed = kzRB.velocity.magnitude;

        if (flightSpeed < minLethalSpeed)
            isTooSlow = true;
        else
            isTooSlow = false;

        switch (kState)
        {
            case ThrowingState.WithPlayer:
                CollectEssence();
                break;
            case ThrowingState.Thrown:
                if (isTooSlow)
                    TryGrabKinzecter();

                if (!coroutineStarted)
                {
                    StartCoroutine(ReturnToPlayer());
                }
                break;
            case ThrowingState.Recalling:
                TryGrabKinzecter();
                break;
            default:
                break;
        }
        CheckParticles();
    }

    private void OLDFixedUpdateStateMachine()
    {
        switch (kState)
        {
            case ThrowingState.WithPlayer:

                break;
            case ThrowingState.Thrown:

                break;
            case ThrowingState.Recalling:
                Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
                kzRB.velocity = dirToPlayer * kzRecallSpeed;


                break;
        }
    }

    private void OLDLateUpdateStateMachine()
    {
        switch (kState)
        {
            case ThrowingState.WithPlayer:
                kinzecterParticles.Stop();
                hpPS.Stop();
                ammoPS.Stop();
                energyPS.Stop();
                shouldFly = true;
                kzColl.enabled = false;
                kzSprite.enabled = false;
                transform.position = player.transform.position;
                break;
            case ThrowingState.Thrown:
                kinzecterParticles.Play();

                if (isTooSlow)
                    kzColl.enabled = false;
                else
                    kzColl.enabled = true;

                kzSprite.enabled = true;
                break;
            case ThrowingState.Recalling:
                kinzecterParticles.Play();
                kzColl.enabled = false;
                kzSprite.enabled = true;
                break;
            default:
                break;
        }
    }

    
    public void Recall()
    {
        kState = ThrowingState.Recalling;
    }
    private void CollectEssence()
    {
        if (ammoStock>0)
        {
            player.AddAmmo(ammoStock);
            ammoStock = 0;
        }
        if (eStock>0)
        {
            player.AddMeter(eStock);
            eStock = 0;
        }
        if (hpStock>0)
        {
            player.AddRecovery(hpStock);
            hpStock = 0;
        }
    }
    private void CheckParticles()
    {
        if (ammoStock>0)
            ammoPS.Play();
        else
            ammoPS.Stop();
        if (eStock>0)
            energyPS.Play();
        else
            energyPS.Stop();
        if (hpStock > 0)
            hpPS.Play();
        else
            hpPS.Stop();
    }
    public bool isWithPlayer()
    {
        return kState == ThrowingState.WithPlayer;
    }
    //private void OnTriggerEnter2D(Collider2D enemyColl)
    //{
    //    boss = enemyColl.GetComponentInParent<BossPatrolManager>();
    //    enemy = enemyColl.gameObject.GetComponentInParent<EnemyHealthManager>();
    //    if (enemy !=null)
    //    {
    //        if (flightSpeed>minLethalSpeed)
    //        {
    //            if (!enemy.IsInvul)
    //            {
    //                if (!essenceAdded)
    //                {
    //                    StartCoroutine(AddEssecnce());
    //                }
    //                if (shouldScreenshakeOnHit)
    //                    Screenshake();
    //            }
    //            if (!isTooSlow)
    //            {
    //                EnemyHealthManager nextClosestEnemy = EnemyHealthManager.GetClosestEnemy(transform.position, targetNextEnemyDistance);
    //                if (nextClosestEnemy != null)
    //                {
    //                    nextTargetDir = (nextClosestEnemy.transform.position - transform.position).normalized;
    //                    if (!newEnemyTargeted)
    //                        StartCoroutine(TargetNextEnemy());
    //                }
    //            }
    //        }
    //    }
    //}
    IEnumerator AddEssecnce()
    {
        essenceAdded = true;
        ammoStock += enemy.ammoStock;
        eStock += enemy.eStock;
        hpStock += enemy.hpStock;
        yield return new WaitForSeconds(.1f);
        essenceAdded = false;
    }
    IEnumerator TargetNextEnemy()
    {
        essenceAdded = true;
        kzRB.velocity = nextTargetDir * flightSpeed;
        yield return new WaitForSeconds(.5f);
        essenceAdded = false;
    }
    IEnumerator ReturnToPlayer()
    {
        coroutineStarted = true;
        while (kState==ThrowingState.Thrown)
        {
            yield return new WaitForSeconds(10f);
            kState = ThrowingState.Recalling;
        }
        coroutineStarted = false;
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
}
