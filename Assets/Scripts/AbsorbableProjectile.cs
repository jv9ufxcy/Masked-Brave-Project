using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbsorbableProjectile : MonoBehaviour
{

    [SerializeField] private int healthToGive = 0;
    [SerializeField] private int meterToGive = 1;
    [SerializeField] private string absorptionSound;

    private Player player;
    private AudioManager audioManager;
    // Use this for initialization
    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponentInParent<Player>();
        if (collision.CompareTag("Shield"))
        {
            audioManager.PlaySound(absorptionSound);
            //Debug.Log("Bullet has been absorbed by " + collision.name + " for " + meterToGive + " METER and " + healthToGive + " HP.");
            player.AddRecovery(healthToGive);
            player.AddMeter(meterToGive);
            Destroy(gameObject);
        }
    }
}
