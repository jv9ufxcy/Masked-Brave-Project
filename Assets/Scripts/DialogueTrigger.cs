using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue[] dialogue;
    public UnityEvent OnDialogueStart, OnDialogueEnd;
    public bool shouldMoveCharacter = false;
    public Transform[] character, characterPos;

    private void TriggerDialogue()
    {
        DialogueManager.instance.StartDialogue(dialogue, this);
        GameEngine.gameEngine.mainCharacter.StartStateFromScript(0);
        MusicManager.instance.StopMusic();
        if (shouldMoveCharacter)
        {
            for (int i = 0; i < character.Length; i++)
            {
                character[i].DOMove(characterPos[i].position,1f);
            }
        }
    }
    public void BeginDialogue() { OnDialogueStart.Invoke(); TriggerDialogue(); }
    public void EndDialogue() { OnDialogueEnd.Invoke(); }


}
