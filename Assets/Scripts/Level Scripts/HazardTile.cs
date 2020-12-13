using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardTile : MonoBehaviour
{
    [SerializeField] private int projectileIndex = 61;
    public CharacterObject character;

    private void OnCollisionStay2D(Collision2D collision)
    {
        CharacterObject victim = collision.transform.root.GetComponent<CharacterObject>();
        if (victim != null)
        {
            victim.GetHit(character, projectileIndex,0);
        }
    }
}
