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
    public GameObject[] options;
    public Color normalColor, highlightedColor;

    public int selectedOption;
    public GameObject highlightBlock;
    public CharacterObject player;

    // Start is called before the first frame update
    void Start()
    {
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
                float angle = Mathf.Atan2(moveInput.y, -moveInput.x) / Mathf.PI;
                angle *= 180;
                angle += 90f;
                if (angle < 0)
                {
                    angle += 360;
                }
                for (int i = 0; i < options.Length; i++)
                {
                    if (angle > i * 72 && angle < (i + 1) * 72)
                    {
                        options[i].GetComponent<TextMeshProUGUI>().color = highlightedColor;
                        selectedOption = i;
                        highlightBlock.transform.rotation = Quaternion.Euler(0, 0, i * -72);
                        options[i].transform.DOScale(2, .2f);
                    }
                    else
                    {
                        options[i].GetComponent<TextMeshProUGUI>().color = normalColor;
                        options[i].transform.DOScale(1, .4f);
                    }
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
            switch (selectedOption)
            {
                case 0:
                    //SwitchWindGod();
                    break;
                case 1:
                    SwitchPursuer();
                    break;
                case 2:
                    SwitchBrave();
                    break;
                case 3:
                    SwitchBomb();
                    break;
                case 4:
                    
                    break;
                default:
                    break;
            }

            theMenu.SetActive(false);
            PauseManager.IsGamePaused = false;
            Time.timeScale = 1f;
        }
    }
    public void SwitchBrave()
    {
        player.DOChangeMovelist(0);
    }

    public void SwitchBomb()
    {
        player.DOChangeMovelist(1);
    }

    public void SwitchPursuer()
    {
        player.DOChangeMovelist(2);
    }

    public void SwitchWindGod()
    {
        player.DOChangeMovelist(3);
    }

    public void SwitchYellow()
    {
        player.DOChangeMovelist(4);
    }

}
