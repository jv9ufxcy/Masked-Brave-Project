using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnImageActive : MonoBehaviour
{
    public GlobalVars _globalVars;
    public bool UpSlashActive, DownSlashActive, isImageActive;
    [SerializeField]private Animator slashAnim;
    [SerializeField] private SpriteRenderer slashImage, buttonPrompt;
    [SerializeField] private Sprite buttonPromptPS, buttonPromptXB, buttonPromptPC;
    // Start is called before the first frame update
    void Start()
    {
        _globalVars = GameObject.FindGameObjectWithTag("GV").GetComponent<GlobalVars>();
        slashImage = GetComponent<SpriteRenderer>();
        //buttonPrompt = GetComponentInChildren<SpriteRenderer>();
        slashAnim = GetComponent<Animator>();

        slashImage.color = Color.clear;
        buttonPrompt.color = Color.clear;
    }

    // Update is called once per frame
    void Update()
    {
        switch (GlobalVars.controllerNumber)
        {
            case 0:
                buttonPrompt.sprite = buttonPromptPS;//ControllerState.ps4
                break;
            case 1:
                buttonPrompt.sprite = buttonPromptXB;//ControllerState.xbox
                break;
            case 2:
                buttonPrompt.sprite = buttonPromptPC;//ControllerState.keyboard
                break;
            default:
                buttonPrompt.sprite = buttonPromptPS;//ControllerState.ps4
                break;
        }
        UpdateAnimBools();
    }

    private void UpdateAnimBools()
    {
        slashAnim.SetBool("UpSlash", UpSlashActive);
        slashAnim.SetBool("DownSlash", DownSlashActive);
        slashAnim.SetBool("AnySlash", isImageActive);
    }

    public void ActivateUpSlash()
    {
        slashImage.color = Color.white;
        buttonPrompt.color = Color.white;
        UpSlashActive = true;
        DownSlashActive = false;
        isImageActive = true;
    }
    public void ActivateDownSlash()
    {
        slashImage.color = Color.white;
        buttonPrompt.color = Color.white;
        DownSlashActive = true;
        UpSlashActive = false;
        isImageActive = true;
    }
    public void StopSlash()
    {
        StartCoroutine(Disappear());
    }
    private IEnumerator Disappear()
    {
        isImageActive = false;
        DownSlashActive = false;
        UpSlashActive = false;
        yield return new WaitForSeconds(0.2f);
        slashImage.color = Color.clear;
        buttonPrompt.color = Color.clear;
    }
}
