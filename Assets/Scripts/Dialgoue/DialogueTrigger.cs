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
    private CharacterObject[] charObj;
    private void TriggerDialogue()
    {
        DialogueManager.instance.StartDialogue(dialogue, this);
        GameEngine.gameEngine.mainCharacter.StartStateFromScript(0);
        //MusicManager.instance.StopMusic();
        
    }

    IEnumerator MoveCharactersIntoPosition()
    {
        for (int i = 0; i < character.Length; i++)
        {
            while (Vector2.Distance(character[i].position,characterPos[i].position)>1f)
            {
                float distance = characterPos[i].position.x - character[i].position.x;
                float dir = Mathf.Sign(distance);
                float moveSpeed = 8f;
                charObj[i].FaceDir(dir);
                charObj[i].FrontVelocity(moveSpeed);
                charObj[i].CutsceneUpdatePhysics();
                yield return new WaitForFixedUpdate();
            }

            charObj[i].FaceTarget((characterPos[0].position+characterPos[characterPos.Length-1].position)/2);
            charObj[i].FrontVelocity(0);
            charObj[i].CutsceneUpdatePhysics();
        }
        yield return null;
        //float moveTime = 1f;
        //for (int i = 0; i < character.Length; i++)
        //{
        //    character[i].DOMoveX(characterPos[i].position.x, moveTime);
        //}
        //yield return new WaitForSeconds(moveTime);
    }
    private IEnumerator MoveAndTalk()
    {
        yield return StateWait();
        yield return GravityWait();
        yield return new WaitForFixedUpdate();
        yield return MoveCharactersIntoPosition();
        TriggerDialogue();
    }
    private IEnumerator GravityWait()
    {
        while (!AreActorsGrounded())
        {
            foreach (CharacterObject character in charObj)
            {
                character.CutsceneUpdatePhysics();
            }
            yield return null;
        }
    }
    private IEnumerator StateWait()
    {
        //while (!AreActorsNeutral())
        //{
        //    foreach (CharacterObject character in charObj)
        //    {
        //        character.UpdateCharacter();
        //    }
        //    yield return null;
        //}
        foreach (CharacterObject character in charObj)
        {
            character.StartStateFromScript(0);
        }
        yield return null;
    }
    public bool AreActorsNeutral()
    {
        foreach (CharacterObject character in charObj)
        {
            if (character.currentState!=0)
            {
                return false;
            }
        }
        return true;
    }
    public bool AreActorsGrounded()
    {
        foreach (CharacterObject character in charObj)
        {
            if (!character.IsGrounded())
            {
                return false;
            }
        }
        return true;
    }
    public void BeginDialogue() 
    { 
        OnDialogueStart.Invoke(); /*TriggerDialogue();*/
        if (shouldMoveCharacter)
        {
            charObj = new CharacterObject[character.Length];
            for (int i = 0; i < character.Length; i++)
            {
                charObj[i] = character[i].gameObject.GetComponent<CharacterObject>();
            }
            StartCoroutine(MoveAndTalk());
        }
        else
            TriggerDialogue();
    }
    public void EndDialogue() { OnDialogueEnd.Invoke(); }


}
