using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class SkillListItem : MonoBehaviour, ISelectHandler
{
    private SkillListMenu skillMenu;
    [SerializeField] private string skillDescription, controlDesc,statDesc;
    [SerializeField] private string psControls, xboxControls, keyboardControls;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;
    private AttackEvent curAtk;
    private TextMeshProUGUI description;
    private AudioManager audioManager;
    private string uiCursorSound = "UI/Move Cursor";
    // Start is called before the first frame update
    void Start()
    {
        skillMenu = GetComponentInParent<SkillListMenu>();
        audioManager = AudioManager.instance;
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
        SkillStats();
    }
    private void OnEnable()
    {
        audioManager = AudioManager.instance;
    }
    public void OnSelect(BaseEventData eventData)
    {
        skillMenu = GetComponentInParent<SkillListMenu>();
        skillMenu.UpdateDescriptionAreaText(skillDescription+statDesc, controlDesc);
        if (audioManager!=null)
            audioManager.PlaySound(uiCursorSound);
    }
    private int damage, comboVal;
    private void SkillStats()
    {

        if (stateIndex>0)
            curAtk = GameEngine.coreData.characterStates[stateIndex].attacks[0];
        if (curAtk != null)
        {
            damage = curAtk.damage;
            comboVal = curAtk.comboValue;
            statDesc = "\n DMG:  " + damage + "\n COMBO: " + comboVal;
        }
    }
}
