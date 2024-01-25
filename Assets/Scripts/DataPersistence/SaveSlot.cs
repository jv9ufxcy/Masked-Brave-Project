using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private string profileId = "";
    [Header("Content")]
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;

    [SerializeField] private TextMeshProUGUI percentageCompletetext;
    [Header("Clear Data")]
    [SerializeField] private Button clearButton;
    public void SetData(GameData data)
    {
        if (data==null)
        {
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
            clearButton.gameObject.SetActive(false);
        }
        else
        {
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            //Set Content
            percentageCompletetext.text = data.GetPercentageUnlocked() + "%";
            clearButton.gameObject.SetActive(true);
        }
    }
    public string GetProfileId()
    {
        return profileId;
    }
}
