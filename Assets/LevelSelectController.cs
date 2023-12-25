using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour, IDataPersistence
{
    public GameObject levelSelectMenuPanel, optionsPanel;
    public EventSystem eventSystem;
    [SerializeField] StandaloneInputModule inputModule;
    public GameObject selectedObject;
    private AudioManager audioManager;
    private string uiSelectSound = "UI/Select", uiCancelSound = "UI/Cancel", uiCursorSound = "UI/Move Cursor";

    private int savedLevelUnlockIndex;
    [SerializeField] private Button[] levelButtons;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (i > savedLevelUnlockIndex)
                levelButtons[i].interactable = false;
        }
        Debug.Log("Unlocked "+ savedLevelUnlockIndex+" Levels");
        audioManager = AudioManager.instance;
        SetMenuInput();
    }
    private void SetMenuInput()
    {
        inputModule.horizontalAxis = GameEngine.coreData.rawInputs[13].name;
        inputModule.verticalAxis = GameEngine.coreData.rawInputs[14].name;
        inputModule.submitButton = GameEngine.coreData.rawInputs[0].name;
        inputModule.cancelButton = GameEngine.coreData.rawInputs[2].name;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            PlaySelectSound(2);
            if (levelSelectMenuPanel.activeSelf)//earliest nest, reselect button
            {
                selectedObject = levelSelectMenuPanel.GetComponentInChildren<Button>().gameObject;
                StartCoroutine(SetSelectedButton());
            }
            else if (optionsPanel.activeSelf)//options open, return to level select menu
            {
                ReturnToLevelSelectMenu();
            }
        }
    }

    public void ReturnToLevelSelectMenu()
    {
        optionsPanel.SetActive(false);
        levelSelectMenuPanel.SetActive(true);
        selectedObject = levelSelectMenuPanel.GetComponentInChildren<Button>().gameObject;
        StartCoroutine(SetSelectedButton());
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
    public void LoadByIndex(string sceneToLoad)
    {
        PlaySelectSound(0);
        DataPersistenceManager.instance.SaveGame();
        SceneTransitionController.instance.LoadScene(sceneToLoad);
        //SceneManager.LoadScene(sceneIndex);
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    IEnumerator SetSelectedButton()
    {
        eventSystem.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        eventSystem.SetSelectedGameObject(selectedObject);
    }

    public void LoadData(GameData data)
    {
        savedLevelUnlockIndex = data.levelUnlocked;
    }

    public void SaveData(GameData data)
    {
        //throw new System.NotImplementedException();
    }

    [Header("Video Settings")]
    [SerializeField] private TextMeshProUGUI resDisplayText;
    [SerializeField] private Toggle fullScreenToggle, vSyncToggle, hpToggle;
    [SerializeField] private List<int> resolutionWidths;
    [SerializeField] private List<int> resolutionHeights;
    private int resolutionIndex = 3;
    private int chosenWidth, chosenHeight;
    private bool isFullScreen, visibleHealth, isVSync;
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
        ReturnToLevelSelectMenu();
    }

    
}
