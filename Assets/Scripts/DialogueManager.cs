using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    public DialogueTrigger currentDialogue;

    public TextMeshProUGUI nameText, dialogueText;
    public Image profileImage;
    public RectTransform dialogueParent;

    public Vector3 dialoguePos, offScreenPos;
    public float tweenSpeed = 0.5f;

    public bool isDialogueActive=false;

    private Queue<Dialogue> sentences;
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }
    void Start()
    {
        dialogueParent.DOScaleY(0f, 0f);
        sentences = new Queue<Dialogue>();
    }
    private void Update()
    {
        if (isDialogueActive)
        {
            DialogueControls();
            GameEngine.gameEngine.mainCharacter.velocity = Vector2.zero;
        }
    }

    private void DialogueControls()
    {
        if (Input.GetButtonDown(GameEngine.coreData.rawInputs[0].name))
        {
            DisplayNextSentence();
        }
        if (Input.GetButtonDown(GameEngine.coreData.rawInputs[8].name))
        {
            EndDialogue();
        }
    }

    public void StartDialogue(Dialogue[] dialogue, DialogueTrigger trigger)
    {
        GameEngine.gameEngine.mainCharacter.UpdateCharacter();
        if (!isDialogueActive)
        {
            dialogueParent.DOScaleY(1f, tweenSpeed);
        }
        isDialogueActive = true;
        //PauseManager.IsGamePaused = true;
        currentDialogue = trigger;

        sentences.Clear();
        foreach (Dialogue sentence in dialogue)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    private void DisplayNextSentence()
    {
        if (sentences.Count==0)
        {
            EndDialogue();
            return;
        }
        Dialogue dialogue = sentences.Dequeue();

        nameText.text = dialogue.name;
        profileImage.sprite = dialogue.profile;
        //dialogueText.text = sentence;
        string sentence = dialogue.sentences;
        StopAllCoroutines();
        StartCoroutine(Type(sentence));
    }
    IEnumerator Type(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForFixedUpdate();
        }
    }
    private void EndDialogue()
    {
        currentDialogue.EndDialogue();
        //PauseManager.IsGamePaused = false;
        isDialogueActive = false;
        dialogueParent.DOScaleY(0f, tweenSpeed);
    }
}
