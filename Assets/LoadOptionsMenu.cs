using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOptionsMenu : MonoBehaviour
{
    [SerializeField]private OptionsMenuController optionsMenu;
    // Start is called before the first frame update
    void Start()
    {
        optionsMenu = GetComponentInParent<OptionsMenuController>();
    }

    private void OnEnable()
    {
        if (optionsMenu != null)
        {
            optionsMenu.ApplyVideoOptions();
            Debug.Log("Options Loaded");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
