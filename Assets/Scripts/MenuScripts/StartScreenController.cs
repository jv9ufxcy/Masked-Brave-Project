using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StartScreenController : MonoBehaviour
{
    private bool isMainMenuActive=false;
    public GameObject startPanel, menuPanel, optionsPanel;
    private int value;
    public EventSystem eventSystem;
    public GameObject selectedObject;
    private AudioManager audioManager;
    private string uiSelectSound = "UI/Select", uiCancelSound = "UI/Cancel", uiCursorSound = "UI/Move Cursor";
    // Start is called before the first frame update
    void Start()
    {
        //ApplyVideoOptions();
        audioManager = AudioManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isMainMenuActive)
        {
            if (Input.GetButtonDown("Ps4Options"))
            {
                value = 0;
                GlobalVars.instance.PassControllerValue(0);
                Debug.Log("PS4 Start");
                ActivateMenu();
                PlaySelectSound(0);
            }
            else if (Input.GetButtonDown("XboxMenu"))
            {
                value = 1;
                GlobalVars.instance.PassControllerValue(1);
                Debug.Log("Xbox Start");
                ActivateMenu();
                PlaySelectSound(0);
            }
            else if (Input.GetKey(KeyCode.Return))
            {
                value = 2;
                GlobalVars.instance.PassControllerValue(2);
                Debug.Log("PC Start");
                ActivateMenu();
                PlaySelectSound(0);
            }
        }
        else
        {
            if (Input.GetButtonDown("Cancel")/*eventSystem.alreadySelecting == false*/)
            {
                if (!menuPanel.activeSelf)
                {
                    ActivateMenu();
                    PlaySelectSound(2);
                }
                    eventSystem.SetSelectedGameObject(selectedObject);
                //eventSystem.SetSelectedGameObject(selectedObject);
                //buttonSelected = true;
            }
        }
        
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
    void ActivateMenu()
    {
        
        startPanel.SetActive(false);
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
        isMainMenuActive = true;
        GlobalVars.instance.PassControllerValue(value);
        eventSystem.SetSelectedGameObject(selectedObject);
    }
    public void LoadByIndex(string sceneToLoad)
    {
        PlaySelectSound(0);
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
        ActivateMenu();
    }
}
