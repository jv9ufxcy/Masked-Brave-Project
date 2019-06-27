using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image healthUI;
    [SerializeField] private Sprite[] healthSprites;
    [Header("Busters")]
    [SerializeField] private Image busterUI;
    [SerializeField] private Sprite[] busterSprites;
    [Header("Energy")]
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
        busterUI.sprite = busterSprites[playerStats.CurrentNumberOfBullets];
        energySlider.value = playerStats.CurrentEnergyMeterAsPercentage();

        //slider value must be between 0 and 1
        //slider.value = player.CurrentHitPointsAsPercentage;
    }
}
