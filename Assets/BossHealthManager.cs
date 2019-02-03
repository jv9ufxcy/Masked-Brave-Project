using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealthManager : MonoBehaviour
{
    [SerializeField] private Player thePlayer;

    [SerializeField] private bool isEnemyInvincible = false;
    [SerializeField] private int currentEnemyHealth;
    [SerializeField] private int maxEnemyHealth;

    [SerializeField] private GameObject deathParticle;

    [SerializeField] private GameObject itemDropped;

    [SerializeField] private GameObject energyDropped;
    [SerializeField] private GameObject bossHitbox;

    [SerializeField] private bool shouldDropAtHalf;

    //knockback
    public float enemyKnockbackDuration, enemyForce, enemyMaxKnockbackDuration;
    public bool enemyKnockFromRight;
    private bool isInvul;
    [SerializeField] private float damageCooldownInSeconds = .75f;

    //jump
    [SerializeField] private float jumpStrength;
    private Vector2 jumpForce;
    private float defaultGravityScale;

    //patrol variables
    [SerializeField] private float detectionRange=40;
    [SerializeField] Transform wallDetectPoint;
    [SerializeField] Transform groundDetectPoint;
    [SerializeField] Transform edgeDetectPoint;
    [SerializeField] float DetectRadius = 0.2f;
    [SerializeField] LayerMask whatCountsAsWall;
    private bool notAtEdge;
    private bool hittingWall;
    private bool isOnGround;
    [SerializeField] private bool playerIsToTheRight;
    [SerializeField] private bool isFacingRight;

    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;
    private Animator enemyAnim;
    [SerializeField] private BossDoorTrigger bossDoorTrigger;
    //audio
    private AudioManager audioManager;
    [SerializeField] private string enemyTakeDamageSound;
    [SerializeField] private string enemyDeathSound;
    private int deathCount=1;
    #region Properties
    public int CurrentHitPoints
    {
        get
        {
            return currentEnemyHealth;
        }
        private set
        {
            currentEnemyHealth = value;
            if (currentEnemyHealth < 0)
                currentEnemyHealth = 0;
            if (currentEnemyHealth > maxEnemyHealth)
                currentEnemyHealth = maxEnemyHealth;
        }
    }

    public bool IsInvul
    {
        get
        {
            return isInvul;
        }

        set
        {
            isInvul = value;
        }
    }
    public bool IsNotAtEdge
    {
        get
        {
            return notAtEdge;
        }

        set
        {
            notAtEdge = value;
        }
    }
    public bool IsHittingWall
    {
        get
        {
            return hittingWall;
        }

        set
        {
            hittingWall = value;
        }
    }

    public bool IsOnGround
    {
        get
        {
            return isOnGround;
        }

        set
        {
            isOnGround = value;
        }
    }

    public bool PlayerIsToTheRight
    {
        get
        {
            return playerIsToTheRight;
        }

        set
        {
            playerIsToTheRight = value;
        }
    }

    public bool IsFacingRight
    {
        get
        {
            return isFacingRight;
        }

        set
        {
            isFacingRight = value;
        }
    }
    #endregion
    // Use this for initialization
    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        enemyRend = GetComponent<SpriteRenderer>();
        enemyAnim = GetComponent<Animator>();
        enemyRB = GetComponent<Rigidbody2D>();
        bossDoorTrigger = GetComponent<BossDoorTrigger>();
        currentEnemyHealth = maxEnemyHealth;
        jumpForce = new Vector2(0, jumpStrength);
        defaultGravityScale = enemyRB.gravityScale;

        thePlayer = FindObjectOfType<Player>();

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEnemyHealth <= maxEnemyHealth / 2 && shouldDropAtHalf == true)
        {
            Instantiate(energyDropped, transform.position, transform.rotation);
            shouldDropAtHalf = false;
        }
        if (currentEnemyHealth <= 0)
        {
            currentEnemyHealth = 0;
            OnDeath();
        }
        PassPatrolVariables();
        WhereIsPlayer();
    }

    private void PassPatrolVariables()
    {
        IsNotAtEdge = Physics2D.OverlapCircle(edgeDetectPoint.position, DetectRadius, whatCountsAsWall);
        IsHittingWall = Physics2D.OverlapCircle(wallDetectPoint.position, DetectRadius, whatCountsAsWall);


        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, DetectRadius, whatCountsAsWall);
        IsOnGround = groundObjects.Length > 0;
        enemyAnim.SetBool("Ground", IsOnGround);
        if (isOnGround)
        {
            enemyAnim.SetBool("Ground", true);
        }
        if (enemyRB.velocity.y < 0)
        {
            enemyAnim.SetBool("Ground", false);
        }
    }
    public void Jump()
    {
        if (isOnGround)
        {
            enemyAnim.SetBool("Ground", false);
            enemyAnim.SetFloat("vSpeed", enemyRB.velocity.y);
            enemyRB.AddForce(jumpForce, ForceMode2D.Impulse);
            isOnGround = false;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
    public void FlipFacingRight()
    {
        IsFacingRight = true;
        transform.localScale = new Vector3(-1f, 1f, 1f);
    }
    public void FlipFacingLeft()
    {
        IsFacingRight = false;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }
    private void WhereIsPlayer()
    {
        //if player is to the right, charge at the right side
        if (thePlayer.transform.position.x > transform.position.x)
            PlayerIsToTheRight = true;
        else if (thePlayer.transform.position.x < transform.position.x)
            PlayerIsToTheRight = false;
    }
    public void Charge()
    {
        //ChangeDirection();

        if (!IsFacingRight)
            enemyRB.velocity = new Vector2(-enemyForce, 0);
        else if (isFacingRight)
            enemyRB.velocity = new Vector2(enemyForce, 0);
    }

    public void ChangeDirection()
    {
        if (PlayerIsToTheRight && !IsFacingRight)
        {
            FlipFacingRight();
        }
        else if (!PlayerIsToTheRight && IsFacingRight)
        {
            FlipFacingLeft();
        }
    }

    public void Knockback()
    {
        if (enemyKnockFromRight)
        {
            enemyRB.velocity = new Vector2(-enemyForce, enemyForce);
        }
        if (!enemyKnockFromRight)
        {
            enemyRB.velocity = new Vector2(enemyForce, enemyForce);
        }
        enemyKnockbackDuration -= Time.deltaTime;
    }
    public void TakeDamage(int damageToTake)
    {
        if (!IsInvul&&deathCount>0)
        {
            if (!isEnemyInvincible)
                currentEnemyHealth -= damageToTake;
            audioManager.PlaySound(enemyTakeDamageSound);
            StartCoroutine(DamageCooldownCoroutine());
        }
    }
    private IEnumerator DamageCooldownCoroutine()
    {
        IsInvul = true;
        StartCoroutine(BlinkWhileInvulnerableCoroutine());
        yield return new WaitForSeconds(damageCooldownInSeconds);
        IsInvul = false;
    }

    private IEnumerator BlinkWhileInvulnerableCoroutine()
    {
        Color defaultColor = enemyRend.color;
        Color invulnerableColor = Color.red;

        float blinkInterval = .10f;

        while (IsInvul)
        {
            enemyRend.color = invulnerableColor;
            yield return new WaitForSeconds(blinkInterval);

            enemyRend.color = defaultColor;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
    private void OnDeath()
    {
        if (deathCount>0)
        {
            deathCount--;
            audioManager.PlaySound(enemyDeathSound);
            Instantiate(deathParticle, transform.position, transform.rotation);
            Instantiate(itemDropped, transform.position, transform.rotation);
            enemyAnim.SetTrigger("Death");
            isEnemyInvincible = true;
            
            //bossDoorTrigger.DeSpawnBoss();
        }
        bossHitbox.gameObject.SetActive(false);
    }
}
