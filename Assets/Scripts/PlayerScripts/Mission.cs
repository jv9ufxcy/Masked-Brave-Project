using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

public class Mission : MonoBehaviour
{
    public static Mission instance;

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

    public int missionPointMax;

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
        KillStreakUpdate();
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
        mainChar.controlType = CharacterObject.ControlType.PLAYER;
        BeginTimer();
        MusicManager.instance.StartBGM(stageTheme);
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
    }
    private IEnumerator MissionStart()
    {
        missionStartText.rectTransform.DOAnchorPos(offScreen, 0);
        missionStartText.text = missionStart;
        mainChar.controlType = CharacterObject.ControlType.OBJECT;
        yield return new WaitForSeconds(missionStartSeconds / 2);
        mainChar.StartStateFromScript(37);

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
    }
    private void BeginTimer()
    {
        //elapsedTime = 0f;
        isMissionActive = true;
        StartCoroutine(UpdateTimer());
    }
    private void EndTimer()
    {
        savedTime = elapsedTime;
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
    public float _scoreMultiplier = 1, killStreakTimer, maxKillStreakTime = 1f;
    private int _scoreMultiplicand, killStreak, killStreakBonus = 150;
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
        if (damage > 0) ScoreMultiplicand += damage;
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
        KillStreakBegin();
        if (killMultiplier >= 3f)
        {
            int finisherBonus = 50;
            ScoreMultiplicand += finisherBonus;
            PopUpTextQueue("Z-Finisher\n<color=#FCE945>+" + finisherBonus);
        }
        OnMissionPoint(Mathf.RoundToInt(killPoint));
    }
    private float chainKillTimer, maxChainTime = 5;
    private void OnChainKill()
    {
        if (chainKillTimer>0)
        {
            int chainBonus = 30;
            ScoreMultiplicand += chainBonus;
            PopUpTextQueue("Quick Kill\n<color=#FCE945>+" + chainBonus);
            chainKillTimer = maxChainTime;
        }
        else
        {
            chainKillTimer = maxChainTime;
        }
    }
    private void KillStreakBegin()
    {
        killStreakTimer = maxKillStreakTime;
        killStreak++;
    }
    private void KillStreakUpdate()
    {
        if (chainKillTimer > 0)
        {
            chainKillTimer -= Time.deltaTime;
        }
        if (killStreak > 0)
        {
            if (killStreakTimer > 0)
            {
                killStreakTimer -= Time.deltaTime;
            }
            else
            {
                if (killStreak>=2)
                {
                    int bonus = 100;
                    switch (Mathf.Clamp(killStreak,0,8))
                    {
                        case 2://double kill
                            bonus = 100;
                            PopUpTextQueue("Z-Double\n<color=#FCE945>+" + bonus);
                            break;
                        case 3://triple
                            bonus = 250;
                            PopUpTextQueue("Z-Triple\n<color=#FCE945>+" + bonus);
                            break;
                        case 4://quad
                            bonus = 400;
                            PopUpTextQueue("Z-Quadruple\n<color=#FCE945>+" + bonus);
                            break;
                        case 5://quint
                            bonus = 550;
                            PopUpTextQueue("<i>555</i>\n<color=#FCE945>+" + bonus);
                            break;
                        case 6://sex
                            bonus = 600;
                            PopUpTextQueue("<i>666</i>\n<color=#FCE945>+" + bonus);
                            break;
                        case 7://hept
                            bonus = 750;
                            PopUpTextQueue("<i>777</i>\n<color=#FCE945>+" + bonus);
                            break;
                        case 8://carnage
                            bonus = 900;
                            PopUpTextQueue("<i>CRITICAL DEAD</i>\n<color=#FCE945>+" + bonus);
                            break;
                        case 9:
                            bonus = 900;
                            PopUpTextQueue("<i>CRITICAL DEAD</i>\n<color=#FCE945>+" + bonus);
                            break;
                        default:
                            bonus = 100;
                            PopUpTextQueue("Double\n<color=#FCE945>+" + bonus);
                            break;
                    }
                    IncreaseScore(bonus);
                    //notify double triple etc
                    ScoreMultiplier += killStreak;
                    killStreakTimer = 0;
                    killStreak = 0;
                }
                else
                {
                    killStreakTimer = 0;
                    killStreak = 0;
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
        ScoreMultiplicand = 0;
        ScoreMultiplier = 1;
        ScoreActive = false;

        strikeCounter = maxStrikeCounter;
        kudosHandler.UpdateStrike(strikeCounter);
    }
    public void FailScore()
    {
        kudosHandler.SetContainerColor(1);
        //play fail sound
        ScoreMultiplicand = 0;
        ScoreMultiplier = 1;
        ScoreActive = false;

        strikeCounter = maxStrikeCounter;
        kudosHandler.UpdateStrike(strikeCounter);
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
        scoreText[0] = "x"+timeScore.ToString();

        scoreText[1] = MissionPoints.ToString();

        totalScore = (Mathf.Round(timeScore * MissionPoints));
        scoreText[5] = totalScore.ToString();
    }
    private void CalculateGrade()
    {
        timeScore = TimeGrade(elapsedTime, bestTime,20f);
        scoreText[0] = timeScore.ToString();

        missionScore = Grade(MissionPoints, missionPointMax);
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

    private IEnumerator MissionComplete()
    {
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

        PauseManager.pauseManager.Results();
        LevelChange();
    }
    [SerializeField] private string nextLevel = "MainMenu";
    private int missionPoints;

    private void LevelChange()
    {
        SceneTransitionController.instance.LoadScene(nextLevel);
        Destroy(gameObject);
        Destroy(GameManager.instance.gameObject);
    }
}
