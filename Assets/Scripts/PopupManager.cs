using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using DG.Tweening;

public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;
    public TextMeshProUGUI Text;
    public RectTransform popupHolder;

    public Queue<string> PopupQueue = new Queue<string>();

    public float FadeTime = .3f, DisplayTime = 1.5f;
    private Vector2 anchorLocation, hiddenLocation;
    private bool isPlaying;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        anchorLocation = popupHolder.anchoredPosition;
        hiddenLocation = anchorLocation - new Vector2(Screen.width,0);
        popupHolder.DOAnchorPos(hiddenLocation,0);
    }
    public void AddToQueue(string val)
    {
        PopupQueue.Enqueue(val);
    }
    void ExecuteQueue()
    {
        var val = PopupQueue.Dequeue();
        Text.text = val;
        StartCoroutine(DisplayQueueText());
    }

    private IEnumerator DisplayQueueText()
    {
        isPlaying = true;
        popupHolder.DOAnchorPos(hiddenLocation, 0);
        popupHolder.DOAnchorPos(anchorLocation ,FadeTime);
        yield return new WaitForSeconds(DisplayTime);
        popupHolder.DOAnchorPos(hiddenLocation, FadeTime);
        yield return new WaitForSeconds(DisplayTime);
        isPlaying = false;
    }
    private void Update()
    {
        if (isPlaying||PopupQueue.Count<=0)
        {
            return;
        }
        ExecuteQueue();
    }
}
