using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    GameManager GM;
    [SerializeField]
    string sceneToLoad;
    private void Start()
    {
        GM = GameManager.instance;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GM.RestoreCheckpointStart();
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
