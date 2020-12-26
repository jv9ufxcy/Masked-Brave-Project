using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

public class Mission : MonoBehaviour
{
    public static Mission instance;

    [Header("Mission Start Text")]
    [Space]
    public bool isMissionActive = false;
    public TextMeshProUGUI missionStartText, menuTimer;
    public string missionText;
    public Vector2 midScreen, offScreen;
    public float missionStartSeconds = 6f;
    [Header("Timer")]
    [Space]
    private TimeSpan timePlaying;
    [SerializeField]private string timeCounter = "Time: 00:00.00";
    public float elapsedTime;
    public float bestTime, maxTime;

    public float missionPoints, missionPointMax;

    public float enemiesKilled, enemyKillReq;

    public float damageCount, damageTaken, damageTakenMax, retryCount, retryTaken, retryMax;

    public float timeScore, missionScore, enemyScore, damageScore, retryScore, totalScore;
    private string timeScoreText, missionScoreText, enemyScoreText, damageScoreText, retryScoreText, totalScoreText;
    public string[] scoreText;

    public CharacterObject mainChar;
    public AudioClip stageTheme;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        timeCounter = "00:00.00";
        isMissionActive = false;
        
        //StartMission();
    }
    private void FixedUpdate()
    {
        
    }
    private IEnumerator MissionStart()
    {
        yield return new WaitForFixedUpdate();
        mainChar = GameEngine.gameEngine.mainCharacter;
        menuTimer = PauseManager.pauseManager.pointsText[3];
        currencyText = PauseManager.pauseManager.pointsText[4];
        missionStartText = PauseManager.pauseManager.pointsText[5];
        mainChar.controlType = CharacterObject.ControlType.OBJECT;

        missionStartText.rectTransform.DOAnchorPos(offScreen, 0);
        missionStartText.text = missionText;

        mainChar.QuickChangeForm(4);
        mainChar.SetState(37);

        yield return new WaitForSeconds(missionStartSeconds/2);
        missionStartText.DOColor(Color.white, missionStartSeconds / 4);
        missionStartText.rectTransform.DOAnchorPos(midScreen, missionStartSeconds / 4);
        yield return new WaitForSeconds(missionStartSeconds/2);
        missionStartText.transform.DOScale(8, .25f);
        missionStartText.DOColor(Color.clear, 0.25f);

        mainChar.controlType = CharacterObject.ControlType.PLAYER;
        BeginTimer();
        MusicManager.instance.StartBGM(stageTheme);
    }
    public void StartMission()
    {
        StartCoroutine(MissionStart());
    }
    public void EndMission()
    {
        EndTimer();
    }
    private void BeginTimer()
    {
        //elapsedTime = 0f;
        isMissionActive = true;
        StartCoroutine(UpdateTimer());
    }
    private void EndTimer()
    {
        MusicManager.instance.StopMusic();
        isMissionActive = false;
    }
    private IEnumerator UpdateTimer()
    {
        while (isMissionActive)
        {
            if (!PauseManager.IsGamePaused)
            {
                elapsedTime += Time.deltaTime;
                timePlaying = TimeSpan.FromSeconds(elapsedTime);
                string timePlayingStr = /*"Time: " +*/ timePlaying.ToString("mm' : 'ss'.'ff");
                timeCounter = timePlayingStr;
                menuTimer.text = timeCounter;
            }
            yield return null;
        }
    }
    public int currency;
    private TextMeshProUGUI currencyText;
    public void ChangeCurrency(int val)
    {
        currency += val;
        if (currencyText != null)
            currencyText.text = "x " + currency.ToString();
    }
    public void OnEnemyKilled() 
    {
        enemiesKilled++;
    }
    public void OnPlayerDamaged(int damage) 
    {
        damageTaken -= damage;
        damageCount += damage;
    }
    public void OnPlayerContinue() 
    {
        retryTaken--;
        retryCount++;
    }
    public void OnMissionPoint(int point) 
    {
        missionPoints += point;
        Mathf.Clamp(missionPoints, 0, missionPointMax);
    }
    private void CalculateGrade()
    {
        timeScore = TimeGrade(elapsedTime, bestTime);
        scoreText[0] = timeScore.ToString();

        missionScore = Grade(missionPoints, missionPointMax);
        scoreText[1] = missionScore.ToString();

        enemyScore = Grade(enemiesKilled, enemyKillReq);
        scoreText[2] = enemyScore.ToString();

        damageScore = Grade(damageTaken, damageTakenMax);
        scoreText[3] = damageScore.ToString();

        retryScore = Grade(retryTaken, retryMax);
        scoreText[4] = retryScore.ToString();

        totalScore = (timeScore + missionScore + enemyScore + damageScore + retryScore);
        scoreText[5] = totalScore.ToString();
    }
    public int TimeGrade(float elapsed, float best)
    {
        float score = Mathf.Clamp(elapsed, best, maxTime);
        float result = (score-best)/(maxTime-best);
        float grade = 20f - (20f*result);
        return Mathf.RoundToInt(grade);
    }
    public int Grade(float score,float max)
    {
        Mathf.Clamp(score, 0, max);
        float result = ((score / max) * 20f);
        return Mathf.RoundToInt(Mathf.Clamp(result, 0, 20));
    }
    public void EndLevel()
    {
        CalculateGrade();
        GameManager.instance.RestoreCheckpointStart();
        PauseManager.pauseManager.Results();
        StartCoroutine(LevelChange());
    }
    private IEnumerator LevelChange()
    {
        yield return new WaitForSeconds(3f);
        
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }
}
