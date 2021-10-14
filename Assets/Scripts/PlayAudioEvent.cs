using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioEvent : MonoBehaviour
{
    private AudioManager audioManager;
    private void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    public void PlayFootstepAudio(string path)
    {
        //event:/Sound Effects/Player/Footstep
        FMOD.Studio.EventInstance Footsteps = FMODUnity.RuntimeManager.CreateInstance(path);
        Footsteps.start();
        Footsteps.release();
            
    }
    public void PlayAudio(string audioName)
    {
        audioManager.PlaySound(audioName);
    }
}
