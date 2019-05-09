using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthManager : MonoBehaviour
{
    [SerializeField] private bool isEnemyInvincible = false;
    [SerializeField] private int currentEnemyHealth;
    [SerializeField] private int maxEnemyHealth;

    public DamageState _damagedState;
    public enum DamageState { stunned, knockedback, launched }

    [SerializeField] private GameObject deathParticle;

    [SerializeField] private GameObject itemDropped;

    [SerializeField] private GameObject energyDropped;
    [SerializeField] private bool shouldDropAtHalf;

    //knockback
    public float enemyKnockbackDuration, enemyKnockbackForce, enemyMaxKnockbackDuration,  maxEnemyStunDuration, maxEnemyLaunchDuration;
    [SerializeField] private Vector2 launchForce =new Vector2 (0,15);
    public bool enemyKnockFromRight;
    private bool isInvul;
    [SerializeField] private float damageCooldownInSeconds = .75f;

    //patrol variables
    [SerializeField] Transform wallDetectPoint;
    [SerializeField] Transform groundDetectPoint;
    [SerializeField] Transform edgeDetectPoint;
    [SerializeField] float DetectRadius = 0.2f;
    [SerializeField] LayerMask whatCountsAsWall;
    private bool notAtEdge;
    private bool hittingWall;
    private bool isOnGround;

    private bool isFacingRight;
    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;
    private Animator enemyAnim;
    //audio
    private AudioManager audioManager;
    [SerializeField] private string enemyTakeDamageSound;
    [SerializeField] private string enemyDeathSound;

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

    // Use this for initialization
    void Start ()
    {
        Initialize();
    }

    private void Initialize()
    {
        currentEnemyHealth = maxEnemyHealth;

        enemyRend = GetComponent<SpriteRenderer>();
        enemyAnim = GetComponent<Animator>();
        enemyRB = GetComponent<Rigidbody2D>();

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void Update ()
    {
        //if (currentEnemyHealth<=maxEnemyHealth/2 && shouldDropAtHalf==true)
        //{
        //    Instantiate(energyDropped, transform.position, transform.rotation);
        //    shouldDropAtHalf = false;
        //}
		if (currentEnemyHealth<=0)
        {
            currentEnemyHealth = 0;
            OnDeath();
        }
        PassPatrolVariables();
    }
    public void FlipFacingRight()
    {
        transform.localScale = new Vector3(-1f, 1f, 1f);
    }
    public void FlipFacingLeft()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void PassPatrolVariables()
    {
        IsNotAtEdge = Physics2D.OverlapCircle(edgeDetectPoint.position, DetectRadius, whatCountsAsWall);
        IsHittingWall = Physics2D.OverlapCircle(wallDetectPoint.position, DetectRadius, whatCountsAsWall);
        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, DetectRadius, whatCountsAsWall);
        IsOnGround = groundObjects.Length > 0;
    }

    public void Knockback()
    {
        switch (_damagedState)
        {
            case DamageState.stunned:
                enemyRB.velocity = new Vector2(0, 0);
                enemyKnockbackDuration -= Time.deltaTime;
                break;
            case DamageState.knockedback:
                if (enemyKnockFromRight)
                {
                    enemyRB.velocity = new Vector2(-enemyKnockbackForce, 0);
                }
                if (!enemyKnockFromRight)
                {
                    enemyRB.velocity = new Vector2(enemyKnockbackForce, 0);
                }
                enemyKnockbackDuration -= Time.deltaTime;
                break;
            case DamageState.launched:
                enemyRB.velocity = new Vector2(enemyRB.velocity.x, 0);
                enemyRB.AddForce(launchForce, ForceMode2D.Impulse);
                enemyKnockbackDuration -= Time.deltaTime;
                break;
            default:
                break;
        }
    }
    private void DropMeter()
    {
        Instantiate(energyDropped, transform.position, transform.rotation);
    }
    public void TakeDamage(int damageToTake, HurtEnemyOnHit.DamageEffect _effect)
    {
        if (!IsInvul)
        {
            if (!isEnemyInvincible)
            {
                currentEnemyHealth -= damageToTake;
                DropMeter();
            }
            switch (_effect)
            {
                case HurtEnemyOnHit.DamageEffect.stun:
                    enemyKnockbackDuration = maxEnemyStunDuration;
                    _damagedState = DamageState.stunned;
                    break;
                case HurtEnemyOnHit.DamageEffect.knockback:
                    enemyKnockbackDuration = enemyMaxKnockbackDuration;
                    _damagedState = DamageState.knockedback;
                    break;
                case HurtEnemyOnHit.DamageEffect.launch:
                    enemyKnockbackDuration = maxEnemyLaunchDuration;
                    _damagedState = DamageState.launched;
                    break;
                default:
                    break;
            }
            
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
        audioManager.PlaySound(enemyDeathSound);
        Instantiate(deathParticle, transform.position, transform.rotation);
        Instantiate(itemDropped, transform.position, transform.rotation);
        enemyAnim.SetTrigger("Death");
        isEnemyInvincible = true;
        this.gameObject.SetActive(false);
        //Destroy(this.gameObject);
    }
}
