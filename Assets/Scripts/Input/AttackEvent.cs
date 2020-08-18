using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackEvent
{
    public float start;
    public float length;
    public float hitStun;
    public float hitStop;

    public int damage;
    public int meterGain;
    public int blastBlight;
    public int poiseDamage=1;

    public Vector2 hitAnim;
    public Vector3 knockback;

    public Vector3 hitBoxPos;
    public Vector3 hitBoxScale;

    public float cancelWindow;
}
