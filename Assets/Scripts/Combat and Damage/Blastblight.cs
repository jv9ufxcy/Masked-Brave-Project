using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blastblight : MonoBehaviour
{
    [SerializeField] private int blastBuildup, blastThreshold = 100, blastIncrease=45, blastLimit=1000;
    public CharacterObject player;
    [SerializeField] private GameObject bombObject;
    public HealthManager enemyHealth;
    // Start is called before the first frame update
    void Start()
    {
        player = GameEngine.gameEngine.mainCharacter;
        enemyHealth = GetComponentInParent<HealthManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (blastBuildup>=blastThreshold)
        {
            Explosion();
        }
        if (enemyHealth.IsDead)
        {
            InstantiateExplosion();
            Destroy(gameObject, .1f);
        }
    }
    public void AddBlast(int blightDamage)
    {
        blastBuildup += blightDamage;
    }
    public void Explosion()
    {
        if(blastThreshold<=blastLimit)
            blastThreshold += blastIncrease;

        blastBuildup = 0;
        InstantiateExplosion();
    }
    private void InstantiateExplosion()
    {
        GameObject bomb = Instantiate(bombObject, transform.position, Quaternion.identity);
        bomb.transform.parent = transform;
        bomb.GetComponentInChildren<Hitbox>().character = GameEngine.gameEngine.mainCharacter;
        bomb.GetComponent<BombController>().Detonate();
    }
    private void DeathExplosion()
    {

    }
}
