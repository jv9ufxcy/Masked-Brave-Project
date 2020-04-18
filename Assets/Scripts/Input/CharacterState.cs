using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterState
{
    public string stateName;
    public int index;

    public float length;
    public bool loop;
    public float blendRate = 0.1f;

    //public int frame;

    //public float currentTime;
    //public int currentState;

    public List<StateEvent> events;
    public List<AttackEvent> attacks;

    public int jumpReq;
    public int meterReq;
    public float dashCooldownReq;
    public bool groundedReq;

    public bool ConditionsMet(CharacterObject character)
    {
        if (character.jumps<jumpReq){ return false;}

        if (groundedReq&&character.aerialFlag){ return false;}

        if (dashCooldownReq>0)
        {
            if (character.dashCooldown > 0) { return false; }
            else { character.dashCooldown = dashCooldownReq; }
        }
        if (meterReq>0)
        {
            if (character.specialMeter <meterReq) { return false; }
            else { character.UseMeter(meterReq); }
        }
        
        //else { character.jumps--; }
        return true;
    }
}
[System.Serializable]
public class StateEvent
{
    public float start;
    public float end;
    public float variable;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.SCRIPTS)]
    public int script;
}
[System.Serializable]
public class CharacterScript
{
    [HideInInspector]
    public int index;

    public string name;
    public float variable;
}
[System.Serializable]
public class InputCommand
{
    public string inputString;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int state;
}