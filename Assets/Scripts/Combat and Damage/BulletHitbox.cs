using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitbox : MonoBehaviour
{
    private List<Collider2D> enemies = new List<Collider2D>();
    public int projectileIndex = 0, atkIndex = 0;
    public CharacterObject character;
    private BombController bomb;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;

    // Start is called before the first frame update
    void Start()
    {
        if (character == null)
        {
            character = GetComponentInParent<CharacterObject>();
            //character = transform.root.GetComponent<CharacterObject>();
        }
            bomb = GetComponentInParent<BombController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (bomb.hitActive > 0 && !enemies.Contains(collision))
        {
            IHittable victim = collision.transform.root.GetComponent<IHittable>();
            if (victim != null)
            {
                enemies.Add(collision);
                victim.Hit(character, projectileIndex, atkIndex);
            }
            if (collision.gameObject.CompareTag("Shootable"))
            {
                HealthManager destructible = collision.transform.root.GetComponent<HealthManager>();
                enemies.Add(collision);
                destructible.RemoveHealth(1, null);
            }
        }
    }
    public void RestoreGetHitBools()
    {
        enemies.Clear();
    }
}
