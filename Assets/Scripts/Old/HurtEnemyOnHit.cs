using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class HurtEnemyOnHit : MonoBehaviour
{
    public DamageEffect _effect;
    public enum DamageEffect { stun, knockback, launch}
    [Header("Damage Stats")]
    [SerializeField] private Vector2 hitDistance;
    [SerializeField] private int damageToGive = 1, meterToGive = 1;
    [SerializeField] private float hitStopDuration = 0.1f, knockbackDuration = 0.5f;
    [SerializeField] private bool shouldScreenshakeOnHit = false, shouldHitStop = true;
    [SerializeField] private ParticleSystem hitSpark;
    private EnemyHealthManager enemyHP;
    private BossPatrolManager bossHP;
    private BombController bombHP;
    private BulletHit bulletController;

    private Player player;
    private AudioManager audioManager;

    
    private void Start()
    {
        player = Player.Instance;

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    private void OnTriggerEnter2D(Collider2D enemyColl)
    {
        Vector2 enemySideDistance = hitDistance;
        enemyHP = enemyColl.gameObject.GetComponentInParent<EnemyHealthManager>();
        bossHP = enemyColl.GetComponentInParent<BossPatrolManager>();
        bombHP = enemyColl.GetComponent<BombController>();
        bulletController = enemyColl.GetComponent<BulletHit>();
        if (enemyHP!=null)
        {
            if (enemyColl.transform.position.x < transform.position.x)
                enemySideDistance.x *= -1;//is on ur right
            else
                enemySideDistance.x *= 1;//is on ur left

            enemyHP.TakeDamage(damageToGive, knockbackDuration, enemySideDistance, hitStopDuration);
            if (shouldScreenshakeOnHit)
                Screenshake();
            if (player != null)
                player.AddMeter(meterToGive);
        }
        if (bombHP != null)
        {
            if (enemyColl.transform.position.x < transform.position.x)
                enemySideDistance.x *= -1;//is on ur right
            else
                enemySideDistance.x *= 1;//is on ur left

            bombHP.TakeDamage(damageToGive);
            //bombHP.DoStopAndKnockback(knockbackDuration, enemySideDistance, hitStopDuration);
            if (shouldScreenshakeOnHit)
                Screenshake();
        }

        if (bulletController!=null)
        {
            bulletController.ReverseForce();
        }

        if (shouldHitStop)
            player.DoHitStop(hitStopDuration);
        if (hitSpark!=null)
            hitSpark.Play();
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
    
}
