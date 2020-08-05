using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillListMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionAreaText;
    private void Start()
    {
        
    }
    private void Update()
    {
    }
    public void UpdateDescriptionAreaText(string descriptionText)
    {
        descriptionAreaText.text = descriptionText;
    }
    
}
