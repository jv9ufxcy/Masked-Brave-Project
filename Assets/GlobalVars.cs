using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVars : MonoBehaviour
{
    public enum ControllerState { ps4, xbox, keyboard}
    public static ControllerState _controllerState;
    public static int controllerNumber;
    private static GlobalVars instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        switch (controllerNumber)
        {
            case 0:
                _controllerState = ControllerState.ps4;
                break;
            case 1:
                _controllerState = ControllerState.xbox;
                break;
            case 2:
                _controllerState = ControllerState.keyboard;
                break;
            default:
                _controllerState = ControllerState.ps4;
                break;
        }
    }
    public void PassControllerValue(int dropdownValue)
    {
        controllerNumber = dropdownValue;
    }
}
