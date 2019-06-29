using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Vector2 lastCheckpointPos;
    [SerializeField] private Transform startingCheckpointPos;
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
    public void RestoreCheckpointStart()
    {
        lastCheckpointPos = startingCheckpointPos.transform.position;
    }
}
