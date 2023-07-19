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
    [SerializeField] private GameObject destroyEffect;
    AudioManager audioManager;
    Rigidbody2D rb;
    BoxCollider2D boxCollider;
    bool isFalling;
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
        RemoveHealth(curAtk.damage);
        StartCoroutine(FlashWhiteDamage(curAtk.hitStop));
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
        defaultMat=spriteRend.material;
        audioManager = AudioManager.instance;
        currentHealth=maxHealth;
        transform.SetParent(null);
    }
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float collisionHeight = 0.1f;
    [SerializeField] private int projectileIndex = 61;
    // Update is called once per frame
    void Update()
    {
        isFalling = rb.velocity.y < -1;
        if (isFalling)
        {
            RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider.bounds.center,boxCollider.bounds.size/2,0f, Vector2.down, boxCollider.bounds.extents.y + collisionHeight, playerLayer);
            Debug.DrawRay(boxCollider.bounds.center, Vector2.down * (boxCollider.bounds.extents.y + collisionHeight), Color.red);
            if (rayCastHit)
            {
                IHittable victim = rayCastHit.transform.GetComponent<IHittable>();
                victim.Hit(GameEngine.gameEngine.mainCharacter, projectileIndex, 0);
                RemoveHealth(currentHealth);
            }
        }
    }
    [SerializeField] private bool isExplosion = false;
    [SerializeField] private float lifeTime = 0.1f;
    void RemoveHealth(int damage)
    {
        currentHealth -= damage;
        audioManager.PlaySound(hitSound);

        if (currentHealth <= 0)
            OnDestroy();

    }

    private void OnDestroy()
    {
        GameObject destroyedEffect = Instantiate(destroyEffect, transform.position, transform.rotation);
        if (isExplosion)
        {
            BombController bomb = destroyedEffect.GetComponent<BombController>();
            bomb.character = GameEngine.gameEngine.mainCharacter;
            bomb.StartState();
        }
        Destroy(gameObject, lifeTime);
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
