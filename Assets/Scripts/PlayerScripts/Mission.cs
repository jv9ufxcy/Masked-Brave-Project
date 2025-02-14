﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Mission : MonoBehaviour,IDataPersistence
{
    public static Mission instance;
    [Range(0,8)]
    public int MissionIndex = 0;
    [Header("Mission Start Text")]
    [Space]
    public bool isMissionActive = false, isReload = false;
    public TextMeshProUGUI missionStartText, menuTimer;
    public string missionStart, missionComplete;
    public Vector2 midScreen, offScreen;
    public float missionStartSeconds = 6f;
    [Header("Timer")]
    [Space]
    private TimeSpan timePlaying;
    [SerializeField] private string timeCounter = "Time: 00:00.00";
    public float elapsedTime, savedTime;
    public float bestTime, maxTime;
    [Header("Score On Reload")]
    [Space]
    public int savedScore;

    public float enemiesKilled, enemyKillReq;

    public float damageCount, damageTaken, damageTakenMax, retryCount, retryTaken, retryMax;

    public float timeScore, missionScore, enemyScore, damageScore, retryScore, totalScore;
    public string[] scoreText;

    public CharacterObject mainChar;
    //[FMODUnity.EventRef(MigrateTo = "<fieldname>")]
    public FMODUnity.EventReference stageTheme;
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
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isReload)
        {
            RestartMission();
        }
    }
    private void Start()
    {
        timeCounter = "00:00.00";
        isMissionActive = false;
        //Debug.Log("Start");
        StartCoroutine(InitializeCoRoutine());
    }
    private void Update()
    {
        if (!PauseManager.IsGamePaused && !DialogueManager.instance.isDialogueActive)
        {
            if (GameEngine.hitStop <= 0)
            {
                MultiKillUpdate();
            }
        }  
    }
    private IEnumerator InitializeCoRoutine()
    {
        yield return new WaitForFixedUpdate();
        Initialize();
    }

    private void RestartMission()
    {
        StartCoroutine(InitializeCoRoutine());
        elapsedTime = savedTime;
        StartCoroutine(DelayedScoreReset());
        BeginTimer();
        mainChar.controlType = CharacterObject.ControlType.PLAYER;
        MusicManager.instance.StartBGM(stageTheme);
        Debug.Log("Mission Restarted");
    }
    private IEnumerator DelayedScoreReset()
    {
        yield return new WaitForFixedUpdate();
        MissionPoints = savedScore;
    }
    private void Initialize()
    {
        mainChar = GameEngine.gameEngine.mainCharacter;
        menuTimer = PauseManager.pauseManager.pointsText[3];
        currencyText = PauseManager.pauseManager.pointsText[4];
        missionStartText = PauseManager.pauseManager.pointsText[5];
        scoreMultiplicandText = PauseManager.pauseManager.pointsText[6];
        scoreMultiplierText = PauseManager.pauseManager.pointsText[7];
        totalScoreText = PauseManager.pauseManager.pointsText[8];
        scoreRectTransform = scoreMultiplierText.transform.parent.GetComponent<RectTransform>();
        kudosHandler = scoreRectTransform.GetComponentInChildren<KudosHandler>();
        scorePos = scoreRectTransform.anchoredPosition;
        hiddenPos = new Vector3(-128, 0, 0) + scorePos;
        scoreRectTransform.DOAnchorPos(hiddenPos, 0);
        ScoreMultiplier = 1;
        //Debug.Log("Initialize");
        if (!isReload)
            mainChar.controlType = CharacterObject.ControlType.OBJECT;
    }
    [SerializeField] private int henshinState = 37;
    private IEnumerator MissionStart()
    {
        missionStartText.rectTransform.DOAnchorPos(offScreen, 0);
        missionStartText.text = missionStart;
        mainChar.controlType = CharacterObject.ControlType.OBJECT;
        yield return new WaitForSeconds(missionStartSeconds / 2);
        mainChar.StartStateFromScript(henshinState);

        yield return new WaitForSeconds(missionStartSeconds / 2);
        missionStartText.DOColor(Color.white, missionStartSeconds / 4);
        missionStartText.rectTransform.DOAnchorPos(midScreen, missionStartSeconds / 4);
        yield return new WaitForSeconds(missionStartSeconds / 2);
        missionStartText.transform.DOScale(8, .25f);
        missionStartText.DOColor(Color.clear, 0.25f);

        mainChar.controlType = CharacterObject.ControlType.PLAYER;
        BeginTimer();
        MusicManager.instance.StartBGM(stageTheme);
    }

    public void HumanForm()
    {
        mainChar.QuickChangeForm(4);
        mainChar.controlType = CharacterObject.ControlType.OBJECT;
    }
    public void StartMission()
    {
        if (isReload)
            RestartMission();
        else
            StartCoroutine(MissionStart());
    }
    public void EndMission()
    {
        EndTimer();
        MusicManager.instance.StopReverbZone();
        GameEngine.gameEngine.mainCharacter.controlType = CharacterObject.ControlType.OBJECT;
    }
    public void BeginTimer()
    {
        //elapsedTime = 0f;
        isMissionActive = true;
        StartCoroutine(UpdateTimer());
    }
    public void EndTimer()
    {
        //CheckpointTimer();
        MusicManager.instance.StopMusic();
        isMissionActive = false;
    }

    public void CheckpointTime()
    {
        savedTime = elapsedTime;
        savedScore = MissionPoints;
    }

    private IEnumerator UpdateTimer()
    {
        while (isMissionActive)
        {
            if (!PauseManager.IsGamePaused&&!DialogueManager.instance.isDialogueActive)
            {
                elapsedTime += Time.deltaTime;
                timePlaying = TimeSpan.FromSeconds(elapsedTime);
                var output = $"{(int)timePlaying.TotalMinutes}:{timePlaying.Seconds:00}";
                //string timePlayingStr = timePlaying.ToString("mm' : 'ss'.'ff");
                timeCounter = output;
                menuTimer.text = timeCounter;
            }
            yield return null;
        }
    }
    public int currency;
    private KudosHandler kudosHandler;
    private RectTransform scoreRectTransform;
    private TextMeshProUGUI currencyText, scoreMultiplicandText, scoreMultiplierText, totalScoreText;
    public void ChangeCurrency(int val)
    {
        currency += val;
        if (currencyText != null)
            currencyText.text = "x " + currency.ToString();
    }
    public float maxScoremultiplier = 99.9f;
    public float _scoreMultiplier = 1, multiKillTimer, maxMKTime = 1f;
    private int _scoreMultiplicand, multiKill, killStreakBonus = 150;
    public int ScoreMultiplicand
    {
        get { return _scoreMultiplicand; }
        set
        {
            UpdateScoreText(value);
            _scoreMultiplicand = value;
        }
    }
    public float ScoreMultiplier
    {
        get { return _scoreMultiplier; }
        set
        {
            _scoreMultiplier = Mathf.Round(value * 10) / 10;
            scoreMultiplierText.SetText("x " + _scoreMultiplier.ToString());
            Math.Round(_scoreMultiplier, 1);
        }
    }
    public void OnEnemyDamaged(int damage)
    {
        if (damage > 0) IncreaseScore(damage);
        if (ScoreActive == false)
        {
            strikeCounter = maxStrikeCounter;
            kudosHandler.UpdateStrike(strikeCounter);
        }
    }
    public void OnEnemyKilled(float killMultiplier)
    {
        float killPoint = 100;
        killPoint *= killMultiplier;

        enemiesKilled++;
        ScoreMultiplier += 0.1f;
        Mathf.Clamp(ScoreMultiplier, 1, maxScoremultiplier);
        //Quick Chain Kills
        OnChainKill();
        //Multi - Killstreak
        if (killMultiplier == 3f || killMultiplier==300)//skill kill or boss skill kill
        {
            MultiKillInit();
            //int finisherBonus = 50;
            //ScoreMultiplicand += finisherBonus;
            //PopUpTextQueue("Z-Finisher\n<color=#FCE945>+" + finisherBonus);
        }
        OnMissionPoint(Mathf.RoundToInt(killPoint));
    }
    private float chainKillTimer, maxChainTime = 5;
    private int chainCount, maxChainCount = 5;
    private void OnChainKill()
    {
        if (chainKillTimer>0)
        {
            chainCount++;
            int chainBonus = 20;
            chainBonus *= Mathf.Min(5,chainCount);
            IncreaseScore(chainBonus);
            PopUpTextQueue("Quick Kill-"+chainCount+" \n<color=#FCE945>+" + chainBonus);
            chainKillTimer = maxChainTime;
        }
        else
        {
            chainCount = 1;
            chainKillTimer = maxChainTime;
        }
    }
    private void MultiKillInit()
    {
        multiKillTimer = maxMKTime;
        multiKill++;
    }
    private void MultiKillUpdate()
    {
        if (chainKillTimer > 0)
        {
            chainKillTimer -= Time.deltaTime;
            kudosHandler.UpdateTimer(chainKillTimer / maxChainTime);
        }
        if (multiKill > 0)
        {
            if (multiKillTimer > 0)
            {
                multiKillTimer -= Time.deltaTime;
            }
            else
            {
                if (multiKill>=2)
                {
                    int finisherBonus=50;
                    finisherBonus *= multiKill;
                    PopUpTextQueue("Z-Finish\n<color=#FCE945>+" + finisherBonus);
                    IncreaseScore(finisherBonus);

                    ScoreMultiplier += multiKill;
                    multiKillTimer = 0;
                    OnSkillUsed();
                    multiKill = 0;
                }
                else
                {
                    int finisherBonus = 50;
                    finisherBonus *= multiKill;
                    PopUpTextQueue("Z-Finish\n<color=#FCE945>+" + finisherBonus);
                    IncreaseScore(finisherBonus);
                    multiKillTimer = 0;
                    OnSkillUsed();
                    multiKill = 0;
                }
            }
        }
    }
    private void PopUpTextQueue(string textToDisplay)
    {
        PopupManager.instance.AddToQueue(textToDisplay);
    }
    private void IncreaseScore(int points)
    {
        ScoreMultiplicand += points;
        ScoreMultiplier += points / 100;
        //print score
    }

    private Coroutine CountingCoroutine;
    private void UpdateScoreText(int value)
    {
        if(CountingCoroutine!=null) { StopCoroutine(CountingCoroutine); }
        CountingCoroutine = StartCoroutine(CountText(value));
        ScoreActive = true;
    }
    public float Duration = 1f, CountFPS = 60f;
    public string NumberFormat = "N0", DecimalFormat = "D8", PercentFormat = "P1";
    private bool _scoreActive = false;
    public Vector3 scorePos, hiddenPos;
    public bool ScoreActive
    { 
        get => _scoreActive;
        set
        {
            _scoreActive = value;
            ScoreVisible(value);
        }
    }
    [Header("Score")]
    [Space]
    [SerializeField] private int missionPoints;
    public int MissionPoints
    {
        get => missionPoints;
        set
        { 
            missionPoints = value; 
            totalScoreText.SetText(missionPoints.ToString(DecimalFormat)); 
        }
    }


    void ScoreVisible(bool isVisible)
    {
        if (isVisible)
        {
            scoreRectTransform.DOAnchorPos(scorePos, .25f);
            kudosHandler.SetContainerColor(0);
        }
        else
            scoreRectTransform.DOAnchorPos(hiddenPos, .25f).SetDelay(2f);
    }
    private IEnumerator CountText(int newVal)
    {
        WaitForSeconds Wait = new WaitForSeconds(1 / CountFPS);
        int prevVal = _scoreMultiplicand;
        int stepAmt;

        if (newVal - prevVal < 0)
        {
            stepAmt = Mathf.FloorToInt((newVal - prevVal) / (CountFPS * Duration));
        }
        else
        {
            stepAmt = Mathf.CeilToInt((newVal - prevVal) / (CountFPS * Duration));
        }

        if (prevVal < newVal)
        {
            while (prevVal < newVal)
            {
                prevVal += stepAmt;
                if (prevVal > newVal)
                {
                    prevVal = newVal;
                }
                scoreMultiplicandText.SetText(prevVal.ToString(NumberFormat));
                yield return Wait;
            }
        }
        else
        {
            while (prevVal > newVal)
            {
                prevVal += stepAmt;
                if (prevVal < newVal)
                {
                    prevVal = newVal;
                }
                scoreMultiplicandText.SetText(prevVal.ToString(NumberFormat));
                yield return Wait;
            }
        }
    }
    public void CompleteScore()
    {
        int score;
        float multiplicand = ScoreMultiplicand;
        score = Mathf.RoundToInt(multiplicand * ScoreMultiplier);
        OnMissionPoint(score);

        kudosHandler.SetContainerColor(2);
        //play good sound
        DefaultScore();
    }
    public void FailScore()
    {
        kudosHandler.SetContainerColor(1);
        //play fail sound
        DefaultScore();
    }
    private void DefaultScore()
    {
        ScoreMultiplicand = 0;
        ScoreMultiplier = 1;
        ScoreActive = false;

        chainKillTimer = 0;
        chainCount = 0;

        strikeCounter = maxStrikeCounter;
        kudosHandler.UpdateStrike(strikeCounter);
        kudosHandler.UpdateTimer(chainKillTimer);
    }
    private int strikeCounter = 3, maxStrikeCounter = 3;
    public void OnPlayerDamaged(int damage) 
    {
        damageTaken -= damage;
        damageCount += damage;

        if (damage > 0 && ScoreActive)
        {
            HandleFailureStrike();
        }
    }

    private void HandleFailureStrike()
    {
        if (strikeCounter > 0)
        {
            strikeCounter--;
            kudosHandler.UpdateStrike(strikeCounter);
        }
        if (strikeCounter <= 0)
        {
            FailScore();
        }
    }
    public void OnSkillUsed()
    {
        if(ScoreActive)CompleteScore();
    }
    public void OnPlayerContinue() 
    {
        retryTaken--;
        retryCount++;
        isReload = true;
    }
    public void OnMissionPoint(int point) 
    {
        MissionPoints += point;
        
        //Mathf.Clamp(missionPoints, 0, missionPointMax);
    }
    private void CalculateScore()
    {
        CompleteScore();
        timeScore = TimeGrade(elapsedTime, bestTime,2);
        timeScore = Mathf.Clamp(timeScore, 0.5f, 2);
        var output = $"{(int)timePlaying.TotalMinutes}:{timePlaying.Seconds:00}";
        scoreText[0] = output.ToString();
        
        scoreText[1] = "x"+timeScore.ToString();

        //scoreText[2] = MissionPoints.ToString();

        totalScore = (Mathf.Round(timeScore * MissionPoints));
        scoreText[3] = totalScore.ToString();
        Ranking();
        scoreText[5] = missionGrade;
    }
    [System.Serializable]
    public struct Rankings
    {
        public int Score;
        public string Rank;
        public Rankings(int s,string r)
        {
            Score = s;
            Rank = r;
        }
    }
    [SerializeField]
    private Rankings[] rankingGrades = new Rankings[7] { new Rankings(150000, "Z"), new Rankings(100000, "S"), new Rankings(80000, "A"), new Rankings(60000, "B"), new Rankings(30000, "C"), new Rankings(150000, "D"), new Rankings(0, "F") };
    private string missionGrade;
    private void Ranking()
    {
        for (int i = 0; i < rankingGrades.Length; i++)
        {
            if (totalScore> rankingGrades[i].Score)
            {
                missionGrade = rankingGrades[i].Rank;
                break;
            }
        }
    }
    private void CalculateGrade()
    {
        timeScore = TimeGrade(elapsedTime, bestTime,20f);
        scoreText[0] = timeScore.ToString();

        //missionScore = Grade(MissionPoints, missionPointMax);
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
    public int TimeGrade(float elapsed, float best, float multiplier)
    {
        float score = Mathf.Clamp(elapsed, best, maxTime);
        float result = (score-best)/(maxTime-best);
        float grade = multiplier - (multiplier*result);
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
        EndMission();
        StartCoroutine(MissionComplete());
    }
    public UnityEvent OnMissionComplete;
    private void UnlockLevels()
    {
        GameEngine.gameEngine.UnlockLevel(3);
    }
    private IEnumerator MissionComplete()
    {
        OnMissionComplete.Invoke();
        NeutralState();
        missionStartText = PauseManager.pauseManager.battleUI;
        menuTimer = PauseManager.pauseManager.menuTimer;
        CalculateScore();
        //CalculateGrade();
        GameManager.instance.RestoreCheckpointStart();
        yield return new WaitForSeconds(missionStartSeconds);

        missionStartText.transform.DOScale(1, 0);
        missionStartText.rectTransform.DOAnchorPos(offScreen, 0);
        missionStartText.text = missionComplete;

        yield return new WaitForSeconds(missionStartSeconds / 2);
        missionStartText.DOColor(Color.white, missionStartSeconds / 4);
        missionStartText.rectTransform.DOAnchorPos(midScreen, missionStartSeconds / 4);
        yield return new WaitForSeconds(missionStartSeconds / 2);
        missionStartText.transform.DOScale(2, .25f);
        missionStartText.DOColor(Color.clear, 0.25f);

        yield return new WaitForSeconds(missionStartSeconds);
        //GameEngine.gameEngine.mainCharacter.controlType = CharacterObject.ControlType.PLAYER;
        PauseManager.pauseManager.Results();
        LevelChange();
    }

    private static void NeutralState()
    {
        CharacterObject mChar = GameEngine.gameEngine.mainCharacter;
        mChar.controlType = CharacterObject.ControlType.OBJECT;
        mChar.moveSpeed = 0;
        mChar.StartStateFromScript(0);
        mChar.CutsceneUpdatePhysics();
    }
    [Header("Next Level")]
    [SerializeField] private string nextLevel = "LevelSelectScene";
    [SerializeField] private string trainingLevel = "BombardierTraining", unlockedFormState = "Zoe/BombHenshin";
    [SerializeField] private bool shouldCheckTrainingStage = false;
    private bool formUnlockedHere = true;
    

    private void LevelChange()
    {
        if (shouldCheckTrainingStage)
        {
            if (formUnlockedHere)
                SceneTransitionController.instance.LoadScene(trainingLevel);
            else
                SceneTransitionController.instance.LoadScene(nextLevel);
        }
        else
            SceneTransitionController.instance.LoadScene(nextLevel);
        if (EnemySpawner.spawnerInstance!=null)
            SceneManager.MoveGameObjectToScene(EnemySpawner.spawnerInstance.gameObject, SceneManager.GetActiveScene());

        SceneManager.MoveGameObjectToScene(instance.gameObject, SceneManager.GetActiveScene());
        SceneManager.MoveGameObjectToScene(GameManager.instance.gameObject, SceneManager.GetActiveScene());
    }

    public void LoadData(GameData data)
    {
        if (data.unlockedSkillsData.Contains(unlockedFormState))
        {
            formUnlockedHere = false;
        }
        else
            formUnlockedHere = true;
    }

    public void SaveData(GameData data)
    {
        if (data.missionScoreIndex[MissionIndex]<totalScore)
        {
            data.missionScoreIndex[MissionIndex] = (int)this.totalScore;
            data.missionGrade[MissionIndex] = this.missionGrade;
        }
    }
}
