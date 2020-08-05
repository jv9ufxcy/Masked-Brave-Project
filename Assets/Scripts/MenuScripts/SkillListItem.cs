using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillListItem : MonoBehaviour
{
    private SkillListMenu skillMenu;
    [SerializeField] private string skillDescription;
    private TextMeshProUGUI description;
    // Start is called before the first frame update
    void Start()
    {
        skillMenu = GetComponentInParent<SkillListMenu>();
    }

    public void OnValueChanged()
    {
        skillMenu.UpdateDescriptionAreaText(skillDescription);
    }
}
