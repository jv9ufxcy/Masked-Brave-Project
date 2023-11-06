using System;
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
    [SerializeField]private int upgradeAmt = 2,meterAmt = 50;
    [SerializeField]private string pickupSound;
    [SerializeField] private string unlockedMoveName;
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
    private void Update()
    {
        if (collected)
        {
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        playerHP = collision.gameObject.GetComponent<HealthManager>();
        player = collision.gameObject.GetComponent<CharacterObject>();
        if (collision.CompareTag("Player") && !collected)
        {
            if (upgradeAmt > 0)
            {
                playerHP.HealthUpgrade(upgradeAmt);
            }
            if (meterAmt > 0)
            {
                player.BuildMeter(meterAmt);
            }
            if (unlockedMoveName != null)
            {
                GameEngine.gameEngine.UnlockMove(unlockedMoveName);
            }
            Pickup();
        }
    }

    private void Pickup()
    {
        audioManager.PlaySound(pickupSound);
        GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
        collected = true;
        gameObject.SetActive(false);
    }
    [Header("Magnet")]
    [SerializeField] private bool shouldAttract = false;
    [SerializeField]
    private float attractionSpeed = 8f, attractionRange = 16f;
    [SerializeField] private LayerMask whatCountsAsPlayer;
    private void FixedUpdate()
    {
        if (shouldAttract)
            MoveTowardsPlayer();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attractionRange);
    }
    private void MoveTowardsPlayer()
    {
        bool isPlayerInRange = Physics2D.OverlapCircle(transform.position, attractionRange, whatCountsAsPlayer);
        if (isPlayerInRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, GameEngine.gameEngine.mainCharacter.transform.position, attractionSpeed * Time.fixedDeltaTime);
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
