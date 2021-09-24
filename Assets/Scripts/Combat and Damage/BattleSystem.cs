using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;
using System;

public class BattleSystem : MonoBehaviour
{
    public UnityEvent OnBattleStarted;
    public UnityEvent OnBattleEnded;

    [SerializeField] private TextMeshProUGUI numOfEnemies,battleFightText;
    [SerializeField] private RectTransform enemyCounter;
    [SerializeField] private Vector2 restingLocation, startingLocation, midScreen;
    private Image enemyIcon;
    [SerializeField] private string battleStartText="TATAKAE!", battleEndText="FINISH!", battleStartAudio = "SlimePsi", battleEndAudio = "Victory";

    public int activeWaveCount;
    private enum State { Idle, Active, Conclusion}
    [SerializeField] private State battleState;
    [SerializeField] Wave[] waveArray;
    [SerializeField] ColliderTrigger collTrigger;
    private List<Wave> activeWaveList;
    private List<EnemySpawn> enemySpawnList= new List<EnemySpawn>();
    private AudioManager audioManager;
    private MusicManager musicManager;
    public AudioClip battleTheme, stageTheme;
    // Start is called before the first frame update

    void Start()
    {
        battleFightText.DOColor(Color.clear, 0);
        numOfEnemies.color = Color.clear;
        enemyIcon = enemyCounter.GetComponentInChildren<Image>();
        battleState = State.Idle;
        activeWaveList = new List<Wave>();

        audioManager = AudioManager.instance;
        musicManager = MusicManager.instance;
    }
    private int enemyNum()
    {
        return enemySpawnList.Count;
    }
    // Update is called once per frame
    void Update()
    {
        switch (battleState)
        {
            case State.Idle:
                
                break;
            case State.Active:
                foreach (Wave wave in waveArray)
                {
                    wave.timer -= Time.deltaTime;
                    if (wave.timer <= 0f)
                    {
                        if (!wave.alreadySpawned)
                            SpawnWave(wave);
                        wave.alreadySpawned = true;
                    }
                    CheckNumOfEnemies();
                    if (wave.IsWaveOver()) continue;
                    else break;
                    
                }
                enemySpawnList.RemoveAll(e => !e.IsAlive());
                TestBattleOver();
                break;
            default:
                break;
        }
    }

    public void StartBattle()
    {
        if (battleState == State.Idle)
        {
            StartBattleUI();
            audioManager.PlaySound(battleStartAudio);

            //stageTheme = musicManager.BackgroundMusic.clip;
            //musicManager.ChangeBGM(battleTheme);

            battleState = State.Active;
            OnBattleStarted.Invoke();
        }
    }
    private void StartBattleUI()
    {
        battleFightText.text = battleStartText;
        battleFightText.transform.DOScale(8, 0f);
        battleFightText.DOColor(Color.white, 0.25f);
        battleFightText.transform.DOScale(1, .5f);

        enemyCounter.DOAnchorPos(midScreen, 0.5f).SetDelay(1f);
        numOfEnemies.transform.DOScale(2, 0.25f).SetDelay(1f);

        enemyIcon.DOColor(Color.white, 0f).SetDelay(1f);
        numOfEnemies.DOColor(Color.white, 0f).SetDelay(1f);

        enemyCounter.DOAnchorPos(restingLocation, .5f).SetDelay(2f);
        numOfEnemies.transform.DOScale(1, .5f).SetDelay(2f);


        battleFightText.transform.DOScale(8, .25f).SetDelay(3f);
        battleFightText.DOColor(Color.clear, 0.25f).SetDelay(3f);
    }
    private void EndBattleUI()
    {
        battleFightText.text = battleEndText;

        battleFightText.DOColor(Color.white, 0.5f);
        battleFightText.transform.DOScale(1, 0.5f);

        numOfEnemies.transform.DOScale(2f, 1f);
        numOfEnemies.DOColor(Color.clear, 1f);

        enemyIcon.DOColor(Color.clear, 1f);

        battleFightText.transform.DOScale(8, .25f).SetDelay(1f);
        battleFightText.DOColor(Color.clear, 0.25f).SetDelay(1f);

        enemyCounter.DOAnchorPos(startingLocation, 0).SetDelay(1f);
        numOfEnemies.transform.DOScale(1, .5f);
        battleFightText.transform.DOScale(1, 0).SetDelay(1f);
    }
    private void CheckNumOfEnemies()
    {
        foreach (Wave wave in waveArray)
        {
            if (!wave.listAlreadyChecked)
            {
                List<EnemySpawn> waveSpawnEnemyList = new List<EnemySpawn>();
                if (wave.enemySpawnContainer != null)
                {
                    foreach (Transform transform in wave.enemySpawnContainer)
                    {
                        EnemySpawn enemySpawn = transform.GetComponent<EnemySpawn>();
                        if (enemySpawn != null)
                            waveSpawnEnemyList.Add(enemySpawn);
                    }
                }
                if (wave.enemySpawnArray != null)
                    waveSpawnEnemyList.AddRange(wave.enemySpawnArray);

                foreach (EnemySpawn enemySpawn in waveSpawnEnemyList)
                {
                    enemySpawnList.Add(enemySpawn);
                }
            }
            wave.listAlreadyChecked = true;
        }
    }

        private void SpawnWave(Wave wave)
    {
        List<EnemySpawn> waveSpawnEnemyList = new List<EnemySpawn>();
        if (wave.enemySpawnContainer!=null)
        {
            foreach (Transform transform in wave.enemySpawnContainer)
            {
                EnemySpawn enemySpawn = transform.GetComponent<EnemySpawn>();
                if (enemySpawn != null)
                    waveSpawnEnemyList.Add(enemySpawn);
            }
        }
        if (wave.enemySpawnArray != null)
            waveSpawnEnemyList.AddRange(wave.enemySpawnArray);

        foreach (EnemySpawn enemySpawn in waveSpawnEnemyList)
        {
            if (wave.bossBattle)
                enemySpawn.Spawn(1);
            else
                enemySpawn.Spawn(0);
        }
    }
    private void TestBattleOver()
    {
        numOfEnemies.text = "x " + enemyNum();
        if (battleState==State.Active&&IsBattleOver())
        {
            audioManager.PlaySound(battleEndAudio);
            //musicManager.ChangeBGM(stageTheme);
            OnBattleEnded.Invoke();
            enemySpawnList.Clear();
            numOfEnemies.text = "x " + enemyNum();
            EndBattleUI();
            battleState = State.Conclusion;
        }
    }
    private bool IsBattleOver()
    {
        foreach (Wave wave in waveArray)
        {
            if (wave.IsWaveOver())
            {

            }
            else
                return false;
        }
        return true;
        
    }
    [System.Serializable]
    private class Wave
    {
        public Transform enemySpawnContainer;
        public EnemySpawn[] enemySpawnArray;
        public float timer=2f;
        public bool alreadySpawned=false;
        public bool listAlreadyChecked=false;
        public bool bossBattle = false;

        public bool IsWaveOver()
        {
            foreach (EnemySpawn enemySpawn in enemySpawnArray)
            {
                if (enemySpawn.IsAlive())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
