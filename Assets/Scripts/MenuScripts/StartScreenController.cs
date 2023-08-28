using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StartScreenController : MonoBehaviour
{
    private bool areControlsSet=false;
    public GameObject startPanel, levelSelectMenuPanel, optionsPanel, dataPanel;
    private int value;
    public EventSystem eventSystem;
    public GameObject selectedObject;
    private AudioManager audioManager;
    private string uiSelectSound = "UI/Select", uiCancelSound = "UI/Cancel", uiCursorSound = "UI/Move Cursor";
    [SerializeField] Button newGameButton,loadGameButton, continueGameButton;
    [SerializeField] SaveSlotsMenu saveMenu;
    // Start is called before the first frame update
    void Start()
    {
        DisableButtonsWithoutData();
        //ApplyVideoOptions();
        audioManager = AudioManager.instance;
    }

    private void DisableButtonsWithoutData()
    {
        if (!DataPersistenceManager.instance.HasGameData())
        {
            loadGameButton.interactable = false;
            continueGameButton.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!areControlsSet)
        {
            if (Input.GetButtonDown("Ps4Options"))
            {
                value = 0;
                GlobalVars.instance.PassControllerValue(0);
                Debug.Log("PS4 Start");
                ActivateDataMenu();
                PlaySelectSound(0);
            }
            else if (Input.GetButtonDown("XboxMenu"))
            {
                value = 1;
                GlobalVars.instance.PassControllerValue(1);
                Debug.Log("Xbox Start");
                ActivateDataMenu();
                PlaySelectSound(0);
            }
            else if (Input.GetKey(KeyCode.Return))
            {
                value = 2;
                GlobalVars.instance.PassControllerValue(2);
                Debug.Log("PC Start");
                ActivateDataMenu();
                PlaySelectSound(0);
            }
        }
        else
        {
            if (Input.GetButtonDown("Cancel"))
            {
                PlaySelectSound(2);
                if (dataPanel.activeSelf)//earliest nest, reselect button
                {
                    selectedObject = dataPanel.GetComponentInChildren<Button>().gameObject;
                    StartCoroutine(SetSelectedButton());
                }
                else if (saveMenu.gameObject.activeSelf)//save slot menu open, return to data panel
                {
                    SaveMenuToDataMenu();
                }
                else if (levelSelectMenuPanel.activeSelf)//level select menu open, return to data panel
                {
                    LevelSelectMenuToDataMenu();
                }
                else if (optionsPanel.activeSelf)//options open, return to level select menu
                {
                    ReturnToLevelSelectMenu();
                }
            }
        }
        
    }

    public void SaveMenuToLevelSelectMenu()
    {
        saveMenu.gameObject.SetActive(false);
        levelSelectMenuPanel.SetActive(true);
        selectedObject = levelSelectMenuPanel.GetComponentInChildren<Button>().gameObject;
        StartCoroutine(SetSelectedButton());
    }
    public void LevelSelectMenuToDataMenu()
    {
        levelSelectMenuPanel.SetActive(false);
        dataPanel.SetActive(true);
        selectedObject = dataPanel.GetComponentInChildren<Button>().gameObject;
        StartCoroutine(SetSelectedButton());

        DisableButtonsWithoutData();
    }
    public void SaveMenuToDataMenu()
    {
        saveMenu.gameObject.SetActive(false);
        dataPanel.SetActive(true);
        selectedObject = dataPanel.GetComponentInChildren<Button>().gameObject;
        StartCoroutine(SetSelectedButton());

        DisableButtonsWithoutData();
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
    void ActivateDataMenu()//set controls and activate start data menu
    {
        startPanel.SetActive(false);
        GlobalVars.instance.PassControllerValue(value);
        areControlsSet = true;
        dataPanel.SetActive(true);
        selectedObject = dataPanel.GetComponentInChildren<Button>().gameObject;
        eventSystem.SetSelectedGameObject(selectedObject);
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
    [Header("Video Settings")]
    [SerializeField] private TextMeshProUGUI resDisplayText;
    [SerializeField] private Toggle fullScreenToggle, hpToggle;
    [SerializeField] private List<int> resolutionWidths;
    [SerializeField] private List<int> resolutionHeights;
    private int resolutionIndex=3;
    private int chosenWidth, chosenHeight;
    private bool isFullScreen,visibleHealth;
    public void ApplyVideoOptions()
    {
        GlobalVars.LoadOptions();

        isFullScreen = (Screen.fullScreen);
        fullScreenToggle.isOn = isFullScreen;
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
        GlobalVars.SaveOptions(isFullScreen, chosenWidth, chosenHeight, 1, 1, visibleHealth);
        Screen.SetResolution(chosenWidth, chosenHeight, isFullScreen);
        PlaySelectSound(0);
        ReturnToLevelSelectMenu();
    }
    public void OnNewGameClicked()
    {
        //DataPersistenceManager.instance.NewGame();
        saveMenu.ActivateSaveMenu(false);
        dataPanel.SetActive(false);
        selectedObject = saveMenu.gameObject.GetComponentInChildren<Button>().gameObject;
        eventSystem.SetSelectedGameObject(selectedObject);
    }
    public void OnLoadGameClicked()
    {
        saveMenu.ActivateSaveMenu(true);
        dataPanel.SetActive(false);
        selectedObject = saveMenu.gameObject.GetComponentInChildren<Button>().gameObject;
        eventSystem.SetSelectedGameObject(selectedObject);
    }
    public void OnContinueGameClicked()
    {
        dataPanel.SetActive(false);
        SaveMenuToLevelSelectMenu();
    }
    IEnumerator SetSelectedButton()
    {
        eventSystem.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        eventSystem.SetSelectedGameObject(selectedObject);
    }
}
