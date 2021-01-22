using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeTrap : MonoBehaviour
{
    [SerializeField]
    private float groundDetectRadius;

    [SerializeField]
    private Transform groundDetectPoint;

    [SerializeField]
    private LayerMask areasToLandOn;//our groundobjects

    [SerializeField]
    private float maxGroundTime = 0.3f;

    [SerializeField]
    private float freezeTime=2f, lifeTime=6f;

    [SerializeField] private string stickSound, landSound, releaseSound;
    [SerializeField] private ParticleSystem burstFree;

    public enum TrapState { airborne, airHold, grounded, groundHold};
    public TrapState _stateOfTrap;
    private bool landed = false, active=false, isOnGround;
    private float groundTimer = 0;
    private Rigidbody2D slimeRB;
    private SpriteRenderer renderer;
    private BoxCollider2D coll;
    private Animator slimeAnim;
    private AudioManager audioManager;
    private Player moveScript;
    public ParabolaController trapParabola;
    // Start is called before the first frame update
    void Start()
    {
        audioManager = AudioManager.instance;
        slimeAnim = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        slimeRB = GetComponent<Rigidbody2D>();
        trapParabola = GetComponent<ParabolaController>();
    }

    // Update is called once per frame
    void Update()
    {
        slimeAnim.SetBool("IsActive", active);
        slimeAnim.SetBool("IsOnGround", landed);

        if (active&&moveScript!=null)
        {
            trapParabola.enabled = false;
            moveScript.transform.position = transform.position;
            if (IsOnGround())
            {
                _stateOfTrap = TrapState.groundHold;
            }
            else
                _stateOfTrap = TrapState.airHold;
        }
        else
        {
            CheckLifeTime();
        }
        switch (_stateOfTrap)
        {
            case TrapState.airborne:
                GroundCheck();
                break;
            case TrapState.airHold:
                GroundCheck();
                break;
            case TrapState.grounded:
                transform.position = this.transform.position;
                break;
            case TrapState.groundHold:
                transform.position = this.transform.position;
                break;
            default:
                break;
        }
        
    }

    private void CheckLifeTime()
    {
        if (lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;
        }
        else if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void GroundCheck()
    {
        if (IsOnGround())//if it is touching a  ground tile
        {
            
            if (!landed && groundTimer > maxGroundTime)//if it hasn't been grounded already and it's touched the ground for a little bit
                SetTrapGrounded();//arm it

            groundTimer += Time.deltaTime;//if it hasn't touched long enough and it's still on the ground, keep counting
        }
        else
            groundTimer = 0;
    }
    void SetTrapGrounded()
    {
        trapParabola.enabled = false;
        //slimeAnim.SetBool("IsOnGround", true);//set the animator to do the ground animation
        slimeRB.velocity = Vector2.zero;//no sliding
        landed = true;//set the ice cream to armed
        audioManager.PlaySound(landSound);
        _stateOfTrap = TrapState.grounded;
    }
    private bool IsOnGround()
    {
        Collider2D[] groundObjects = Physics2D.OverlapCircleAll(groundDetectPoint.position, groundDetectRadius, areasToLandOn);
        return groundObjects.Length > 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)//CHANGE THIS TO OVERLAP CIRCLE!!!
    {
        if (collision.tag == "Player")//if a player is in range
        {
            moveScript = collision.GetComponentInParent<Player>();
            StartCoroutine(TrapPlayer(moveScript));
        }
    }
    private IEnumerator TrapPlayer(Player moveScript)
    {
        active = true;//can't be used again
        //coll.enabled = false;

        //AUDIO play freeze sfx
        audioManager.PlaySound(stickSound);

        yield return new WaitForSeconds(freezeTime);

        //AUDIO play thaw sfx
        audioManager.PlaySound(releaseSound);
        //burstFree.Play();
        Destroy(gameObject);//destroy after has been used
    }
}
