using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnInput : MonoBehaviour
{
    public EventSystem eventSystem;
    public GameObject selectedObject;

    private bool buttonSelected;
	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetButton("Cancel") && eventSystem.alreadySelecting == false)
        {
            if (selectedObject!=null)
            {
                SelectDefaultObject();
            }
            //buttonSelected = true;
        }
	}

    public void SelectDefaultObject()
    {
        eventSystem.SetSelectedGameObject(selectedObject);
    }
    public void SelectNewObject(GameObject menuObj)
    {
        eventSystem.SetSelectedGameObject(menuObj);
        selectedObject = menuObj;
    }

    private void OnDisable()
    {
        //buttonSelected = false;
    }
}
