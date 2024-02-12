using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelScoreVisual : MonoBehaviour,IDataPersistence
{
    [SerializeField] private int[] missionScores;
    [SerializeField] private string[] missionGrades;
    [SerializeField] private TextMeshProUGUI[] levelScoreText, missionGradeText;
    public void LoadData(GameData data)
    {
        missionScores = data.missionScoreIndex;
        missionGrades = data.missionGrade;
        SetLevelScores();
    }

    private void SetLevelScores()
    {
        for (int i = 0; i < levelScoreText.Length; i++)
        {
            levelScoreText[i].SetText(missionScores[i].ToString());
            missionGradeText[i].SetText(missionGrades[i].ToString());
        }
    }

    public void SaveData(GameData data)
    {
        //throw new System.NotImplementedException();
    }

}
