using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinScript : MonoBehaviour
{
    static int coinCount = 0;

    Text coinCountText;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    // Use this for initialization

    private void OnPlayerRespawnedFromCheckpoint()
    {
        ReenableCoin();
        coinCount = 0;
    }
    private void ReenableCoin()
    {
        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;
    }
    private void OnEnable()
    {
        RespawnPlayer.PlayerRespawnedFromCheckpoint += OnPlayerRespawnedFromCheckpoint;
    }
    private void OnDisable()
    {
        RespawnPlayer.PlayerRespawnedFromCheckpoint -= OnPlayerRespawnedFromCheckpoint;
    }
    void Start ()
    {
        coinCountText=GameObject.Find("Text").GetComponent<Text>();
        coinCountText.text = coinCount+ " Gears";
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
	}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            audioSource.Play();
            coinCount++;
            coinCountText.text= coinCount+ " Gears";
            spriteRenderer.enabled = false;
            boxCollider2D.enabled = false;
            //Destroy(this.gameObject);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
