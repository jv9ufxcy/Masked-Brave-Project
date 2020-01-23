using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    private Animator bombAnim;
    private Rigidbody2D bombRB;
    [SerializeField] private float currentBombTimer, maxBombTime = 10;
    [SerializeField] private float hitFreezeTime;
    private enum bombState {primed, cooking, exploding }
    // Start is called before the first frame update
    void Start()
    {
        bombAnim = GetComponent<Animator>();
        bombRB = GetComponent<Rigidbody2D>();
        currentBombTimer = maxBombTime;
    }

    // Update is called once per frame
    void Update()
    {
        currentBombTimer -= Time.deltaTime;
        if (currentBombTimer<(maxBombTime/2))
        {

        }
    }
    public void DoHitFreeze()
    {
        StartCoroutine(DoHitStop(hitFreezeTime));
    }
    public void DoHitKnockback(float knockbackDur, Vector2 hitDistance)
    {
        StartCoroutine(DoKnockback(knockbackDur, hitDistance));
    }
    public void DoStopAndKnockback(float knockbackDuration, Vector2 distance, float hitStopDuration)
    {
        StartCoroutine(DoHitStopAndKnockback(knockbackDuration, distance, hitStopDuration));
    }
    IEnumerator DoHitStop(float hitStopDuration)
    {
        Vector2 savedVelocity = bombRB.velocity;//get current velocity and save it
        bombRB.velocity = Vector2.zero;//set velocity to 0
        bombAnim.speed = 0;//set animator speed to 0
        //stop enemy from moving
        yield return new WaitForSeconds(hitStopDuration);

        bombRB.velocity = savedVelocity;//restore saved velocity
        bombAnim.speed = 1;//restore animator.speed to 1
    }
    IEnumerator DoHitStopAndKnockback(float knockbackDuration, Vector2 hitDistance, float hitStopDuration)
    {
        StartCoroutine(DoHitStop(hitStopDuration));

        yield return new WaitForSeconds(hitStopDuration);
        //allow enemy to move again unless you have something else for knockback
        //DoKnockback
        StartCoroutine(DoKnockback(knockbackDuration, hitDistance));
        yield return new WaitForSeconds(knockbackDuration);
    }
    private IEnumerator DoKnockback(float knockbackDur, Vector2 hitDistance)
    {
        bombRB.velocity = hitDistance;
        yield return new WaitForSeconds(knockbackDur);
    }
}
