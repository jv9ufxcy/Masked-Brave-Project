using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ResolutionController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Slider resSlider;
    private StartScreenController startController;
    [SerializeField] private TextMeshProUGUI sliderDesc;
    public int resolutionIndex;
    private AudioManager audioManager;
    // Start is called before the first frame update
    void Start()
    {
        resSlider = GetComponent<Slider>();
        startController = GetComponentInParent<StartScreenController>();
        sliderDesc = GetComponentInChildren<TextMeshProUGUI>();
        audioManager = AudioManager.instance;
    }

    public void SetResolutionSlider(float index)//unity event
    {
        resolutionIndex = (int)index;
        startController.SetResolution(resolutionIndex);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        sliderDesc.color = Color.white;
    }
    public void OnSelect(BaseEventData eventData)
    {
        sliderDesc.color = Color.yellow;
        //audioManager.PlaySound("UI/Move Cursor");
    }
}
