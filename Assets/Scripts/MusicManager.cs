using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    private StudioEventEmitter studioEventEmitter;
    FMOD.Studio.EventInstance BGM;
    //[FMODUnity.EventRef(MigrateTo ="<fieldname>")]
    public EventReference stageTheme;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        studioEventEmitter = GetComponent<StudioEventEmitter>();
        //BackgroundMusic = GetComponent<AudioSource>();
    }
    private void FixedUpdate()
    {
        //if (PauseManager.IsGamePaused)
        //    BackgroundMusic.volume = 0.1f;
        //else
        //    BackgroundMusic.volume = 0.5f;
    }
    public void SetStageTheme(EventReference path)
    {
        stageTheme = path;
    }
    public void StartBGM(EventReference path)
    {
        StopMusic();
        //BackgroundMusic.clip = music;
        studioEventEmitter.EventReference = path;
        //BGM = FMODUnity.RuntimeManager.CreateInstance(path);
        StartMusic();

        //FMODUnity.RuntimeManager.PlayOneShot()
    }

    public void StartMusic()
    {
        studioEventEmitter.Play();
        //BGM.start();
        //BackgroundMusic.Play();
    }

    public void StopMusic()
    {
        studioEventEmitter.Stop();
        //BGM.release();
        //BackgroundMusic.Stop();
    }
}
