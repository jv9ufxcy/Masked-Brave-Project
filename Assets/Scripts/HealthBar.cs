using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    //Health
    [SerializeField] private Image healthUI;
    [SerializeField] private Sprite[] healthSprites;

    //meter
    [SerializeField] private Slider energySlider;

    [SerializeField] private Player playerStats;

    private void Start()
    {

    }
    void Update ()
    {
        UpdateHealthPips();
    }

    private void UpdateHealthPips()
    {
        healthUI.sprite = healthSprites[playerStats.RecoveryPoints];
        energySlider.value = playerStats.CurrentEnergyMeterAsPercentage();

        //slider value must be between 0 and 1
        //slider.value = player.CurrentHitPointsAsPercentage;
    }
}
