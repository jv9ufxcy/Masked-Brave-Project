using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public long lastUpdated;
    public int healthBoost;
    public int levelUnlocked;
    public List<string> unlockedSkillsData;
    public List<string> upgradesCollected;
    public List<string> energyTanksCollected;
    
    //initial data
    public GameData()
    {
        this.healthBoost = 0;
        this.levelUnlocked = 0;
        unlockedSkillsData = new List<string>();
        upgradesCollected = new List<string>();
        energyTanksCollected = new List<string>();
    }
    public int GetPercentageUnlocked()
    {
        int totalUnlocked = 0;
        foreach (string collected in upgradesCollected)
        {
            totalUnlocked++;
        }

        int percentUnlocked = -1;
        if (upgradesCollected.Count!=0)
        {
            percentUnlocked = totalUnlocked * 100/ upgradesCollected.Count;
        }
        return percentUnlocked;
    }
}
