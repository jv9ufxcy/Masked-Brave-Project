using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchActive : MonoBehaviour
{
    [SerializeField] GameObject defaultAvatar, alternateAvatar;
    int whichAvatarIsOn = 1;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip transformingSound;

    // Use this for initialization
    void Start()
    {
        audioSource.GetComponent<AudioSource>();
        defaultAvatar.gameObject.SetActive(true);
        alternateAvatar.gameObject.SetActive(false);
        
    }
    public void SwitchAvatar()
    {
        if (Input.GetButtonDown("Henshin"))
        {
            whichAvatarIsOn = whichAvatarIsOn == 1 ? 2 : 1;
            audioSource.clip = transformingSound;
            audioSource.Play();
        }
        defaultAvatar.gameObject.SetActive(whichAvatarIsOn == 1);
        alternateAvatar.gameObject.SetActive(whichAvatarIsOn == 2);
    }
    private void Update()
    {
        SwitchAvatar();
    }
}