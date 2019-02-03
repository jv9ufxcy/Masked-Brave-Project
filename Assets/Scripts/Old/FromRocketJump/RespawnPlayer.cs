using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnPlayer : MonoBehaviour
{
    private GameManager gm;
    public static event Action PlayerRespawnedFromCheckpoint;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        transform.position = gm.lastCheckpointPos;
    }
    public void Respawn()
    {
        if (Checkpoint.currentlyActiveCheckpoint == null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            transform.position = gm.lastCheckpointPos;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            //if (PlayerRespawnedFromCheckpoint)
            //{

            //}
        }
    }
}
