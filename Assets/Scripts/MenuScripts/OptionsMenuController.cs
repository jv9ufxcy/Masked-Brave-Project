using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OptionsMenuController : MonoBehaviour,IDataPersistence
{
    public EventSystem eventSystem;
    private AudioManager audioManager;
    private string uiSelectSound = "UI/Select", uiCancelSound = "UI/Cancel", uiCursorSound = "UI/Move Cursor";
    
    // Start is called before the first frame update
    void Start()
    {
        audioManager = AudioManager.instance;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PlaySelectSound(int sound)
    {
        if (audioManager!=null)
        {
            switch (sound)
            {
                case 0:
                    audioManager.PlaySound(uiSelectSound);
                    break;
                case 1:
                    audioManager.PlaySound(uiCancelSound);
                    break;
                case 2:
                    audioManager.PlaySound(uiCursorSound);
                    break;
            }
        }
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    [Header("Video Settings")]
    [SerializeField] private TextMeshProUGUI resDisplayText;
    [SerializeField] private Toggle fullScreenToggle,vSyncToggle, hpToggle;
    [SerializeField] private List<int> resolutionWidths;
    [SerializeField] private List<int> resolutionHeights;
    [SerializeField] private ResolutionController resolutionCon;
    private int resolutionIndex=3;
    private int chosenWidth, chosenHeight;
    private bool isFullScreen,visibleHealth,isVSync;
    public void ApplyVideoOptions()
    {
        //GlobalVars.LoadOptions();

        isFullScreen = (Screen.fullScreen);
        fullScreenToggle.isOn = isFullScreen;
        //resolutionCon.DefaultSliderPos(resolutionIndex);
        //vSyncToggle.isOn = isVSync;
        //if (isVSync)
        //    QualitySettings.vSyncCount = 1;
        //else
        //    QualitySettings.vSyncCount = 0;
        hpToggle.isOn = visibleHealth;
        resolutionIndex = 3;
        chosenWidth = resolutionWidths[resolutionIndex];
        chosenHeight = resolutionHeights[resolutionIndex];
    }
    public void ToggleFullScreen()
    {
        isFullScreen = fullScreenToggle.isOn;
        PlaySelectSound(0);
        //if (!isFullScreen&&isVSync)
        //{
        //    ToggleVSync();
        //}
    }
    public void ToggleVSync()
    {
        isVSync = vSyncToggle.isOn;
        PlaySelectSound(0);
    }
    public void ToggleHP()
    {
        visibleHealth = hpToggle.isOn;
        PlaySelectSound(0);
    }
    private void DisplayChosenRes()
    {
        resDisplayText.text = chosenWidth + " x " + chosenHeight;
    }
    public void SetResolution(int index)
    {
        resolutionIndex = index;
        chosenWidth = resolutionWidths[resolutionIndex];
        chosenHeight = resolutionHeights[resolutionIndex];

        DisplayChosenRes();//show what was set with the text
        PlaySelectSound(2);
    }
    public void Apply()
    {
        //GlobalVars.SaveOptions(isFullScreen, chosenWidth, chosenHeight, 1, 1, visibleHealth, isVSync);
        Screen.SetResolution(chosenWidth, chosenHeight, isFullScreen);
        for (int i = 0; i < audioControllers.Length; i++)
        {
            volumeMaster[i] = (audioControllers[i].vcaVolume * 10);
        }
        PlaySelectSound(0);
        //ReturnToLevelSelectMenu();
    }
    [Tooltip("Master, Music, SFX")]
    [SerializeField] private VCAController[] audioControllers;
    private float[] volumeMaster=new float[3];
    public void SetAudioLevels()
    {
        for (int i = 0; i < audioControllers.Length; i++)
        {
            audioControllers[i].SetSlider(volumeMaster[i]);
        }
    }

    public void LoadData(GameData data)
    {
        this.resolutionIndex = data.resolutionIndex;
        this.isFullScreen = data.isFullScreen;
        this.isVSync = data.isVSync;
        this.visibleHealth = data.visibleHealth;
        ApplyVideoOptions();
        this.volumeMaster = data.masterVolume;
        SetAudioLevels();
    }

    public void SaveData(GameData data)
    {
        data.resolutionIndex = this.resolutionIndex;

        data.isFullScreen = this.isFullScreen;
        data.isVSync = this.isVSync;
        data.visibleHealth = this.visibleHealth;

        data.masterVolume = this.volumeMaster;
    }
}
