using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


[System.Serializable]
public class Save
{
    public float SfxMultiplier, MusicMultiplier;
    public bool fullscreen, hpBars;
    public int resWidth, resHeight;

    public Save CreateSaveGameObject(bool fullscreen, int resWidth, int resHeight, float musicMultiplier, float sfxMultiplier, bool visibleHP)
    {
        Save save = new Save();
        save.fullscreen = fullscreen;
        save.hpBars = visibleHP;
        save.resWidth = resWidth;
        save.resHeight = resHeight;
        save.MusicMultiplier = musicMultiplier;
        save.SfxMultiplier = sfxMultiplier;

        return save;
    }

    public void SaveOptions(Save saveInstance)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/options.save");
        bf.Serialize(file, saveInstance);//save the options to a file
        file.Close();
    }

    public void LoadOptions()
    {
        if (File.Exists(Application.persistentDataPath + "/options.save"))//if there is saved options, open them
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/options.save", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();

            //set those options
            fullscreen = save.fullscreen;
            resWidth = save.resWidth;
            resHeight = save.resHeight;
            SfxMultiplier = save.SfxMultiplier;
            MusicMultiplier = save.MusicMultiplier;
        }
        else//if no saved options, set those to default
        {
            fullscreen = true;
            resWidth = 1920;
            resHeight = 1080;
            SfxMultiplier = 1;
            MusicMultiplier = 1;
        }

        Screen.SetResolution(resWidth, resHeight, this.fullscreen);//apply loaded changes
        //AudioManager.instance.SetMultiplier(SfxMultiplier);
    }
}
public class GlobalVars : MonoBehaviour
{
    public enum ControllerState { ps4, xbox, keyboard}
    public static ControllerState _controllerState;
    public static int controllerNumber;
    public static bool visibleEnemyHealth=false;
    public static GlobalVars instance;

    public static Save save = new Save();//empty save instance.

    public static void SaveOptions(bool fullscreen, int resWidth, int resHeight, float musicMultiplier, float sfxMultiplier, bool visibleHP)
    {
        visibleEnemyHealth = visibleHP;
        save = save.CreateSaveGameObject(fullscreen, resWidth, resHeight, musicMultiplier, sfxMultiplier, visibleHP);
        save.SaveOptions(save);
    }

    public static void LoadOptions()
    {
        save.LoadOptions();
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(gameObject);
    }
    public void PassControllerValue(int dropdownValue)
    {
        controllerNumber = dropdownValue;
        GameEngine.coreData.currentControllerIndex = controllerNumber;
        switch (controllerNumber)
        {
            case 0:
                GameEngine.coreData.rawInputs = GameEngine.coreData.ps4Inputs;
                break;
            case 1:
                GameEngine.coreData.rawInputs = GameEngine.coreData.xboxInputs;
                break;
            case 2:
                GameEngine.coreData.rawInputs = GameEngine.coreData.keyboardInputs;
                break;
            default:
                break;
        }
        
    }
}
