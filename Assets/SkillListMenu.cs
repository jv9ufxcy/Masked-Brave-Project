using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillListMenu : MonoBehaviour
{
    [SerializeField] private Text descriptionAreaText;
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
