using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class MenuSoundController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private TextMeshProUGUI uiText;
    private AudioManager audioManager;
    private string uiCursorSound = "UI/Move Cursor";
    // Start is called before the first frame update
    void Start()
    {
        uiText = GetComponentInChildren<TextMeshProUGUI>();
        audioManager = AudioManager.instance;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (uiText != null)
            uiText.color = Color.white;
    }
    public void OnSelect(BaseEventData eventData)
    {
        audioManager.PlaySound(uiCursorSound);

        if (uiText!=null)
            uiText.color = Color.yellow;
    }
}
