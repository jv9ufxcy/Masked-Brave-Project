using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float jumpPow = 2f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        collision.gameObject.GetComponent<CharacterObject>().Jump(jumpPow);
    }
}
