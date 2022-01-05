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
        //studioEventEmitter.EventReference = path;
        BGM = FMODUnity.RuntimeManager.CreateInstance(path);
        BGM.start();
        BGM.release();

        //Debug.Log(path);
        //FMODUnity.RuntimeManager.PlayOneShot()
    }

    public void StartMusic()
    {
        //studioEventEmitter.Play();
        //Debug.Log(studioEventEmitter.EventReference);
        
        //BackgroundMusic.Play();
    }

    public void StopMusic()
    {
        //studioEventEmitter.Stop();
        BGM.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        //BackgroundMusic.Stop();
    }
    public void StopReverbZone()
    {
        studioEventEmitter.enabled = false;
    }
}
