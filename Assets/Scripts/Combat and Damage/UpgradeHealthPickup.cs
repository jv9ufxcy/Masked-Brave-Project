using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeHealthPickup : MonoBehaviour,IDataPersistence
{
    [SerializeField] string id;
    [ContextMenu("Generate GUID")]
    private void GenerateGUID()
    {
        id=System.Guid.NewGuid().ToString();
    }

    CharacterObject player;
    HealthManager playerHP;
    AudioManager audioManager;
    [SerializeField]private int upgradeAmt = 2;
    [SerializeField]private string pickupSound;
    [SerializeField]private GameObject pickupEffect;
    private bool collected = false;
    // Start is called before the first frame update
    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        playerHP = collision.gameObject.GetComponent<HealthManager>();
        player = collision.gameObject.GetComponent<CharacterObject>();
        if (collision.CompareTag("Player")&&!collected)
        {
            playerHP.HealthUpgrade(upgradeAmt);
            audioManager.PlaySound(pickupSound);
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            collected = true;
            gameObject.SetActive(false);
        }
    }

    public void LoadData(GameData data)
    {
        data.upgradesCollected.TryGetValue(id, out collected);
        if (collected)
        {
            gameObject.SetActive(false);
        }
    }

    public void SaveData(GameData data)
    {
        if (data.upgradesCollected.ContainsKey(id))
        {
            data.upgradesCollected.Remove(id);
        }
        data.upgradesCollected.Add(id, collected);
    }
}
