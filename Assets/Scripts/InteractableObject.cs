using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IHittable, ISpawnable
{
    [Header("Movement")]
    public Vector2 velocity;

    public float gravity = -0.01f, gravityMin = -17f;
    public float aniMoveSpeed;

    public Vector3 friction = new Vector3(0.95f, 0.99f, 0.95f);
    [SerializeField] private float direction = 1;
    [Header("CharacterModel")]
    public GameObject character;
    public GameObject draw;
    public Animator characterAnim;
    [Header("Health")]
    public SpriteRenderer spriteRend;
    public Material defaultMat, whiteMat;
    private Color flashColor = new Color(0, 0.5f, 0.75f, 1f);
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

    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        controller = GetComponent<Controller2D>();
        spriteRend = characterAnim.gameObject.GetComponent<SpriteRenderer>();
        audioManager = AudioManager.instance;

        currentHealth = maxHealth;
        defaultMat = spriteRend.material;
        spawnPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!PauseManager.IsGamePaused && !DialogueManager.instance.isDialogueActive)
        {
            if (GameEngine.hitStop <= 0)
            {
                UpdatePhysics();
            }
        }
        UpdateAnimator();
    }
    public string hitAudio = "TakeDamage";
    public float movementSpeed=15f;
    void GetHit(CharacterObject attacker, AttackEvent curAtk)
    {
        Vector3 nextKnockback = curAtk.knockback;
        Vector3 knockOrientation = transform.position - attacker.transform.position;
        knockOrientation.Normalize();
        nextKnockback.x *= knockOrientation.x;
        nextKnockback.y = 0;
        if (isDestructable)
        {
            RemoveHealth(curAtk.damage);
            StartCoroutine(FlashWhiteDamage(curAtk.hitStop));
            GameEngine.SetHitPause(curAtk.hitStop);
        }
        else
        {
            nextKnockback.x = movementSpeed * knockOrientation.x;
            Debug.Log(nextKnockback.x);
        }
        SetVelocity(nextKnockback);
        targetHitAnim.x = curAtk.hitAnim.x;
        targetHitAnim.y = curAtk.hitAnim.y;
        curHitAnim = targetHitAnim * .25f;

        PlayAudio(hitAudio);
        attacker.hitConfirm += 1;
        attacker.BuildMeter(curAtk.meterGain);

        
    }
    public int maxHealth, currentHealth;
    void RemoveHealth(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (!deathCoroutineStarted)
                StartCoroutine(DeathEvent(shouldSpawnHealth));
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
    [HideInInspector] public AudioManager audioManager;
    private void PlayAudio(string audioName)
    {
        audioManager.PlaySound(audioName);
    }
    [Header("Grounded Check")]
    [SerializeField]
    private LayerMask whatCountsAsGround;
    [HideInInspector] public bool aerialFlag, wallFlag, isOnGround, isOnWall;
    public float aerialTimer, groundDetectHeight, wallDetectWidth, animAerialState, animFallSpeed, coyoteTimer = 3f;
    void UpdatePhysics()
    {
        if (IsGrounded())
        {
            aerialFlag = false;
            aerialTimer = 0;
            animAerialState = 0f;
            GroundTouch();
        }
        else
        {
            if (!aerialFlag)
            {
                aerialTimer++;
            }
            if (aerialTimer >= coyoteTimer)//coyote time
            {
                aerialFlag = true;
                if (animAerialState <= 1f)
                {
                    animAerialState += 0.1f;
                }

                if (controller.collisions.above || controller.collisions.below)
                    velocity.y = 0;
                else
                {
                    velocity.y += gravity;
                    Mathf.Clamp(velocity.y, gravityMin, 0);
                    hasLanded = false;
                }
                wallFlag = false;
            }
        }
        Move(velocity);
        velocity.Scale(friction);
    }
    [Header("Hit Stun")]
    public Vector2 curHitAnim;
    public Vector2 targetHitAnim;
    [HideInInspector] public float animSpeed;

    void UpdateAnimator()
    {
        animSpeed = 1;
        if (GameEngine.hitStop > 0)
        {
            animSpeed = 0;
        }

        Vector2 latSpeed = new Vector2(velocity.x, 0);
        aniMoveSpeed = Vector3.SqrMagnitude(latSpeed);
        animFallSpeed = velocity.y /** 30f*/;
        characterAnim.SetFloat("moveSpeed", aniMoveSpeed);
        characterAnim.SetFloat("aerialState", animAerialState);
        //characterAnim.SetBool("wallState", wallFlag);
        characterAnim.SetFloat("fallSpeed", animFallSpeed);
        characterAnim.SetFloat("hitAnimX", curHitAnim.x);
        characterAnim.SetFloat("hitAnimY", curHitAnim.y);
        characterAnim.SetFloat("animSpeed", animSpeed);

    }

    public void SetVelocity(Vector3 v)
    {
        velocity = v;
    }
    public bool IsGrounded()
    {
        RaycastHit2D rayCastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, groundDetectHeight, whatCountsAsGround);
        Color rayColor;
        if (rayCastHit.collider != null)
            rayColor = Color.green;
        else
            rayColor = Color.red;
        Debug.DrawRay(boxCollider2D.bounds.center + new Vector3(boxCollider2D.bounds.extents.x, 0), Vector2.down * (boxCollider2D.bounds.extents.y + groundDetectHeight), rayColor);
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, 0), Vector2.down * (boxCollider2D.bounds.extents.y + groundDetectHeight), rayColor);
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, boxCollider2D.bounds.extents.y + groundDetectHeight), Vector2.right * (boxCollider2D.bounds.extents.x), rayColor);

        return rayCastHit.collider != null;

        //return controller.collisions.below;
    }
    public bool IsOnWall()
    {

        return controller.collisions.left || controller.collisions.right;
    }
    private bool hasLanded;
    public ParticleSystem landingParticle;
    private void GroundTouch()
    {
        if (!hasLanded)
        {
            animFallSpeed = 0f;
            hasLanded = true;
            landingParticle.Play();
        }
    }
    [HideInInspector] public Controller2D controller;
    [HideInInspector] public BoxCollider2D boxCollider2D;
    public void Move(Vector2 velocity)
    {
        //myRB.velocity = velocity;
        controller.Move(velocity * Time.fixedDeltaTime, Vector2.zero);
    }
    public int healthDropRate = 2, effectIndex, numOfPickups = 1;
    public bool deathCoroutineStarted = false, shouldSpawnHealth = false, isDestructable = true;
    public GameObject[] pickup;
    public GameObject deathEffect;
    /// <summary>
    /// waits until the death animation is done and then destroys the character
    /// </summary>
    /// <returns></returns>
    IEnumerator DeathEvent(bool spawnPickup)
    {
        deathCoroutineStarted = true;
        Instantiate(deathEffect, transform.position, transform.rotation);
        if (spawnPickup)
        {
            for (int i = 0; i < numOfPickups; i++)
            {
                SpawnPickup(pickup[0]);
            }
            int randNum = UnityEngine.Random.Range(0, 10);
            if (randNum <= healthDropRate)
            {
                SpawnPickup(pickup[1]);
            }
        }
        audioManager.PlaySound("Death");
        yield return new WaitForFixedUpdate();//get length of death animation        
        DeSpawn();
    }
    private void SpawnPickup(GameObject pickup)
    {
        int randNumX = UnityEngine.Random.Range(-20, 20);
        int randNumY = UnityEngine.Random.Range(15, 35);
        Vector2 offsetDir = new Vector2(randNumX, randNumY);
        GameObject effect = Instantiate(pickup, transform.position, transform.rotation);
        effect.GetComponentInChildren<Rigidbody2D>().AddForce(offsetDir, ForceMode2D.Impulse);
    }
    public bool IsDead=false, IsSpawned=false;
    void SetMaxHealth()
    {
        currentHealth = maxHealth;
    }
    Vector3 spawnPos;
    public LayerMask defaultLayer, hiddenLayer;
    private void HideCharacter()
    {
        character.SetActive(false);
        gameObject.layer = (int)Mathf.Log(hiddenLayer.value, 2);
    }
    private void ShowCharacter()
    {
        character.SetActive(true);
        gameObject.layer = (int)Mathf.Log(defaultLayer.value, 2);
    }
    public void Spawn(int type)
    {
        ShowCharacter();

        SetMaxHealth();
        deathCoroutineStarted = false;
        IsDead = false;
        IsSpawned = true;

        transform.SetParent(null);
        transform.position = spawnPos;
    }
    public void DeSpawn()
    {
        HideCharacter();
        SetMaxHealth();
        IsDead = true;
        IsSpawned = false;
        transform.position = spawnPos;
    }
}
