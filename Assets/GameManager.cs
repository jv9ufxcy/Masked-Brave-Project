using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Vector2 lastCheckpointPos, startingCheckpointPos = new Vector2(-4.5f, -3.5f);
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
    private void Start()
    {
        Application.targetFrameRate = 60;
    }
    public void RestoreCheckpointStart()
    {
        lastCheckpointPos = startingCheckpointPos;
    }
    public InputBuffer playerInputBuffer;
    void OnGUI()
    {

        int xSpace = 20;
        int ySpace = 25;
        for (int i = 0; i < playerInputBuffer.inputList.Count; i++)
        {
            GUI.Label(new Rect(xSpace, i * ySpace, 100, 20), playerInputBuffer.inputList[i].button + ":");
            for (int j = 0; j < playerInputBuffer.inputList[i].buffer.Count; j++)
            {
                if (playerInputBuffer.inputList[i].buffer[i].used)
                {
                    GUI.Label(new Rect(j * xSpace + 100, i * ySpace, 100, 20), playerInputBuffer.inputList[i].buffer[j].hold.ToString() + "*");
                }
                GUI.Label(new Rect(j * xSpace + 100, i * ySpace, 100, 20), playerInputBuffer.inputList[i].buffer[j].hold.ToString());
            }
        }
    }
}
