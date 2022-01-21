using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardTile : MonoBehaviour
{
    [SerializeField] private int projectileIndex = 61;
    public CharacterObject character;
    public LayerMask nonInteractableLayer;
    private void OnCollisionStay2D(Collision2D collision)
    {
        IHittable victim = collision.transform.root.GetComponent<IHittable>();
        if (victim != null&& nonInteractableLayer!=(nonInteractableLayer|(1<<collision.gameObject.layer)))
        {
            victim.Hit(character, projectileIndex, 0);
        }
    }
}
