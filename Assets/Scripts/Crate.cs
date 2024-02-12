using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Crate : MonoBehaviour, IHittable
{
    public int maxHealth = 1, currentHealth;
    [Header("Effects")]
    public SpriteRenderer spriteRend;
    public Material defaultMat, whiteMat;
    [SerializeField] private string hitSound = "Props/Box Break";
    [SerializeField] private GameObject destroyEffect,droppedItem,currencyPickup;
    AudioManager audioManager;
    Rigidbody2D rb;
    BoxCollider2D boxCollider;
    bool isFalling;
    public bool IsSpawned;
    Vector3 spawnPos;
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Hit(CharacterObject attacker, int projectileIndex, int atkIndex)
    {
        AttackEvent curAtk;
        if (projectileIndex == 0)//not a projectile
        {
            curAtk = GameEngine.coreData.characterStates[attacker.currentState].attacks[attacker.currentAttackIndex];
        }
        else//projectiles
        {
            curAtk = GameEngine.coreData.characterStates[projectileIndex].attacks[atkIndex];
        }
        GetHit(attacker, curAtk);
    }

    private void GetHit(CharacterObject attacker, AttackEvent curAtk)
    {
        RemoveHealth(curAtk);
       
        GameEngine.SetHitPause(curAtk.hitStop);
        attacker.hitConfirm += 1;
        attacker.BuildMeter(curAtk.meterGain);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRend = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        audioManager = AudioManager.instance;
        defaultMat = spriteRend.material;
        spawnPos = transform.position;
        OnSpawn();
    }

    public void OnSpawn()
    {
        gameObject.SetActive(true);
        currentHealth = maxHealth;
        transform.SetParent(null);
        transform.position = spawnPos;
        IsSpawned = true;
        isFalling = false;
    }

    [SerializeField] private LayerMask playerLayer,groundLayer;
    [SerializeField] private float collisionHeight = 0.1f;
    [SerializeField] private int projectileIndex = 61;
    [SerializeField] private float fallSpeedCheck = -0.2f;
    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.y < fallSpeedCheck)
        {
            isFalling = true;
        }

        if (isFalling)
        {
            RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size / 2, 0f, Vector2.down, boxCollider.bounds.extents.y + collisionHeight, playerLayer);
            Debug.DrawRay(boxCollider.bounds.center, Vector2.down * (boxCollider.bounds.extents.y + collisionHeight), Color.red);
            if (rayCastHit)
            {
                IHittable victim = rayCastHit.transform.GetComponent<IHittable>();
                victim.Hit(GameEngine.gameEngine.mainCharacter, projectileIndex, 0);
                DestroyCrate();
            }
            //RaycastHit2D groundCastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size / 2, 0f, Vector2.down, boxCollider.bounds.extents.y + collisionHeight, groundLayer);//maybe hcange to linecast
            RaycastHit2D groundCastHit = Physics2D.Raycast(boxCollider.bounds.center, Vector2.down, boxCollider.bounds.extents.y + (collisionHeight * 2), groundLayer);//maybe hcange to linecast
            if (groundCastHit)
            {
                DestroyCrate();
            }
        }
    }
    [SerializeField] private bool isExplosion = false;
    //[SerializeField] private float lifeTime = 0.1f;
    void RemoveHealth(AttackEvent curAtk)
    {
        currentHealth -= curAtk.damage;
        audioManager.PlaySound(hitSound);

        if (currentHealth <= 0)
            DestroyCrate();
        else
            StartCoroutine(FlashWhiteDamage(curAtk.hitStop));

    }

    private void DestroyCrate()
    {
        GameObject destroyedEffect = Instantiate(destroyEffect, transform.position, transform.rotation);
        if (isExplosion)
        {
            BombController bomb = destroyedEffect.GetComponent<BombController>();
            bomb.character = GameEngine.gameEngine.mainCharacter;
            bomb.StartState();
        }
        DropItem();
        IsSpawned = false;
        spriteRend.material = defaultMat;
        gameObject.SetActive(false);
    }
    [SerializeField] int dropRate = 2,numOfPickups;
    private void DropItem()
    {
        if (dropRate>0)
        {
            for (int i = 0; i < numOfPickups; i++)
            {
                SpawnPickup(currencyPickup);
            }
            int randNum = UnityEngine.Random.Range(0, 100);
            if (randNum <= dropRate)
            {
                SpawnPickup(droppedItem);
            }
        }
    }
    private void SpawnPickup(GameObject pickup)
    {
        if (pickup != null)
        {
            int randNumX = UnityEngine.Random.Range(-20, 20);
            int randNumY = UnityEngine.Random.Range(15, 35);
            Vector2 offsetDir = new Vector2(randNumX, randNumY);
            GameObject effect = Instantiate(pickup, transform.position, transform.rotation);
            effect.GetComponentInChildren<Rigidbody2D>().AddForce(offsetDir, ForceMode2D.Impulse);
            effect.transform.SetParent(null);
        }
    }

    private IEnumerator FlashWhiteDamage(float hitFlash)
    {
        spriteRend.material = defaultMat;
        spriteRend.material = whiteMat;
        for (int i = 0; i < hitFlash; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        spriteRend.material = defaultMat;
    }
}
