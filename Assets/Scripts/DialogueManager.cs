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
    private AudioManager audioManager;
    private MusicManager musicManager;
    public FMODUnity.EventReference dialogueTheme;
    public TextMeshProUGUI nameText, dialogueText, tooltipText;
    public Image profileImage,cutsceneImage,borderImage;
    public RectTransform dialogueParent, tooltipParent;

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
        audioManager = AudioManager.instance;
        musicManager = MusicManager.instance;

        dialogueParent.DOScaleY(0f, 0f);
        tooltipParent.DOScaleY(0f, 0f);

        cutsceneImage.sprite = null;
        borderImage.DOFade(0, 0);
        cutsceneImage.DOFade(0, 0);

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
        if (dialogue.cutscene!=null)
        {
            cutsceneImage.sprite = dialogue.cutscene;
            borderImage.DOFade(1, 1f);
            cutsceneImage.DOFade(1, 1f);
        }
        else
        {
            FadeCutscene();
        }
        //dialogueText.text = sentence;
        string sentence = dialogue.sentences;
        StopAllCoroutines();
        StartCoroutine(Type(sentence));
    }

    private void FadeCutscene()
    {
        cutsceneImage.sprite = null;
        borderImage.DOFade(0, tweenSpeed);
        cutsceneImage.DOFade(0, tweenSpeed);
    }

    public string charSound = "Cutscene/Dialogue Scroll", radioSound = "Cutscene/Radio Over";
    IEnumerator Type(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            audioManager.PlaySound(charSound);
            yield return new WaitForFixedUpdate();
        }
    }
    private void EndDialogue()
    {
        FadeCutscene();
        isDialogueActive = false;
        dialogueParent.DOScaleY(0f, tweenSpeed);
        currentDialogue.EndDialogue();
        audioManager.PlaySound(radioSound);
    }
    public void BossRoomMusic()
    {
        musicManager.StartBGM(dialogueTheme);
    }
    public void ShowTooltip(string sentence)
    {
        tooltipParent.DOScaleY(1f, tweenSpeed);
        tooltipText.SetText(sentence);
    }
    public void HideTooltip()
    {
        tooltipParent.DOScaleY(0f, tweenSpeed);
        tooltipText.SetText(string.Empty);
    }
}
