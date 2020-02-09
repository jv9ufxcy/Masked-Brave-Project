using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleSystem : MonoBehaviour
{
    public UnityEvent OnBattleStarted;
    public UnityEvent OnBattleEnded;

    private enum State { Idle, Active, Conclusion}
    private State battleState;
    [SerializeField] Wave[] waveArray;
    [SerializeField] ColliderTrigger collTrigger;
    // Start is called before the first frame update
    void Start()
    {
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
                    wave.Update();
                }
                TestBattleOver();
                break;
            default:
                break;
        }
    }

    public void StartBattle()
    {
        if (battleState == State.Idle)
        battleState = State.Active;
        OnBattleStarted.Invoke();
    }

    private void TestBattleOver()
    {
        if (battleState==State.Active&&IsBattleOver())
        {
            battleState = State.Conclusion;
        }
    }
    private bool IsBattleOver()
    {
        foreach (Wave wave in waveArray)
        {
            if (wave.IsWaveOver())
            {
                OnBattleEnded.Invoke();
            }
            else
                return false;
        }
        return true;
    }
    [System.Serializable]
    private class Wave
    {
        [SerializeField] private EnemySpawn[] enemySpawnArray;
        [SerializeField] private float timer;
        public void Update()
        {
            //if (timer>=0)
            //{
            //    timer -= Time.deltaTime;
            //    if (timer <= 0)
                if (IsWaveOver())
                {
                    SpawnEnemies();
                }
            //}
        }
        private void SpawnEnemies()
        {
            foreach (EnemySpawn enemySpawn in enemySpawnArray)
            {
                enemySpawn.Spawn();
            }
        }
        public bool IsWaveOver()
        {
            //if (timer < 0)
            //{
                //wave spawned
                foreach (EnemySpawn enemySpawn in enemySpawnArray)
                {
                    if (enemySpawn.IsAlive())
                    {
                        return false;
                    }
                }
                return true;
            //}
            //else
            //    return false;
        }
    }
}
