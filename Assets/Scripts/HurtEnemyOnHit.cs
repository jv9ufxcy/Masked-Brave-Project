using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class HurtEnemyOnHit : MonoBehaviour
{
    [SerializeField] private int damageToGive = 1;
    public DamageEffect _effect;
    public enum DamageEffect { stun, knockback, launch}

    [SerializeField] private bool shouldScreenshakeOnHit = false;

    private EnemyHealthManager enemyHP;
    private EnemyHealthManager bossHP;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    private void OnTriggerStay2D(Collider2D enemyCollision)
    {
        if (shouldScreenshakeOnHit)
            Screenshake();

        enemyHP = enemyCollision.gameObject.GetComponentInParent<EnemyHealthManager>();
        if (enemyCollision.CompareTag("Enemy"))
        {
            enemyHP.TakeDamage(damageToGive, _effect);

            if (enemyCollision.transform.position.x < transform.position.x)
                enemyHP.enemyKnockFromRight = true;
            else
                enemyHP.enemyKnockFromRight = false;
        }
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
}
