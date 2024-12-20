﻿using UnityEngine;

public class PickupRecoveryItem : MonoBehaviour
{
    [Header("Recovery Val")]
    [SerializeField] private int healthToGive = 1,currencyToGive = 0,meterToGive = 0;
    [Header("Magnet Val")]
    [SerializeField] private bool shouldAttract = false, shouldDestroy = true;
    [SerializeField] private float attractionSpeed = 50f, attractionRange = 10f, lifeTime = 8f;
    [SerializeField] private LayerMask whatCountsAsPlayer;
    [SerializeField] private string pickupSound="PickupRecovery";
    [SerializeField] private GameObject pickupEffect;
    private CharacterObject thePlayer;
    private bool isPlayerInRange;

    private HealthManager playerHP;
    private CharacterObject player;
    private AudioManager audioManager;
    // Use this for initialization
    void Start()
    {
        if (shouldAttract)
            thePlayer = GameEngine.gameEngine.mainCharacter;

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (shouldAttract)
            MoveTowardsPlayer();

        if (shouldDestroy)
        {
            if (lifeTime > 0)
            {
                lifeTime -= Time.deltaTime;
            }
            else if (lifeTime <= 0)
            {
                Destroy(transform.parent.gameObject);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        playerHP = collision.gameObject.GetComponent<HealthManager>();
        player = collision.gameObject.GetComponent<CharacterObject>();
        if (collision.CompareTag("Player"))
        {
            if (healthToGive > 0)
            {
                playerHP.AddHealth(healthToGive);
            }
            if (meterToGive > 0)
            {
                player.BuildMeter(meterToGive);
            }
            if (currencyToGive>0&&Mission.instance!=null)
            {
                Mission.instance.ChangeCurrency(currencyToGive);
            }
            audioManager.PlaySound(pickupSound);
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(transform.parent.gameObject);
        }
    }
    private void MoveTowardsPlayer()
    {
        isPlayerInRange = Physics2D.OverlapCircle(transform.position, attractionRange, whatCountsAsPlayer);
        if (isPlayerInRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, thePlayer.transform.position, attractionSpeed * Time.fixedDeltaTime);
        }
    }
}
