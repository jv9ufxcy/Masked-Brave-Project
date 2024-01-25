using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeEnergyPickup : MonoBehaviour,IDataPersistence
{
    [SerializeField] string id;
    [ContextMenu("Generate GUID")]
    private void GenerateGUID()
    {
        id = System.Guid.NewGuid().ToString();
    }

    CharacterObject player;
    AudioManager audioManager;
    [SerializeField] private int meterAmt = 25;
    [SerializeField] private string pickupSound;
    [SerializeField] private GameObject pickupEffect;
    private bool collected;
    private SpriteRenderer spriteRend;
    // Start is called before the first frame update
    void Start()
    {
        audioManager = AudioManager.instance;
        spriteRend = gameObject.GetComponent<SpriteRenderer>();
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void FixedUpdate()
    {
        if (collected && spriteRend.enabled)
        {
            spriteRend.enabled = false;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collected)
        {
            player = collision.gameObject.GetComponent<CharacterObject>();
            if (collision.CompareTag("Player"))
            {
                if (meterAmt > 0)
                {
                    player.BuildMeter(meterAmt);
                    player.UpgradeMeter(meterAmt);
                }
                Pickup();
            }
        }
    }
    private void Pickup()
    {
        audioManager.PlaySound(pickupSound);
        GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
        collected = true;
        CollectedDataObject();
    }
    private void CollectedDataObject()
    {
        spriteRend.enabled = false;
        collected = true;
    }
    public void LoadData(GameData data)
    {
        if (data.energyTanksCollected.Contains(id))
        {
            collected = true;
            Debug.Log(collected);
        }
    }


    public void SaveData(GameData data)
    {
        if (collected)
        {
            if (!data.energyTanksCollected.Contains(id))//check if list doesnt contain id
            {
                data.energyTanksCollected.Add(id);
            }
        }
    }
}
