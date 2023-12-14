using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Checkpoint currentlyActiveCheckpoint;
    private GameManager gm;


    [SerializeField]
    private float activatedScale, deactivatedScale;
    [SerializeField]
    private Color activatedColor, deactivatedColor;

    private bool isActive = false;
    private SpriteRenderer spriteRenderer;
    AudioManager audioManager;
    public string checkpointSound = "Props/Checkpoint";
    [SerializeField] private int checkPointFXIndex = 14;
    [SerializeField] private GameObject checkPointFX;

    //use this for initialization
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gm = GameManager.instance;
        DeactivateCheckpoint();
        audioManager = AudioManager.instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !isActive)
        {
            ActivateCheckpoint();
        }
    }
    private void ActivateCheckpoint()
    {
        if (currentlyActiveCheckpoint != null)
        {
            currentlyActiveCheckpoint.DeactivateCheckpoint();
        }
        
        isActive = true;
        currentlyActiveCheckpoint = this;
        transform.localScale = transform.localScale * activatedScale;
        spriteRenderer.color = activatedColor;
        gm.lastCheckpointPos = currentlyActiveCheckpoint.transform.position;

        //GameEngine.gameEngine.mainCharacter.FullyHeal();
        Mission.instance.CompleteScore();
        Mission.instance.CheckpointTime();
        audioManager.PlaySound(checkpointSound);
        GameEngine.GlobalPrefab(checkPointFXIndex,checkPointFX, -1, -1);
    }
    private void DeactivateCheckpoint()
    {
        isActive = false;
        transform.localScale = Vector3.one * deactivatedScale;
        spriteRenderer.color = deactivatedColor;
    }
}