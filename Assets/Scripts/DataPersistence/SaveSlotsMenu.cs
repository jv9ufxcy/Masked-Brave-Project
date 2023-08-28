using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotsMenu : MonoBehaviour
{
    private SaveSlot[] saveSlots;
    private bool isLoadingGame = false;
    private void Awake()
    {
        saveSlots=this.GetComponentsInChildren<SaveSlot>();
    }
    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
        if(!isLoadingGame)
            DataPersistenceManager.instance.NewGame();
    }
    public void OnDeleteDataClicked(SaveSlot saveSlot)
    {
        DataPersistenceManager.instance.DeleteProfileData(saveSlot.GetProfileId());
        ActivateSaveMenu(isLoadingGame);
    }
    public void ActivateSaveMenu(bool isLoadingGame)
    {
        this.gameObject.SetActive(true);
        this.isLoadingGame = isLoadingGame;
        Dictionary<string, GameData> profilesGameData=DataPersistenceManager.instance.GetAllProfilesGameData();
        foreach (SaveSlot saveSlot in saveSlots)
        {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileId(), out profileData);
            saveSlot.SetData(profileData);
            if (profileData==null&&isLoadingGame)
            {
                saveSlot.gameObject.GetComponent<Button>().interactable = false;
            }
            else
            {
                saveSlot.gameObject.GetComponent<Button>().interactable = true;
            }
        }
    }
}
