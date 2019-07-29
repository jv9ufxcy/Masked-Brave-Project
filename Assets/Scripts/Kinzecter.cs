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
    private BossHealthManager boss;
    private EnemyHealthManager enemy;

[SerializeField] private State kState;
    private bool shouldFly, coroutineStarted=false;
    private float startScale, flightSpeed;
    [SerializeField] private int hpStock, eStock, ammoStock;

    public DamageEffect _effect;

    public enum DamageEffect { stun, knockback, launch }

    private enum State 
    {
        WithPlayer, Thrown, Recalling,
    }
    
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

        kState = State.Recalling;
    }
    private void Start()
    {
        startScale = transform.localScale.x;
    }
    private void Update()
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
        if (flightSpeed<minLethalSpeed)
        {
            kzColl.enabled = false;
        }
        CheckParticles();
    }
    private void FixedUpdate()
    {
        switch (kState)
        {
            case State.WithPlayer:
                CollectEssence();
                break;
            case State.Thrown:
                TryGrabKinzecter();
                break;
            case State.Recalling:
                Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
                kzRB.velocity = dirToPlayer * kzRecallSpeed;

                TryGrabKinzecter();
                break;
        }
    }
    private void LateUpdate()
    {
        switch (kState)
        {
            case State.WithPlayer:
                kinzecterParticles.Stop();
                hpPS.Stop();
                ammoPS.Stop();
                energyPS.Stop();
                shouldFly = true;
                kzColl.enabled = false;
                kzSprite.enabled = false;
                transform.position = player.transform.position;
                break;
            case State.Thrown:
                kinzecterParticles.Play();
                kzColl.enabled = true;
                kzSprite.enabled = true;
                break;
            case State.Recalling:
                kinzecterParticles.Play();
                kzColl.enabled = false;
                kzSprite.enabled = true;
                break;
            default:
                break;
        }
    }
    public void ThrowKinzecter(Vector3 throwDir)
    {
        this.transform.position = player.transform.position + throwDir * returnDistance;
        kzRB.isKinematic = false;
        kzRB.AddForce(throwDir * kzSpeed, ForceMode2D.Impulse);
        kState = State.Thrown;
    }
    private void TryGrabKinzecter()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < returnDistance)
        {
            kState = State.WithPlayer;
            kzRB.velocity = Vector2.zero;
            kzRB.isKinematic = true;
        }
    }
    public void Recall()
    {
        kState = State.Recalling;
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
        if (eStock>0)
            energyPS.Play();
        if (hpStock > 0)
            hpPS.Play();
    }
    public bool isWithPlayer()
    {
        return kState == State.WithPlayer;
    }
    private void OnTriggerEnter2D(Collider2D enemyColl)
    {
        boss = enemyColl.GetComponentInParent<BossHealthManager>();
        enemy = enemyColl.gameObject.GetComponentInParent<EnemyHealthManager>();
        if (enemy !=null)
        {
            if (flightSpeed>minLethalSpeed)
            {
                if (!enemy.IsInvul)
                {
                    if (!coroutineStarted)
                    {
                        StartCoroutine(AddEssecnce());
                    }
                }
                //EnemyHealthManager nextClosestEnemy = EnemyHealthManager.GetClosestEnemy(transform.position, targetNextEnemyDistance);
                //if (nextClosestEnemy != null)
                //{
                //    Vector3 throwDir = (nextClosestEnemy.transform.position - transform.position).normalized;
                //    kzRB.velocity = throwDir * flightSpeed;
                //}
                if (shouldScreenshakeOnHit)
                    Screenshake();
            }
        }
        if (boss != null)
        {
            if (flightSpeed > minLethalSpeed)
            {
                StartCoroutine(AddBossEssecnce());
            }
        }
    }
    IEnumerator AddEssecnce()
    {
        ammoStock += enemy.ammoStock;
        eStock += enemy.eStock;
        hpStock += enemy.hpStock;
        coroutineStarted = true;
        yield return new WaitForSeconds(.1f);
        coroutineStarted = false;
    }
    IEnumerator AddBossEssecnce()
    {
        ammoStock += boss.ammoStock;
        eStock += boss.eStock;
        hpStock += boss.hpStock;
        coroutineStarted = true;
        yield return new WaitForSeconds(.1f);
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
}
