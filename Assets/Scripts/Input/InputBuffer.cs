﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputBuffer
{public List<InputBufferFrame> buffer;// = new List<InputBufferFrame>();
    public static int bufferWindow = 25;

    public List<int> buttonCommandCheck;
    public List<int> motionCommandCheck;

    void InitializeBuffer()
    {
        buffer = new List<InputBufferFrame>();
        for (int i = 0; i < bufferWindow; i++)
        {
            InputBufferFrame newB = new InputBufferFrame();
            newB.InitializeFrame();
            buffer.Add(newB);
        }

        buttonCommandCheck = new List<int>();
        for (int i = 0; i < GameEngine.coreData.rawInputs.Count; i++)
        {
            buttonCommandCheck.Add(-1);
        }

        motionCommandCheck = new List<int>();
        for (int i = 0; i < GameEngine.coreData.motionCommands.Count; i++)
        {
            motionCommandCheck.Add(-1);
        }
    }

    public void Update()
    {
        GameEngine.gameEngine.playerInputBuffer = this;
        if (buffer == null) { InitializeBuffer(); }
        if (buffer.Count < GameEngine.coreData.rawInputs.Count || buffer.Count == 0)
        {
            InitializeBuffer();
        }

        for (int i = 0; i < buffer.Count - 1; i++)
        {
            for (int r = 0; r < buffer[i].rawInputs.Count; r++)
            {
                buffer[i].rawInputs[r].value = buffer[i + 1].rawInputs[r].value;
                buffer[i].rawInputs[r].hold = buffer[i + 1].rawInputs[r].hold;
                buffer[i].rawInputs[r].used = buffer[i + 1].rawInputs[r].used;
            }
        }
        buffer[buffer.Count - 1].Update();

        for (int r = 0; r < buttonCommandCheck.Count; r++)
        {
            buttonCommandCheck[r] = -1;
            for (int b = 0; b < buffer.Count; b++)
            {
                if (buffer[b].rawInputs[r].CanExecute()) { buttonCommandCheck[r] = b; }
            }
            if (GameEngine.coreData.rawInputs[r].inputType == RawInput.InputType.IGNORE) { buttonCommandCheck[r] = 0; }

        }

        for (int m = 0; m < motionCommandCheck.Count; m++)
        {
            motionCommandCheck[m] = -1;
            GameEngine.coreData.motionCommands[m].checkStep = 0;
            GameEngine.coreData.motionCommands[m].curAngle = 0;
            for (int b = 0; b < buffer.Count; b++)
            {
                //if (UpdateMotionCheck(m, b)) { GameEngine.coreData.motionCommands[m].checkStep++; }
                //if (GameEngine.coreData.motionCommands[m].TestCheck(MotionCommand.GetNumPadDirection(buffer[b].rawInputs[4].value, buffer[b].rawInputs[5].value)))

                if (GameEngine.coreData.motionCommands[m].TestCheck(buffer[b].rawInputs[13].value, buffer[b].rawInputs[14].value)) //horizontal and vertical inputs in the raw input list
                { motionCommandCheck[m] = 1; break; }
            }
        }

    }

    

    

    public void UseInput(int _i, int _m)
    {
        
        buffer[buttonCommandCheck[_i]].rawInputs[_i].used = true;
        //Debug.Log("USED UP!!!> : " + buttonCommandCheck[_i].ToString());
        buttonCommandCheck[_i] = -1;//used button

        buffer[motionCommandCheck[_m]].rawInputs[_m].used = true;
        motionCommandCheck[_m] = -1;//used motion

        //buffer[buttonCommandCheck[_i]].rawInputs[_i].hold = -2;

    }
}


public class InputBufferFrame
{
    public List<InputBufferFrameState> rawInputs;

    public void InitializeFrame()
    {
        rawInputs = new List<InputBufferFrameState>();
        for (int i = 0; i < GameEngine.coreData.rawInputs.Count; i++)
        {
            InputBufferFrameState newFS = new InputBufferFrameState();
            newFS.rawInput = i;
            rawInputs.Add(newFS);
        }
    }

    public void Update()
    {
        if (rawInputs == null) { InitializeFrame(); }
        if(rawInputs.Count == 0 || rawInputs.Count != GameEngine.coreData.rawInputs.Count) { InitializeFrame(); }
        foreach(InputBufferFrameState fs in rawInputs)
        {
            fs.ResolveCommand();
        }
    }

}

public class InputBufferFrameState
{
    public int rawInput;
    public float value;
    public int hold;
    public bool used;

    public void ResolveCommand()
    {
        used = false;
        switch (GameEngine.coreData.rawInputs[rawInput].inputType)
        {
            case RawInput.InputType.BUTTON:
                if (Input.GetButton(GameEngine.coreData.rawInputs[rawInput].name))
                {
                    HoldUp(1f);
                }
                else
                {
                    ReleaseHold();
                }
                break;
            case RawInput.InputType.AXIS:
                if (Mathf.Abs(Input.GetAxisRaw(GameEngine.coreData.rawInputs[rawInput].name)) > GameEngine.gameEngine.deadZone)
                {
                    HoldUp(Input.GetAxisRaw(GameEngine.coreData.rawInputs[rawInput].name));
                }
                else
                { ReleaseHold(); }
                break;
        }
    }

    public void HoldUp(float _val)
    {
        value = _val;

        if (hold < 0) { hold = 1; }
        else { hold += 1; }

    }

    public void ReleaseHold()
    {
        if (hold > 0) { hold = -1; used = false; }
        else { hold = 0; }
        value = 0;
        //GameEngine.gameEngine.playerInputBuffer.buttonCommandCheck[rawInput] = 0;
    }

    public bool CanExecute()
    {
        if (hold == 1 && !used) { return true; }
        return false;
    }

    public bool MotionNeutral()
    {
        if(Mathf.Abs(value) < GameEngine.gameEngine.deadZone) { return true; }
        return false;
    }
}


[System.Serializable]
public class RawInput
{
    public enum InputType { BUTTON, AXIS, DOUBLE_AXIS, DIRECTION, IGNORE }
    public InputType inputType;
    public string name;
}



[System.Serializable]
public class MotionCommand
{
    public string name;
    public int motionWindow;
    public int confirmWindow;
    //[IndexedItem(IndexedItemAttribute.IndexedItemType.MOTION_COMMAND_STEP)]
    public List<MotionCommandDirection> commands;
    public bool clean;
    public bool anyOrder;

    public int checkStep;

    //public bool absoluteDirection;
    public int angleChange;

    public float prevAngle;
    public float curAngle;

    public bool TestCheck(float _x, float _y)//(MotionCommandDirection _dir)
    {
        if (angleChange > 0)
        {
            GetNumPadDirection(_x, _y);
            if (curAngle >= angleChange) { return true; }
        }
        else
        {
            if (commands == null) { return true; }

            if (checkStep >= commands.Count) { return true; }
            if (commands[checkStep] == GetNumPadDirection(_x, _y)) { checkStep++; }
            //if (commands[checkStep] == _dir) { checkStep++; }
        }
        return false;
    }

    public enum MotionCommandDirection
    {
        NEUTRAL, FORWARD, BACK, SIDE, ANGLE_CHANGE
    }

    public MotionCommandDirection GetNumPadDirection(float _x, float _y)
    {
        //if (Mathf.Abs(_x) > GameEngine.gameEngine.deadZone || Mathf.Abs(_y) > GameEngine.gameEngine.deadZone) { return 8; }
        //else { return 5; }
        //return 5;

        if (Mathf.Abs(_x) > GameEngine.gameEngine.deadZone || Mathf.Abs(_y) > GameEngine.gameEngine.deadZone)
        {

            Vector3 charForward = GameEngine.gameEngine.mainCharacter.character.transform.forward;
            Vector3 stickForward = new Vector3();// = buffer[buffer.Count - 1].stick;
            Vector3 camForward = Camera.main.transform.forward;



            camForward.y = 0;
            camForward.Normalize();
            stickForward += camForward * _y;

            stickForward += Camera.main.transform.right * _x;
            stickForward.y = 0;
            stickForward.Normalize();


            float _stickAngle = Vector2.Angle(new Vector2(charForward.x, charForward.z), new Vector2(stickForward.x, stickForward.z));

            if (angleChange > 0)
            {

                //if (Mathf.Abs(_stickAngle) > angleChange / commands.Count) { return MotionCommandDirection.ANGLE_CHANGE; }
                // curr
                _stickAngle = Vector2.Angle(new Vector2(0f, 1f), new Vector2(stickForward.x, stickForward.z));
                curAngle += Mathf.Abs(_stickAngle - prevAngle);
                prevAngle = _stickAngle;
                //curAngle = _stickAngle;
                //if (Mathf.Abs(curAngle) >= angleChange) { return MotionCommandDirection.ANGLE_CHANGE; }
                //if (Mathf.Abs(curAngle) > angleChange / commands.Count) { return MotionCommandDirection.ANGLE_CHANGE; }
                return MotionCommandDirection.ANGLE_CHANGE;
            }
            

            if (_stickAngle < 45) { return MotionCommandDirection.FORWARD; }
            else if (_stickAngle < 135) { return MotionCommandDirection.SIDE; }
            else { return MotionCommandDirection.BACK; }
        }

        return MotionCommandDirection.NEUTRAL;


    }
}

[System.Serializable]
public class MotionCommandStep
{
    public string name;
    
    public MotionCommand.MotionCommandDirection command;
}
