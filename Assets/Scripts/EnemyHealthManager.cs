using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class EnemyHealthManager : MonoBehaviour
{
    [SerializeField] private bool isEnemyDead = false;
    [SerializeField] private int currentEnemyHealth;
    [SerializeField] private int maxEnemyHealth;
    [SerializeField] private GameObject deathParticle, damageParticle;
    [SerializeField] private GameObject itemDropped;
    [SerializeField] private GameObject energyDropped;
    [SerializeField] private bool shouldDropAtHalf;

    public int hpStock=0, eStock=0, ammoStock=0;

    [Header("Knockback")]
    public DamageState _damagedState;
    public enum DamageState { stunned, ableToMove }
    [SerializeField] private bool isInvul;
    [SerializeField] private float damageCooldownInSeconds = .75f, deathFreezeTime = 0.1f;
    [Space]
    [Header("Detection")]
    [SerializeField] Transform wallDetectPoint;
    [SerializeField] Transform groundDetectPoint;
    [SerializeField] Transform edgeDetectPoint;
    [SerializeField] float DetectRadius = 0.2f;
    [SerializeField] LayerMask whatCountsAsWall;
    private bool notAtEdge;
    private bool hittingWall;
    private bool isOnGround;
    private bool canMove_UseProperty=true;

    private bool isFacingRight;
    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;
    private Animator enemyAnim;
    //audio
    private AudioManager audioManager;
    [SerializeField] private string enemyTakeDamageSound;
    [SerializeField] private string enemyDeathSound;
    private bool coroutineStarted = false;
    private Color defaultColor;
    private float defaultGravityScale;

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

    public bool CanMove { get => canMove_UseProperty; set => canMove_UseProperty = value; }

    public static EnemyHealthManager GetClosestEnemy(Vector3 position, float maxRange)
    {
        EnemyHealthManager closest = null;
        foreach (EnemyHealthManager enemy in enemyList)
        {
            if (Vector3.Distance(position, enemy.transform.position) <= maxRange)
            {
                if (closest == null)
                {
                    closest = enemy;
                }
                else
                {
                    if (Vector3.Distance(position, enemy.transform.position) < Vector3.Distance(position, closest.transform.position))
                    {
                        closest = enemy;
                    }
                }
            }
        }
        return closest;
    }
    private static List<EnemyHealthManager> enemyList = new List<EnemyHealthManager>();

    private void Awake()
    {
        enemyList.Add(this);
    }
    // Use this for initialization
    void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        currentEnemyHealth = maxEnemyHealth;
        //isEnemyDead = false;
        canMove_UseProperty = true;
        enemyRend = GetComponent<SpriteRenderer>();
        enemyAnim = GetComponent<Animator>();
        enemyRB = GetComponent<Rigidbody2D>();
        defaultColor = enemyRend.color;
        defaultGravityScale = enemyRB.gravityScale;

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void Update ()
    {
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
    IEnumerator DoHitStop(float knockbackDuration, Vector2 hitDistance, float hitStopDuration)
    {
        Vector2 savedVelocity = enemyRB.velocity;//get current velocity and save it

        canMove_UseProperty=false;//stop letting player move
        enemyRB.velocity = Vector2.zero;//set velocity to 0

        enemyAnim.speed = 0;//set animator speed to 0
        //stop enemy from moving
        yield return new WaitForSeconds(hitStopDuration);

        enemyRB.velocity = savedVelocity;//restore saved velocity
        enemyAnim.speed = 1;//restore animator.speed to 1

        //allow enemy to move again unless you have something else for knockback
        //DoKnockback
        enemyRB.velocity = hitDistance;
        yield return new WaitForSeconds(knockbackDuration);
        canMove_UseProperty=true;//let player move again
    }
    public void TakeDamage(int damageToTake,float knockbackDuration, Vector2 distance, float hitStopDuration)
    {
        if (!IsInvul)
        {
            if (!isEnemyDead)
            {
                currentEnemyHealth -= damageToTake;
            }
            Instantiate(damageParticle, transform.position, transform.rotation);
            StartCoroutine(DamageCooldownCoroutine());
            StartCoroutine(DoHitStop(knockbackDuration, distance, hitStopDuration));
            audioManager.PlaySound(enemyTakeDamageSound);
        }
    }
    private IEnumerator DamageCooldownCoroutine()
    {
        IsInvul = true;
        StartCoroutine(BlinkWhileInvulnerableCoroutine());
        yield return new WaitForSeconds(damageCooldownInSeconds);
        IsInvul = false;
    }
    private IEnumerator LaunchWait(float knockbackDuration)
    {
        coroutineStarted = true;
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);
        enemyRB.gravityScale = 0;
        yield return new WaitForSeconds(knockbackDuration);
        enemyRB.gravityScale = defaultGravityScale;
        coroutineStarted = false;
    }
    private IEnumerator BlinkWhileInvulnerableCoroutine()
    {
        
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
    void RigidbodyDrag(float x)
    {
        enemyRB.drag = x;
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
    private void FreezeTime()
    {
        Camera.main.transform.GetComponent<FreezeTime>().FreezeFrame(deathFreezeTime);
    }
    private void OnDeath()
    {
        //FreezeTime();
        Screenshake();
        audioManager.PlaySound(enemyDeathSound);
        Instantiate(deathParticle, transform.position, transform.rotation);
        Instantiate(itemDropped, transform.position, transform.rotation);
        enemyAnim.SetTrigger("Death");
        isEnemyDead = true;
        this.gameObject.SetActive(false);
        //Destroy(this.gameObject);
    }
}
