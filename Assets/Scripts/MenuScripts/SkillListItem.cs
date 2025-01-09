using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Security.Cryptography.X509Certificates;

public class SkillListItem : MonoBehaviour, ISelectHandler
{
    private SkillListMenu skillMenu;
    [SerializeField] private string skillDescription, controlDesc,statDesc;
    [SerializeField] private string psControls, xboxControls, keyboardControls;
    [Header("State Index")]
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;
    [Header("Henshin Index")]
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int henshinIndex;
    [SerializeField] int atkIndex = 0;
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
        if (henshinIndex>0)
        {
            DisableUntilUnlocked();
        }
    }
    private void OnEnable()
    {
        audioManager = AudioManager.instance;
    }

    private void DisableUntilUnlocked()
    {
        if (GameEngine.coreData.characterStates[henshinIndex].lockedMoveCheck && !GameEngine.gameEngine.IsSkillUnlocked(GameEngine.coreData.characterStates[henshinIndex].stateName))
        {
            gameObject.SetActive(false);
        }
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
        if (stateIndex > 0)
        {
            if (atkIndex> GameEngine.coreData.characterStates[stateIndex].attacks.Count)
            {
                 atkIndex = GameEngine.coreData.characterStates[stateIndex].attacks.Count;
            }
            curAtk = GameEngine.coreData.characterStates[stateIndex].attacks[atkIndex];
        }
        if (curAtk != null)
        {
            damage = curAtk.damage;
            comboVal = curAtk.comboValue;
            statDesc = "\n DMG:  " + damage + "\n COMBO: " + comboVal;
        }
    }
}
