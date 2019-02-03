using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField]
    private int damageToGive = 1;

    private void OnTriggerStay2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponentInParent<Player>();

        if (player != null)
        {
            player.TakeDamage(damageToGive);
            Debug.Log("Player Hit Ponts" + player.CurrentHitPoints);
        }
    }
}
