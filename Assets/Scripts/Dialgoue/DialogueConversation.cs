using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/ConversationObject")]
public class DialogueConversation : ScriptableObject
{
    public List<Dialogue> line = new List<Dialogue>();
}
