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
    // Start is called before the first frame update
    void Start()
    {
        //ApplyVideoOptions();
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
            }
            else if (Input.GetButtonDown("XboxMenu"))
            {
                value = 1;
                GlobalVars.instance.PassControllerValue(1);
                Debug.Log("Xbox Start");
                ActivateMenu();
            }
            else if (Input.GetKey(KeyCode.Return))
            {
                value = 2;
                GlobalVars.instance.PassControllerValue(2);
                Debug.Log("PC Start");
                ActivateMenu();
            }
        }
        else
        {
            if (Input.GetButton("Cancel") && eventSystem.alreadySelecting == false)
            {
                ActivateMenu();
                eventSystem.SetSelectedGameObject(selectedObject);
                //buttonSelected = true;
            }
        }
    }
    void ActivateMenu()
    {
        startPanel.SetActive(false);
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
        isMainMenuActive = true;
        GlobalVars.instance.PassControllerValue(value);
    }
    public void LoadByIndex(string sceneToLoad)
    {
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
    [SerializeField] private Toggle toggle;
    [SerializeField] private List<int> resolutionWidths;
    [SerializeField] private List<int> resolutionHeights;
    private int resolutionIndex=3;
    private int chosenWidth, chosenHeight;
    private bool isFullScreen;
    public void ApplyVideoOptions()
    {
        GlobalVars.LoadOptions();

        isFullScreen = (Screen.fullScreen);
        toggle.isOn = isFullScreen;
        resolutionIndex = 3;
        chosenWidth = resolutionWidths[resolutionIndex];
        chosenHeight = resolutionHeights[resolutionIndex];
    }
    public void ToggleFullScreen()
    {
        isFullScreen = toggle.isOn;
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
        
    }
    public void Apply()
    {
        GlobalVars.SaveOptions(isFullScreen, chosenWidth, chosenHeight, 1, 1);
        Screen.SetResolution(chosenWidth, chosenHeight, isFullScreen);
    }
}
