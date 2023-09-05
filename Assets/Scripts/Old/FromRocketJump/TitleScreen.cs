using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class TitleScreen : MonoBehaviour,IDataPersistence
{
    [SerializeField] private Button[] levelButtons;
    private int savedLevelUnlockIndex;
    private void Start()
    {
        //Cursor.visible = false;
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (i > savedLevelUnlockIndex)
                levelButtons[i].interactable = false;
        }
    }
    public void LoadByIndex(string sceneToLoad)
    {
        SceneTransitionController.instance.LoadScene(sceneToLoad);
        //SceneManager.LoadScene(sceneIndex);
    }

    public void LoadData(GameData data)
    {
        savedLevelUnlockIndex = data.levelUnlocked;
    }

    public void SaveData(GameData data)
    {
        //throw new System.NotImplementedException();
    }
}