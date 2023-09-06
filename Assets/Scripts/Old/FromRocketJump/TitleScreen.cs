using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class TitleScreen : MonoBehaviour,IDataPersistence
{
    private bool areControlsSet = false;
    private int value;
    public EventSystem eventSystem;
    public GameObject selectedObject;
    private AudioManager audioManager;
    private string uiSelectSound = "UI/Select", uiCancelSound = "UI/Cancel", uiCursorSound = "UI/Move Cursor";
    [SerializeField] Button newGameButton, loadGameButton, continueGameButton;
    [SerializeField] GameObject startPanel,optionsPanel, dataPanel;
    [SerializeField] SaveSlotsMenu saveMenu;
    private void Start()
    {
        audioManager = AudioManager.instance;
        DisableButtonsWithoutData();
    }
    private void Update()
    {
        if (!areControlsSet)
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
                else if (optionsPanel.activeSelf)//level select menu open, return to data panel
                {
                    OptionsMenuToDataMenu();
                }
            }
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

    public void LoadData(GameData data)
    {
        
    }

    public void SaveData(GameData data)
    {
        //throw new System.NotImplementedException();
    }
    IEnumerator SetSelectedButton()
    {
        eventSystem.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        eventSystem.SetSelectedGameObject(selectedObject);
    }
    private void DisableButtonsWithoutData()
    {
        if (!DataPersistenceManager.instance.HasGameData())
        {
            loadGameButton.interactable = false;
            continueGameButton.interactable = false;
        }
    }
    public void SaveMenuDisable()
    {
        saveMenu.gameObject.SetActive(false);
        //go to level select after load game.
    }
    public void OptionsMenuToDataMenu()
    {
        optionsPanel.SetActive(false);
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
        SaveMenuDisable();
    }
}