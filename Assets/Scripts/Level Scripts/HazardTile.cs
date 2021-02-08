using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardTile : MonoBehaviour
{
    [SerializeField] private int projectileIndex = 61;
    public CharacterObject character;

    private void OnCollisionStay2D(Collision2D collision)
    {
        IHittable victim = collision.transform.root.GetComponent<IHittable>();
        if (victim != null)
        {
            victim.Hit(character, projectileIndex,0);
        }
    }
}
