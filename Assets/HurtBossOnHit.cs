using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBossOnHit : MonoBehaviour
{

    [SerializeField] private int damageToGive = 2;
    private BossPatrolManager bossHP;


    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    private void OnTriggerEnter2D(Collider2D bossCollision)
    {
        bossHP = bossCollision.gameObject.GetComponentInParent<BossPatrolManager>();
        if (bossHP == null)
            return;
        if (bossCollision.CompareTag("Boss"))
        {
            //bossHP.TakeDamage(damageToGive);

            bossHP.enemyKnockbackDuration = bossHP.enemyMaxKnockbackDuration;
            if (bossCollision.transform.position.x < transform.position.x)
                bossHP.enemyKnockFromRight = true;
            else
                bossHP.enemyKnockFromRight = false;
        }
       
    }
}
