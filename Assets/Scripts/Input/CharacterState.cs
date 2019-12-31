using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterState
{
    public string stateName;
    public int index;

    public float length;
    public int frame;

    public float currentTime;
    public int currentState;

    public List<StateEvent> events;
}
[System.Serializable]
public class StateEvent
{
    public float start;
    public float end;
    public float variable;
    public int script;
}
[System.Serializable]
public class CharacterScriot
{
    [HideInInspector]
    public int index;

    public string name;
    public float variable;
}