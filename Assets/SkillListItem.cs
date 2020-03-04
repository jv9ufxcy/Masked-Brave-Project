using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillListItem : MonoBehaviour
{
    private SkillListMenu skillMenu;
    [SerializeField] private string skillDescription;
    // Start is called before the first frame update
    void Start()
    {
        skillMenu = GetComponentInParent<SkillListMenu>();
    }

    public void OnValueCHanged()
    {
        skillMenu.UpdateDescriptionAreaText(skillDescription);
    }
}
