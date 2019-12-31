using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Core Data", menuName = "Character Action/Core Data", order = 1)]
public class CoreData : ScriptableObject
{
    //Character States
    public List<CharacterState> globalStates;

    public List<CharacterScriot> characterScripts;

    //Save Files
}
