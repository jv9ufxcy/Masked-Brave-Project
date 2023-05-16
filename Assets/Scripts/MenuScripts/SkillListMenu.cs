using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillListMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionAreaText, controlText;

    public void UpdateDescriptionAreaText(string descriptionText, string controls)
    {
        descriptionAreaText.SetText(descriptionText);
        controlText.SetText(controls);
    }
    
}
