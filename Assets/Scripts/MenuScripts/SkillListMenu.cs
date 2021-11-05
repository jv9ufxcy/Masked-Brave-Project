using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillListMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionAreaText, controlText;
    private void Start()
    {
        
    }
    private void Update()
    {
    }
    public void UpdateDescriptionAreaText(string descriptionText, string controls)
    {
        descriptionAreaText.text = descriptionText;
        controlText.text = controls;
    }
    
}
