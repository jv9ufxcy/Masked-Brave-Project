using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class SkillListItem : MonoBehaviour, ISelectHandler
{
    private SkillListMenu skillMenu;
    [SerializeField] private string skillDescription, controlDesc;
    [SerializeField] private string psControls, xboxControls, keyboardControls;
    private TextMeshProUGUI description;
    // Start is called before the first frame update
    void Start()
    {
        skillMenu = GetComponentInParent<SkillListMenu>();

        switch (GameEngine.coreData.currentControllerIndex)
        {
            case 0://ps4
                controlDesc = psControls;
                break;
            case 1://xbox
                controlDesc = xboxControls;
                break;
            case 2://kb
                controlDesc = keyboardControls;
                break;
            default:
                break;
        }
    }
    public void OnSelect(BaseEventData eventData)
    {
        skillMenu = GetComponentInParent<SkillListMenu>();
        skillMenu.UpdateDescriptionAreaText(skillDescription, controlDesc);
    }
}
