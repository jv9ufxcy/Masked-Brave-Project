using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRecoveryItem : MonoBehaviour
{
    //recovery values
    [SerializeField] private int healthToGive = 1;
    //[SerializeField] private int ammoToGive = 0;
    [SerializeField] private int meterToGive = 0;
    //magnet values
    [SerializeField] private bool shouldAttract = false;
    [SerializeField] private float attractionSpeed = 50f;
    [SerializeField] private float attractionRange = 10f;
    [SerializeField] private LayerMask whatCountsAsPlayer;
    [SerializeField] private Player thePlayer;
    private bool isPlayerInRange;

    private Player player;
    private AudioManager audioManager;
    // Use this for initialization
    void Start()
    {
        if (shouldAttract)
            thePlayer = FindObjectOfType<Player>();
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldAttract)
            MoveTowardsPlayer();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponentInParent<Player>();
        if (collision.CompareTag("Player"))
        {
            if (healthToGive > 0)
            {
                player.AddRecovery(healthToGive);
            }
            if (meterToGive > 0)
            {
                player.AddMeter(meterToGive);
            }
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
