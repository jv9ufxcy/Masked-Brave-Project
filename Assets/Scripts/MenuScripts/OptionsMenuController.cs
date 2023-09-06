using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OptionsMenuController : MonoBehaviour
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
    private int resolutionIndex=3;
    private int chosenWidth, chosenHeight;
    private bool isFullScreen,visibleHealth,isVSync;
    public void ApplyVideoOptions()
    {
        GlobalVars.LoadOptions();

        isFullScreen = (Screen.fullScreen);
        fullScreenToggle.isOn = isFullScreen;
        vSyncToggle.isOn = isVSync;
        if (isVSync)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;
        hpToggle.isOn = visibleHealth;
        resolutionIndex = 3;
        chosenWidth = resolutionWidths[resolutionIndex];
        chosenHeight = resolutionHeights[resolutionIndex];
    }
    public void ToggleFullScreen()
    {
        isFullScreen = fullScreenToggle.isOn;
        PlaySelectSound(0);
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
        GlobalVars.SaveOptions(isFullScreen, chosenWidth, chosenHeight, 1, 1, visibleHealth, isVSync);
        Screen.SetResolution(chosenWidth, chosenHeight, isFullScreen);
        PlaySelectSound(0);
        //ReturnToLevelSelectMenu();
    }
}
