using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipTrigger : MonoBehaviour
{
    DialogueManager dialogueManager;
    [SerializeField] private string tooltipText, ttModifier = "Press ", ttButton = "<sprite=12>", ttDescription = "to JASON";
    // Start is called before the first frame update
    void Start()
    {
        dialogueManager = DialogueManager.instance;
        tooltipText = ttModifier + " <size=265%>" + ttButton + "<size=100%> " + ttDescription;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dialogueManager.ShowTooltip(tooltipText);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dialogueManager.HideTooltip();
        }
    }
}
