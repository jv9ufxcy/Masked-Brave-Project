using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public long lastUpdated;
    public int[] missionScoreIndex;
    public string[] missionGrade;
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
        missionScoreIndex = new int[8];
        missionGrade = new string[8];
        unlockedSkillsData = new List<string>();
        upgradesCollected = new List<string>();
        energyTanksCollected = new List<string>();
    }
    public int GetPercentageUnlocked()
    {
        int totalUnlocked = 0;
        foreach (int beatenLevel in missionScoreIndex)
        {
            if (beatenLevel>0)//if score exists at all
            {
                totalUnlocked++;
            }
        }

        int percentUnlocked = -1;
        if (upgradesCollected.Count!=0)
        {
            percentUnlocked = totalUnlocked * 100/ missionScoreIndex.Length;
        }
        return percentUnlocked;
    }
}
