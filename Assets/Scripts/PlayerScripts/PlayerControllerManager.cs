using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerManager/* : MonoBehaviour*/
{
    public string joinName; //this gets set by the join manager. This is the button that they pushed to join. We use it to tell which controls they use.

    public enum ControllerList { keyboard, xbox, ps4 }
    public ControllerList myController;

    #region ButtonNames
    //We're gonna set these names based on what controller is connected
    public string horizontalAxisName;
    public string altHorizontalAxisName;
    public string verticalAxisName;
    public string altVerticalAxisName;
    public string topFaceButtonName { get; private set; }
    public string bottomFaceButtonName { get; private set; }
    public string leftFaceButtonName { get; private set; }
    public string rightFaceButtonName { get; private set; }
    public string leftBumperName { get; private set; }
    public string rightBumperName { get; private set; }
    public string leftTriggerName { get; private set; }
    public string rightTriggerName { get; private set; }
    public string startButtonName { get; private set; }
    public string selectButtonName { get; private set; }
    #endregion


    public PlayerControllerManager(string joinButtonName/*Image image, Sprite ps4Icon, Sprite xboxIcon, int playerNumFromManager,*/ )
    {
        joinName = joinButtonName;
        CheckWhichControllersAreConnected();
    }

    private void CheckWhichControllersAreConnected()
    {
        #region ControllerCheck

        int joystickNumber = Input.GetJoystickNames().Length;//get how many axes are connected to our controller


        if (joystickNumber == 19)
        {
            myController = ControllerList.ps4;

            horizontalAxisName = "Ps4Horizontal";
            altHorizontalAxisName = "altPs4Horizontal";
            verticalAxisName = "Ps4Vertical";
            altVerticalAxisName = "altPs4Vertical";
            topFaceButtonName = "Ps4Triangle";
            bottomFaceButtonName = "Ps4X";
            leftFaceButtonName = "Ps4Square";
            rightFaceButtonName = "Ps4O";
            leftBumperName = "Ps4L1";
            rightBumperName = "Ps4R1";
            leftTriggerName = "Ps4L2";
            rightTriggerName = "Ps4R2";
            startButtonName = "Ps4Options";
            selectButtonName = "Ps4Share";
        }
        else if (joystickNumber == 33)//check if we have an xbox controller
        {
            myController = ControllerList.xbox;

            horizontalAxisName = "XboxHorizontal";
            altHorizontalAxisName = "altXboxHorizontal";
            verticalAxisName = "XboxVertical";
            altVerticalAxisName = "altXboxVertical";
            topFaceButtonName = "XboxY";
            bottomFaceButtonName = "XboxA";
            leftFaceButtonName = "XboxX";
            rightFaceButtonName = "XboxB";
            leftBumperName = "XboxLB";
            rightBumperName = "XboxRB";
            leftTriggerName = "XboxLT";
            rightTriggerName = "XboxRT";
            startButtonName = "XboxMenu";
            selectButtonName = "XboxBack";
        }

        else  //first check for a keyboard control
        {
            myController = ControllerList.keyboard;
            horizontalAxisName = "KeyboardHorizontal";
            altHorizontalAxisName = horizontalAxisName;//no alt button with keyboard
            verticalAxisName = "KeyboardVertical";
            altVerticalAxisName = verticalAxisName;
            topFaceButtonName = "KeyboardV";
            bottomFaceButtonName = "KeyboardZ";
            leftFaceButtonName = "KeyboardX";
            rightFaceButtonName = "KeyboardC";
            leftBumperName = "KeyboardQ";
            rightBumperName = "KeyboardE";
            leftTriggerName = "KeyboardLeftShift";
            rightTriggerName = "KeyboarLeftCtrl";
            startButtonName = "KeyboardEscape";
            selectButtonName = "KeyboardBackspace";
        }
    }
    #endregion

}
