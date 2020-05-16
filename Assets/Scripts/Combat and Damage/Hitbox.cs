using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private List<Collider2D> enemies = new List<Collider2D>();
    public int projectileIndex = 0;
    public CharacterObject character;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;

    // Start is called before the first frame update
    void Start()
    {
        if (character==null)
            character = transform.root.GetComponent<CharacterObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (projectileIndex>0)
        {
            if (!enemies.Contains(collision))
            {
                CharacterObject victim = collision.transform.root.GetComponent<CharacterObject>();
                if (victim != null)
                {
                    victim.GetHit(character, projectileIndex);
                    enemies.Add(collision);
                }
            }
        }
        else
        {
            if (character.hitActive > 0)
            {
                if (!enemies.Contains(collision))
                {
                    CharacterObject victim = collision.transform.root.GetComponent<CharacterObject>();
                    if (victim != null)
                    {
                        victim.GetHit(character, projectileIndex);
                        enemies.Add(collision);
                    }
                }
            }
        }
    }
    public void RestoreGetHitBools()
    {
        enemies.Clear();
    }
}
