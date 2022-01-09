using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class KudosHandler : MonoBehaviour
{
    [Header("Color")]
    private Image scoreContainer;
    private Color defaultColor;

    private void Start()
    {
        scoreContainer = GetComponent<Image>();
        defaultColor = scoreContainer.color;
    }
    public void SetContainerColor(int index)
    {
        switch (index)
        {
            case 0:
                scoreContainer.DOColor(defaultColor, 0);
                break;
            case 1:
                scoreContainer.DOColor(Color.red, 0.25f);
                break;
            case 2:
                scoreContainer.DOColor(Color.yellow, 0.25f);
                break;
            default:
                scoreContainer.DOColor(defaultColor, 0);
                break;
        }
        
    }
    [Header("Strikes")]
    public int numOfStrikes;
    [SerializeField] private Image[] strikes;
    [SerializeField] private Sprite fullStrike, emptyStrike;
    public void UpdateStrike(int strikeHealth)
    {
        if (strikeHealth>numOfStrikes)
        {
            strikeHealth = numOfStrikes;
        }
        for (int i = 0; i < strikes.Length; i++)
        {
            if (i<strikeHealth)
            {
                strikes[i].sprite = fullStrike;
            }
            else
                strikes[i].sprite = emptyStrike;

            if (i<numOfStrikes)
            {
                strikes[i].enabled=true;
            }
            else
                strikes[i].enabled = false;
        }
    }
}
