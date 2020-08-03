using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRecoveryItem : MonoBehaviour
{
    [Header("Recovery Val")]
    [SerializeField] private int healthToGive = 1;
    //[SerializeField] private int ammoToGive = 0;
    [SerializeField] private int meterToGive = 0;
    [Header("Magnet Val")]
    [SerializeField] private bool shouldAttract = false;
    [SerializeField] private float attractionSpeed = 50f;
    [SerializeField] private float attractionRange = 10f;
    [SerializeField] private LayerMask whatCountsAsPlayer;
    [SerializeField] private string pickupSound="PickupRecovery";
    [SerializeField] private GameObject pickupEffect;
    private CharacterObject thePlayer;
    private bool isPlayerInRange;

    private HealthManager player;
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
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<HealthManager>();
        if (collision.CompareTag("Player"))
        {
            if (healthToGive > 0)
            {
                player.AddHealth(healthToGive);
            }
            if (meterToGive > 0)
            {
                player.ChangeMeter(meterToGive);
            }
            GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            audioManager.PlaySound(pickupSound);
            Destroy(gameObject);
        }
    }
    private void MoveTowardsPlayer()
    {
        //TODO: Add a state machine loop that counts down between standing still to shoot and then moving
        isPlayerInRange = Physics2D.OverlapCircle(transform.position, attractionRange, whatCountsAsPlayer);
        if (isPlayerInRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, thePlayer.transform.position, attractionSpeed * Time.fixedDeltaTime);
        }
    }
}
