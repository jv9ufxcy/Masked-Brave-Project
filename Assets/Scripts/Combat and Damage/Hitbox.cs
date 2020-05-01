using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    //public static List<EnemySpawn> enemies;
    private List<Collider2D> enemies = new List<Collider2D>();
    public List<Collider2D> GetColliders() { return enemies; }
    public CharacterObject character;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;

    // Start is called before the first frame update
    void Start()
    {
        character = transform.root.GetComponent<CharacterObject>();
        //enemies = new List<EnemySpawn>();
        //EnemySpawn[] allEnemies = FindObjectsOfType<EnemySpawn>();
        //foreach (EnemySpawn go in allEnemies)
        //{
        //    if (!enemies.Contains(go))
        //    {
        //        enemies.Add(go);
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (character.hitActive > 0)
        {
            if (!enemies.Contains(collision))
            {
                CharacterObject victim = collision.transform.root.GetComponent<CharacterObject>();
                if (victim != null)
                {
                    victim.GetHit(character);
                    enemies.Add(collision);
                }
            }
        }
    }
    public void RestoreGetHitBools()
    {
        enemies.Clear();
    }
}
