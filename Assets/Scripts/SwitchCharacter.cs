using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCharacter : MonoBehaviour
{
    [SerializeField] private CharacterState characterState;
    [SerializeField] private Player thePlayer;
    [SerializeField] private PlayerDashing airDash;
    [SerializeField] private PlayerAttacking swordAttack;
    [SerializeField] private PlayerAttacking riderPunch;
    [SerializeField] GameObject largeAvatar, smallAvatar;
    int whichAvatarIsOn = 1;
    
    [SerializeField] private AudioManager audioManager;

    public bool IsLarge()
    {
        return characterState == CharacterState.Large;
    }
    // Use this for initialization
    void Start ()
    {
        largeAvatar.gameObject.SetActive(true);
        smallAvatar.gameObject.SetActive(false);

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void SwitchAvatarState()
    {
        switch(characterState)
        {
            case CharacterState.Large:
                var isTransformKeyLarge = (Input.GetButtonDown("Henshin"));
                if (isTransformKeyLarge)
                {
                    characterState = CharacterState.Small;
                }
                break;
            case CharacterState.Small:
                var isTransformKeySmall = (Input.GetButtonDown("Henshin"));
                if (isTransformKeySmall)
                {
                    characterState = CharacterState.Large;
                }
                break;
        }
    }
	public void SwitchAvatar()
    {
        if (thePlayer.ShouldAct)
        {
            if (Input.GetButtonDown("Henshin"))
            {
                whichAvatarIsOn = whichAvatarIsOn == 1 ? 2 : 1;
                audioManager.PlaySound("Henshin");
            }
                
            largeAvatar.gameObject.SetActive(whichAvatarIsOn == 1);
            smallAvatar.gameObject.SetActive(whichAvatarIsOn == 2);
        }
    }
    private void Update()
    {
        SwitchAvatar();
        if (thePlayer.ShouldAct)
            SwitchAvatarState();
    }
    public enum CharacterState
    {
        Large,
        Small
    }
}
