using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RadialMenuController : MonoBehaviour
{
    public Vector2 moveInput;

    public GameObject theMenu;
    public Color normalColor, highlightedColor;

    public int selectedOption;
    public GameObject highlightBlock;
    [SerializeField] private Image segmentedBlock, formIconImage;
    public CharacterObject player;
    //private List<string> formNames = new List<string>() { "CYC","BMB","ARM","PRS","ZOE","???"};
    // Start is called before the first frame update
    void Start()
    {
        highlightedColor = segmentedBlock.color;
        player.OnFormChanged += UpdateForms;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        if (theMenu.activeInHierarchy)
        {
            moveInput = player.leftStick;
            moveInput.Normalize();

            if (moveInput != Vector2.zero)
            {
                segmentedBlock.color = highlightedColor;
                float angle = Mathf.Atan2(moveInput.y, -moveInput.x) / Mathf.PI;
                float section = 360/masks.Count;//360/5 = 72
                angle *= 180;
                angle += 45f;
                if (angle < 0)
                {
                    angle += 360;
                }
                for (int i = 0; i < masks.Count; i++)
                {
                    if (angle > i * section && angle < (i + 1) * section)
                    {
                        masks[i].formNameText.color = Color.yellow;
                        selectedOption = i;
                        highlightBlock.transform.rotation = Quaternion.Euler(0, 0, i * -section);

                        if (masks[i].isUnlocked())
                        {
                            if (masks[i] == GetActiveMask())//if the same, switch face to Civ Zoe
                                formIconImage.sprite = unmasked.formIcon;
                            else
                                formIconImage.sprite = masks[i].formIcon;
                        }
                        else
                            formIconImage.sprite = unmasked.formIcon;
                    }
                    else
                    {
                        masks[i].formNameText.color = normalColor;
                    }
                }
            }
            else
            {
                selectedOption = masks.IndexOf(GetActiveMask());
                segmentedBlock.color = Color.clear;
                formIconImage.sprite = GetActiveMask().formIcon;
                foreach (ShiftMask mask in masks)
                {
                    mask.formNameText.color = normalColor;
                }
            }
        }
    }

    public void ActivateMenu()
    {
        theMenu.SetActive(true);
        PauseManager.IsGamePaused = true;
        Time.timeScale = 0.25f;
    }
    public void SelectForm()
    {
        if (theMenu.activeInHierarchy)
        {
            SwitchMask(selectedOption);

            UpdateForms();
            //masks[selectedOption].formNameText.SetText(unmasked.formName);
            theMenu.SetActive(false);
            PauseManager.IsGamePaused = false;
            Time.timeScale = 1f;
        }
    }
    [SerializeField] List<ShiftMask> masks = new List<ShiftMask>();
    [SerializeField] ShiftMask unmasked;
    public ShiftMask GetActiveMask()
    {
        foreach (ShiftMask mask in masks)
        {
            if (mask.formIndex == GameEngine.gameEngine.globalMovelistIndex)
            {
                return mask;
            }
        }
        return unmasked;
    }
    bool ninja = true, bike = true, arm = true, bomb = true;
    public void UpdateForms()
    {
        if (theMenu.activeInHierarchy)
        {
            foreach (ShiftMask mask in masks)
            {
                if (mask.isUnlocked())
                {
                    mask.DefaultName();
                }
                else
                {
                    mask.formNameText.SetText(unmasked.formName);
                }
                GetActiveMask().formNameText.SetText(unmasked.formName);
            }
        }
    }
    public void SwitchArmament()
    {
        if (GameEngine.gameEngine.globalMovelistIndex==0)
        {
            SwitchCivillian();
        }
        else
        {
            if (arm)
                player.DOChangeMovelist(0);
        }
    }

    public void SwitchBombardier()
    {
        if (GameEngine.gameEngine.globalMovelistIndex == 1)
        {
            SwitchCivillian();
        }
        else
        {
            if (bomb)
                player.DOChangeMovelist(1);
        }
    }

    public void SwitchPursuer()
    {
        if (GameEngine.gameEngine.globalMovelistIndex == 2)
        {
            SwitchCivillian();
        }
        else
        {
            if (bike)
                player.DOChangeMovelist(2);
        }
    }

    public void SwitchCyclone()
    {
        if (GameEngine.gameEngine.globalMovelistIndex == 3)
        {
            SwitchCivillian();
        }
        else
        {
            if (ninja)
                player.DOChangeMovelist(3);
        }
    }
    public void SwitchMask(int selectionIndex)
    {
        if (GameEngine.gameEngine.globalMovelistIndex == masks[selectionIndex].formIndex&&moveInput!=Vector2.zero)
        {
            SwitchCivillian();
        }
        else
        {
            if (masks[selectionIndex].isUnlocked())
                player.DOChangeMovelist(masks[selectionIndex].formIndex);
        }
    }

    public void SwitchCivillian()
    {
        unmasked.formNameText = GetActiveMask().formNameText;
        player.DOChangeMovelist(4);
    }
    
}
[System.Serializable]
public class ShiftMask
{
    public string formName, skillListName = "Zoe/Henshin";
    public int formIndex;
    public TextMeshProUGUI formNameText;
    public Sprite formIcon;
    public bool isUnlocked()
    {
        return (GameEngine.gameEngine.unlockedSkillsList.Contains(skillListName));
    }
    public void DefaultName()
    {
        formNameText.SetText(formName);
    }
}