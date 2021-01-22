using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField]
    [Tooltip("All of the images that health UI uses. Background should be 0, healthfill: 1, and damagefill: 2")]
    private Image[] sliderImages;
    [SerializeField] private Color healthColor, damageColor;
    [SerializeField] private Image healthImage, damageImage;
    [SerializeField] private EnemyHealthManager eHealth;
    // Start is called before the first frame update
    void Start()
    {
        //eHealth = GetComponent<EnemyHealthManager>();
    }

    // Update is called once per frame
    void Update()
    {
        healthImage.fillAmount = eHealth.CurrentHitPoints / 100f;
    }
}
