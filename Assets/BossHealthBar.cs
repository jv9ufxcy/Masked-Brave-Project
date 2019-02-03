using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    //Health
    [SerializeField] private Image healthUI;
    [SerializeField] private Sprite[] healthSprites;

    [SerializeField] private BossHealthManager bossStats;

    private void Start()
    {

    }
    void Update()
    {
        UpdateHealthPips();
    }

    private void UpdateHealthPips()
    {
        healthUI.sprite = healthSprites[bossStats.CurrentHitPoints];
    }
}
