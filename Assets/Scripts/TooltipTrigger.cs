using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipTrigger : MonoBehaviour
{
    DialogueManager dialogueManager;
    [SerializeField] private string tooltipText, ttModifier = "Press ", ttButton = "<sprite=12>", ttDescription = "to JASON";
    [Tooltip("PS,XB,PC")]
    [SerializeField] private string[] ttButtons = new string[3];
    // Start is called before the first frame update
    void Start()
    {
        if (GameEngine.coreData!=null)
        {
            switch (GameEngine.coreData.currentControllerIndex)
            {
                case 0://ps
                    ttButton = ttButtons[0];
                    break;
                case 1://xbox
                    ttButton = ttButtons[1];
                    break;
                case 2://kb
                    ttButton = ttButtons[2];
                    break;
                default:
                    break;
            }
        }
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
