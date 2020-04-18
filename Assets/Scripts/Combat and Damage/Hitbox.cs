using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    //public List<Collider2D> colliders;
    public CharacterObject character;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;

    // Start is called before the first frame update
    void Start()
    {
        character = transform.root.GetComponent<CharacterObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject!=transform.root.gameObject)
        {
            if (character.hitActive>0)
            {
                CharacterObject victim = collision.transform.root.GetComponent<CharacterObject>();
                if (victim!=null)
                    victim.GetHit(character);
            }
            //Debug.Log("Hit");
        }
    }
}
