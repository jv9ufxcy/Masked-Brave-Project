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
    //[IndexedItem(IndexedItemAttribute.IndexedItemType.RAW_INPUTS)]
    public int input;

    public string inputString;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int state;
}
[System.Serializable]
public class CommandState
{
    public string stateName;
    //flags
    public bool aerial;
    //explicit state
    public bool explicitState;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int state;

    public List<CommandStep> commandSteps;

    [HideInInspector] public List<int> omitList;
    [HideInInspector] public List<int> nextFollowups;
    public CommandState()
    {
        commandSteps = new List<CommandStep>();
        stateName = "NEW COMMAND STATE";
    }
    public CommandStep AddCommandStep()
    {
        foreach (CommandStep s in commandSteps)
        {
            if (!s.activated) { s.activated = true;return s; }
        }
        CommandStep nextStep = new CommandStep(commandSteps.Count);
        nextStep.activated = true;
        commandSteps.Add(nextStep);
        return nextStep;
    }
    public void RemoveChainCommands(int _id)
    {
        if (_id == 0) { return; }
        commandSteps[_id].activated = false;
        commandSteps[_id].followUps = new List<int>();
    }

    public void CleanUpBaseState()
    {

        omitList = new List<int>();

        for (int s = 1; s < commandSteps.Count; s++)
        {
            for (int f = 0; f < commandSteps[s].followUps.Count; f++)
            {
                omitList.Add(commandSteps[s].followUps[f]);
            }
        }

        nextFollowups = new List<int>();
        for (int s = 1; s < commandSteps.Count; s++)
        {
            bool skip = false;
            for (int m = 0; m < omitList.Count; m++)
            {
                if (omitList[m] == s) { skip = true; }
            }
            if (!skip) { nextFollowups.Add(s); }

        }

        commandSteps[0].followUps = nextFollowups;

    }
}
[System.Serializable]
public class CommandStep
{
    public int idIndex;

    public InputCommand command;

    //[IndexedItem(IndexedItemAttribute.IndexedItemType.CHAIN_COMMAND)]
    public List<int> followUps;

    public bool strict; //Alternatively you could make a whole new Command State

    [HideInInspector]
    public Rect myRect;

    public bool activated;

    public void AddFollowUp(int _nextID)
    {
        if (_nextID == 0) { return; }
        if (idIndex == _nextID) { return; }
        for (int i = 0; i < followUps.Count; i++)
        {

            if (followUps[i] == _nextID) { return; }
        }
        followUps.Add(_nextID);
    }

    public CommandStep(int _index)
    {
        idIndex = _index;
        followUps = new List<int>();
        command = new InputCommand();
        myRect = new Rect(50, 50, 200, 200);
    }
}
