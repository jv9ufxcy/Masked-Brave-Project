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
    // Start is called before the first frame update
    void Start()
    {
        resSlider = GetComponent<Slider>();
        startController = GetComponentInParent<StartScreenController>();
        sliderDesc = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetResolutionSlider(float index)
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
        sliderDesc.color = Color.green;
    }
}
