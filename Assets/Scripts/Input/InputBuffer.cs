using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputBuffer
{
    public static string[] rawInputList = new string[]
    {
        "Jump",
        "Dash",
        "Attack",
        "Special",

    };
    public List<InputBufferItem> inputList = new List<InputBufferItem>();
    public void Update()
    {
        GameManager.instance.playerInputBuffer = this;
        if (inputList.Count < rawInputList.Length || inputList.Count==0)
        {
            InitializeBuffer();
        }

        foreach (InputBufferItem c in inputList)
        {
            c.ResolveCommand();

            for (int b = 0; b < c.buffer.Count - 1; b++)
            {
                c.buffer[b].hold = c.buffer[b + 1].hold;
                c.buffer[b].used = c.buffer[b + 1].used;
            }
        }
    }
    void InitializeBuffer()
    {
        inputList = new List<InputBufferItem>();
        foreach (string s in rawInputList)
        {
            InputBufferItem newB = new InputBufferItem();
            newB.button = s;
            inputList.Add(newB);
        }
    }
}
public class InputBufferItem
{
    public string button;
    public List<InputStateItem> buffer;
    public static int bufferWindow = 12;
    public InputBufferItem()
    {
        buffer = new List<InputStateItem>();
        for (int i = 0; i < bufferWindow; i++)
        {
            buffer.Add(new InputStateItem());
        }
    }
    public void ResolveCommand()
    {
        if (Input.GetButton(button))
        {
            buffer[buffer.Count - 1].HoldUp();
        }
        else
        {
            buffer[buffer.Count - 1].ReleaseHold();
        }
    }
}
public class InputStateItem
{
    public int hold;
    public bool used;
    public bool CanExecute()
    {
        if (hold == 1 && !used){return true;}
        return false;
    }
    public void HoldUp()
    {
        if (hold < 0)
        {
            hold = 1;
        }
        else
            hold += 1;
    }
    public void ReleaseHold()
    {
        if (hold > 0)
        {
            hold = -1; used = false;
        }
        else
            hold = 0;
    }
}