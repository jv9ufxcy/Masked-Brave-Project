using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Door : MonoBehaviour
{
    [SerializeField] private float duration=3, strength=10, randomness=10;
    [SerializeField] private int vibrato = 10;
    [SerializeField] private bool fadeOut;
    [SerializeField] string sceneToLoad;
    GameManager GM;
    private void Start()
    {
        GM = GameManager.instance;
    }
    private void Update()
    {
        transform.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut);
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
