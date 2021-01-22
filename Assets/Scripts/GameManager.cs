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
}
