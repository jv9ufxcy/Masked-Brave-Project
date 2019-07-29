using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class HurtEnemyOnHit : MonoBehaviour
{
    [SerializeField] private int damageToGive = 1;
    [SerializeField] private int meterToGive = 1;
    public DamageEffect _effect;
    public enum DamageEffect { stun, knockback, launch}

    [SerializeField] private bool shouldScreenshakeOnHit = false, shouldScreenFreeze = false;
    private EnemyHealthManager enemyHP;
    private BossHealthManager bossHP;

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
        enemyHP = enemyColl.gameObject.GetComponentInParent<EnemyHealthManager>();
        bossHP = enemyColl.GetComponentInParent<BossHealthManager>();
        if (enemyHP!=null)
        {
            enemyHP.TakeDamage(damageToGive, _effect);

            if (enemyColl.transform.position.x < transform.position.x)
                enemyHP.enemyKnockFromRight = true;
            else
                enemyHP.enemyKnockFromRight = false;
        }
        if (bossHP!=null)
        {
            bossHP.TakeDamage(damageToGive);

            bossHP.enemyKnockbackDuration = bossHP.enemyMaxKnockbackDuration;
            if (bossHP.transform.position.x < transform.position.x)
                bossHP.enemyKnockFromRight = true;
            else
                bossHP.enemyKnockFromRight = false;
        }
        if(player!=null)
            player.AddMeter(meterToGive);

        if (shouldScreenshakeOnHit)
            Screenshake();
        if (shouldScreenFreeze)
            FreezeTime();
    }
    private void FreezeTime()
    {
        Camera.main.transform.GetComponent<FreezeTime>().FreezeFrame();
    }
    private static void Screenshake()
    {
        Camera.main.transform.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }
    
}
