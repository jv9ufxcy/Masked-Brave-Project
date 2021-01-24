using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    private enum BombState {primed, cooking, exploding }
    private BombState _bombState;

    private AudioManager audioManager;
    private Rigidbody2D bombRB;
    private Animator bombAnim;

    [SerializeField] private float currentBombTimer, maxBombTime = 10, explosionLength=0.8f;
    [SerializeField] private float hitFreezeTime;

    [SerializeField] private string spawnSound, tickSound, explosionSound,explosionAnim;

    private bool hasExploded=false;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        hasExploded = false;
        bombAnim = GetComponent<Animator>();
        bombRB = GetComponent<Rigidbody2D>();
        currentBombTimer = maxBombTime;

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (_bombState)
        {
            case BombState.primed:
                currentBombTimer -= Time.deltaTime;
                bombAnim.SetBool("IsCooking", false);
                if (currentBombTimer <= (maxBombTime / 2))//less than half time
                {
                    audioManager.PlaySound(tickSound);
                    _bombState = BombState.cooking;
                }
                break;
            case BombState.cooking:
                currentBombTimer -= Time.deltaTime;
                bombAnim.SetBool("IsCooking", true);
                if (currentBombTimer <= 0)//end of time;
                {
                    audioManager.PlaySound(tickSound);
                    _bombState = BombState.exploding;
                }
                break;
            case BombState.exploding:
                bombRB.velocity = Vector2.zero;
                if (!hasExploded)
                    StartCoroutine(Explosion());
                break;
            default:
                break;
        }
        
    }
    public void Detonate()
    {
        _bombState = BombState.exploding;
    }
    public void TakeDamage(int damageToTake)
    {
        if (!hasExploded)
            currentBombTimer -= damageToTake;
    }
    //public void DoHitFreeze()
    //{
    //    if (!hasExploded)
    //        StartCoroutine(DoHitStop(hitFreezeTime));
    //}
    //public void DoHitKnockback(float knockbackDur, Vector2 hitDistance)
    //{
    //    if (!hasExploded)
    //        StartCoroutine(DoKnockback(knockbackDur, hitDistance));
    //}
    //public void DoStopAndKnockback(float knockbackDuration, Vector2 distance, float hitStopDuration)
    //{
    //    if (!hasExploded)
    //        StartCoroutine(DoHitStopAndKnockback(knockbackDuration, distance, hitStopDuration));
    //}
    //IEnumerator DoHitStop(float hitStopDuration)
    //{
    //    Vector2 savedVelocity = bombRB.velocity;//get current velocity and save it
    //    bombRB.velocity = Vector2.zero;//set velocity to 0
    //    bombAnim.speed = 0;//set animator speed to 0
    //    //stop enemy from moving
    //    yield return new WaitForSeconds(hitStopDuration);

    //    bombRB.velocity = savedVelocity;//restore saved velocity
    //    bombAnim.speed = 1;//restore animator.speed to 1
    //}
    //IEnumerator DoHitStopAndKnockback(float knockbackDuration, Vector2 hitDistance, float hitStopDuration)
    //{
    //    StartCoroutine(DoHitStop(hitStopDuration));

    //    yield return new WaitForSeconds(hitStopDuration);
    //    //allow enemy to move again unless you have something else for knockback
    //    //DoKnockback
    //    StartCoroutine(DoKnockback(knockbackDuration, hitDistance));
    //    yield return new WaitForSeconds(knockbackDuration);
    //}
    //private IEnumerator DoKnockback(float knockbackDur, Vector2 hitDistance)
    //{
    //    bombRB.velocity = hitDistance;
    //    yield return new WaitForSeconds(knockbackDur);
    //}
    public IEnumerator Explosion()
    {
        hasExploded = true;
        bombAnim.Play(explosionAnim);
        audioManager.PlaySound(explosionSound);
        bombRB.velocity = Vector2.zero;
        yield return new WaitForSeconds(explosionLength);
        Destroy(gameObject);
    }
}
