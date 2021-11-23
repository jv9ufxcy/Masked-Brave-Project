using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpPow = 2f, lifeTime=4f;
    public bool isDestructible = false, aerialOnly = false;

    public InteractableObject charaSpawn;
    public enum ObjectType { LEVEL, CONTACT, TIMED };
    public ObjectType objType;
    private void Start()
    {
        if (isDestructible)
        {
            charaSpawn = GetComponent<InteractableObject>();
        }
    }
    private void FixedUpdate()
    {
        switch (objType)
        {
            case ObjectType.LEVEL:
                break;
            case ObjectType.CONTACT:
                break;
            case ObjectType.TIMED:
                if (lifeTime > 0)
                    lifeTime -= Time.fixedDeltaTime;
                else
                    StartCoroutine(DestroyJumpPad());
                break;
            default:
                break;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        CharacterObject chara = collision.gameObject.GetComponent<CharacterObject>();
        if (chara != null)
        {
            if ((aerialOnly && chara.aerialFlag) || !aerialOnly)
            {
                chara.jumps++;
                chara.VelocityY(chara.MaxJumpVelocity*jumpPow);
                //Debug.Log("Jumped at speed of " + chara.MaxJumpVelocity * jumpPow);
                chara.canCancel = true;
                switch (objType)
                {
                    case ObjectType.LEVEL:
                        break;
                    case ObjectType.CONTACT:
                        StartCoroutine(DestroyJumpPad());
                        break;
                    case ObjectType.TIMED:
                        StartCoroutine(DestroyJumpPad());
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private void OnTriggerStay2D(Collider2D coll)
    {
        CharacterObject chara = coll.gameObject.GetComponent<CharacterObject>();
        if (chara != null)
        {
            if ((aerialOnly && chara.aerialFlag) || !aerialOnly)
            {
                chara.jumps++;
                chara.VelocityY(chara.MaxJumpVelocity * jumpPow);
                Debug.Log("Jumped at speed of " + chara.MaxJumpVelocity * jumpPow);
                chara.canCancel=true;
                switch (objType)
                {
                    case ObjectType.LEVEL:
                        break;
                    case ObjectType.CONTACT:
                        StartCoroutine(DestroyJumpPad());
                        break;
                    case ObjectType.TIMED:
                        StartCoroutine(DestroyJumpPad());
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private IEnumerator DestroyJumpPad()
    {
        yield return new WaitForFixedUpdate();
            charaSpawn.DeSpawn();
    }
}
