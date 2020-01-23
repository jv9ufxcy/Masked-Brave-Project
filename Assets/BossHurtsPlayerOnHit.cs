using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHurtsPlayerOnHit : MonoBehaviour
{
    [SerializeField] private int damageToGive = 1;
    [SerializeField] private float knockbackToGive = 0.2f;
    private Player player;
    private BossHealthManager bossHM;

    private void Start()
    {
        bossHM = GetComponentInParent<BossHealthManager>();
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponentInParent<Player>();
        if (player == null)
            return;
        if (bossHM == null)
            return;
        if (collision.CompareTag("Player") && !player.IsInvulnerable && !bossHM.IsInvul)
        {
            player.TakeDamage(damageToGive, knockbackToGive, null);

            if (collision.transform.position.x < transform.position.x)
                player.knockbackFromRight = true;
            else
                player.knockbackFromRight = false;
        }
        //foreach (Component arg in collision.gameObject.GetComponents(typeof(Component)))
        //{
        //    Debug.Log("Component: "+arg);
        //}
    }
}
