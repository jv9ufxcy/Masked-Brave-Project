using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillPlayerUponExit : MonoBehaviour
{
    [SerializeField] private int killPlayerDamage = 100;
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //TODO: replace with respawn
            collision.gameObject.GetComponentInParent<Player>().KillPlayer(killPlayerDamage);
        }
        if (collision.gameObject.tag == "Enemy")
        {
            //TODO: replace with respawn
            collision.gameObject.GetComponentInParent<EnemyHealthManager>().TakeDamage(killPlayerDamage, HurtEnemyOnHit.DamageEffect.launch);
        }
    }
}