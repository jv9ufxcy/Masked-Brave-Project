using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ResolutionController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Slider resSlider;
    [SerializeField] private OptionsMenuController optionsController;
    [SerializeField] private TextMeshProUGUI sliderDesc;
    public int resolutionIndex;
    private AudioManager audioManager;
    private void OnEnable()
    {
        resSlider = GetComponent<Slider>();
        optionsController = GetComponentInParent<OptionsMenuController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        optionsController = GetComponentInParent<OptionsMenuController>();
        sliderDesc = GetComponentInChildren<TextMeshProUGUI>();
        audioManager = AudioManager.instance;
    }
    public void DefaultSliderPos(int index)
    {
        if (resSlider != null)
        {
            resSlider.value = index;
            SetResolutionSlider(index);
            Debug.Log("Set Slider position to default: " + resSlider.value);
        }
        else
        {
            resSlider = GetComponent<Slider>();
            resSlider.value = index;
            SetResolutionSlider(index);
        }
    }
    public void SetResolutionSlider(float index)//unity event
    {
        resolutionIndex = (int)index;
        if (optionsController != null)
            optionsController.SetResolution(resolutionIndex);
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
    private void OnDisable()
    {
        sliderDesc.color = Color.white;
    }
}
