using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtPlayerOnHit : MonoBehaviour
{
    [SerializeField] private int damageToGive = 1;
    [SerializeField] private float knockbackToGive = 0.2f;
    private Player player;
    private EnemyHealthManager enemyHM;

    private void Start()
    {
            enemyHM = GetComponentInParent<EnemyHealthManager>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        player=collision.gameObject.GetComponentInParent<Player>();
        if (collision.CompareTag("Player") && !player.IsInvulnerable && !enemyHM.IsInvul)
        {
            player.TakeDamage(damageToGive, knockbackToGive, enemyHM);
            enemyHM.DoHitFreeze();

            if (collision.transform.position.x < transform.position.x)
                player.knockbackFromRight = true;
            else
                player.knockbackFromRight = false;
        }
    }
}
