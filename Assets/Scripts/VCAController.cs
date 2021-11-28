using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class VCAController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private FMOD.Studio.VCA vcaController;
    public string vcaName = "Master";
    [SerializeField] private float vcaVolume;
    private Slider audioSlider;
    [SerializeField]private TextMeshProUGUI sliderDesc;
    // Start is called before the first frame update
    void Start()
    {
        vcaController = FMODUnity.RuntimeManager.GetVCA("vca:/"+vcaName);
        audioSlider = GetComponent<Slider>();
        vcaController.getVolume(out vcaVolume);
        sliderDesc = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetVolume(float volume)
    {
        vcaController.setVolume(volume/10);
        vcaController.getVolume(out vcaVolume);
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
