using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockLevel : MonoBehaviour, IDataPersistence
{
    private int unlockedLevels = 0;

    public void LoadData(GameData data)
    {
        this.unlockedLevels = data.levelUnlocked;
    }

    public void SaveData(GameData data)
    {
        data.levelUnlocked = this.unlockedLevels;
        Debug.Log("Levels Unlocked: " + data.levelUnlocked);
    }

    public void UnlockLevels(int level)
    {
        unlockedLevels = level;
        this.transform.SetParent(null);//removes parent from mission child 
    }
}
