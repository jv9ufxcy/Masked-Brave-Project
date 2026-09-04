using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    public class OnTickEventArgs : EventArgs
    {
        public int tick;
    }
    [SerializeField] private BattleSystem battle;
    [Header("Timer")]
    [SerializeField] private float remainingTime = 15f;
    private float elapsedTime = 0f;
    private bool countDown = false;
    [SerializeField] TextMeshProUGUI timerText;
    private Color timerColor;
    private int tick;
    private float tickTimer;
    private const float tickTimerMax = 1f;
    public static event EventHandler<OnTickEventArgs> OnTick;
    private void OnEnable()
    {
        battle.OnWaveEnd += Battle_OnWaveEnd;
    }
    private void OnDisable()
    {
        battle.OnWaveEnd -= Battle_OnWaveEnd;
    }
    private void Battle_OnWaveEnd()
    {
        AddTime(10);
    }

    // Start is called before the first frame update
    void Start()
    {
        anchorLocation = popupHolder.anchoredPosition;
        popupHolder.DOAnchorPos(hiddenLocation, 0);
    }

    private void Update()
    {
        if (!PauseManager.IsGamePaused&&!DialogueManager.instance.isDialogueActive)
        {
            if (GameEngine.hitStop <= 0)
            {
                if (countDown) Countdown();
            }
            if (isPlaying || PopupQueue.Count <= 0)
            {
                return;
            }
            ExecuteQueue();
        }
    }

    private void ElapsedTimer()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        SetTimerText(minutes, seconds);
    }
    private void Countdown()
    {
        if (remainingTime > 0)
            remainingTime -= Time.deltaTime;
        else if (remainingTime < 0)
        {
            remainingTime = 0;
            timerText.color = Color.red;
            Debug.Log("TIME OVER");
        }
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        SetTimerText(minutes, seconds);
    }
    public void SetRemainingTime(float seconds)
    {
        timerText.gameObject.SetActive(true);
        timerText.color = Color.white;
        remainingTime = seconds;
        countDown = true;
    }
    public void AddTime(float seconds)
    {
        remainingTime += seconds;
        AddToQueue("+ "+ seconds);
    }
    private void SetTimerText(int minutes, int seconds)
    {
        timerText.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
    }
    void FixedUpdate()
    {
        if (!PauseManager.IsGamePaused && !DialogueManager.instance.isDialogueActive)
        {
            if (GameEngine.hitStop <= 0)
            {
                tickTimer += Time.fixedDeltaTime;
                if (tickTimer >= tickTimerMax)
                {
                    tickTimer -= tickTimerMax;
                    tick++;
                    if (OnTick != null)
                    {
                        OnTick(this, new OnTickEventArgs { tick = tick });
                    }
                }
            }
        }
    }
    [Header("Popups")]
    public TextMeshProUGUI AdditionalTimerText;
    public RectTransform popupHolder;

    public Queue<string> PopupQueue = new Queue<string>();

    public float FadeTime = .3f, DisplayTime = 1.5f;
    [SerializeField]private Vector2 anchorLocation, hiddenLocation;
    private bool isPlaying;
    public void AddToQueue(string val)
    {
        PopupQueue.Enqueue(val);
    }
    void ExecuteQueue()
    {
        string val = PopupQueue.Dequeue();
        AdditionalTimerText.text = val;
        StartCoroutine(DisplayQueueText());
    }

    private IEnumerator DisplayQueueText()
    {
        isPlaying = true;
        popupHolder.DOAnchorPos(hiddenLocation, 0);
        popupHolder.DOAnchorPos(anchorLocation, FadeTime);
        yield return new WaitForSeconds(DisplayTime);
        popupHolder.DOAnchorPos(hiddenLocation, FadeTime);
        yield return new WaitForSeconds(FadeTime);
        isPlaying = false;
    }
}
