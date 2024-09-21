using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IHittable, ISpawnable
{
    [Header("Movement")]
    //public Vector2 velocity;

    //public float gravity = -0.01f, gravityMin = -17f;
    public float aniMoveSpeed;

    //public Vector3 friction = new Vector3(0.95f, 0.99f, 0.95f);
    //[SerializeField] private float direction = 1;
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
    public bool isEnemySpawner = false;
    private MinionSpawner minion;
    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRend = characterAnim.gameObject.GetComponent<SpriteRenderer>();
        audioManager = AudioManager.instance;
        
        currentHealth = maxHealth;
        defaultMat = spriteRend.material;
        spawnPos = transform.position;
        if (isEnemySpawner)
        {
            minion = GetComponentInChildren<MinionSpawner>();
            DeSpawn();
        }
        controller = GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
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
    public string hitAudio = "Props/Box Break";
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
            //Debug.Log(nextKnockback.x);
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
        CalculateVelocity();
        HandleWallSliding();
        controller.Move(velocity * Time.fixedDeltaTime, Vector2.zero);
        wallFlag = wallSliding;
        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.fixedDeltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
        if (IsGrounded())
        {
            aerialFlag = false;
            //wallFlag = false;
            aerialTimer = 0;
            animAerialState = 0f;
            //velocity.y = 0;
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
                hasLanded = false;
                if (animAerialState <= 1f)
                {
                    animAerialState += 0.1f;
                }
            }

        }
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
        animFallSpeed = velocity.y;
    }

    public void SetVelocity(Vector3 v)
    {
        velocity = v;
    }
    public bool IsGrounded()
    {
        return controller.collisions.below;
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
        //audioManager.PlaySound("Enemy/Enemy Explode");
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
        if (type==2)
        {
            transform.position = spawnPos;
        }
    }
    public bool gravityFix = true;
    public void DeSpawn()
    {
        HideCharacter();
        SetMaxHealth();
        IsDead = true;
        
            IsSpawned = false;

        transform.position = spawnPos;
    }
    [Header("Jump")]
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    float moveSpeed = 6;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    public float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    public Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        if (wallSliding)
        {
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
                { // not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }


    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }

        }

    }
    [Header("Floating Balloon")]
    public float floatHeight = 3;
    [SerializeField] private float floatTime = 0.5f;
    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        if (gravityFix)
        {
            velocity.y += gravity * Time.fixedDeltaTime;
        }
        else
        {
            Vector3 newPos = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100, controller.collisionMask);
            if (hit)
            {
                newPos.y = Mathf.Lerp(newPos.y, (hit.point + Vector2.up * floatHeight).y, floatTime);
            }
            transform.position = newPos;
        }
    }
    private void OnDrawGizmos()
    {
        Vector3 yPos = new Vector3(transform.position.x, transform.position.y - floatHeight, 0);
        float size = .3f;
        if (!gravityFix)//floating enemy
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(yPos - Vector3.left * size, yPos - Vector3.right * size);
            Gizmos.DrawLine(yPos - Vector3.up * size, yPos - Vector3.down * size);
        }

    }
    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
