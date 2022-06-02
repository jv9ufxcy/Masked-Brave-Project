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
    private AudioManager audioManager;
    private string uiCursorSound = "UI/Move Cursor";
    // Start is called before the first frame update
    void Start()
    {
        vcaController = FMODUnity.RuntimeManager.GetVCA("vca:/"+vcaName);
        audioSlider = GetComponent<Slider>();
        vcaController.getVolume(out vcaVolume);
        sliderDesc = GetComponentInChildren<TextMeshProUGUI>();
        audioManager = AudioManager.instance;
    }

    public void SetVolume(float volume)
    {
        vcaController.setVolume(volume/10);
        vcaController.getVolume(out vcaVolume);
        audioManager.PlaySound(uiCursorSound);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        sliderDesc.color = Color.white;
    }
    public void OnSelect(BaseEventData eventData)
    {
        audioManager.PlaySound(uiCursorSound);
        sliderDesc.color = Color.yellow;
    }
}
