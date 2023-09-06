using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public long lastUpdated;
    public int deathCount;
    public int healthBoost;
    public int levelUnlocked;
    public List<string> unlockedSkillsData;
    public SerializableDictionary<string, bool> upgradesCollected;
    
    //initial data
    public GameData()
    {
        this.deathCount = 0;
        this.healthBoost = 0;
        this.levelUnlocked = 0;
        this.unlockedSkillsData = new List<string>();
        this.upgradesCollected = new SerializableDictionary<string, bool>();
    }
    public int GetPercentageUnlocked()
    {
        int totalUnlocked = 0;
        foreach (string form in unlockedSkillsData)
        {
            totalUnlocked++;
        }
        //foreach (bool collected in upgradesCollected.Values)
        //{
        //    if (collected)
        //    {
        //        totalUnlocked++;
        //    }
        //}
        int percentUnlocked = -1;
        if (upgradesCollected.Count!=0)
        {
            percentUnlocked = totalUnlocked * 100/ 5;
        }
        return percentUnlocked;
    }
}
