using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    private HealthManager enemyHealth;
    private CharacterObject character;
    public bool IsSpawned;
    private Vector3 spawnPos;
    public static EnemySpawn GetClosestEnemy(Vector3 position, float maxRange)
    {
        EnemySpawn closest = null;
        foreach (EnemySpawn enemy in enemyList)
        {
            if (enemy != null&&enemy.IsAlive()&&enemy.IsSpawned)
            {
                if (Vector3.Distance(position, enemy.transform.position) <= maxRange)
                {
                    if (closest == null)
                    {
                        closest = enemy;
                    }
                    else
                    {
                        if (Vector3.Distance(position, enemy.transform.position) < Vector3.Distance(position, closest.transform.position))
                        {
                            closest = enemy;
                        }
                    }
                }
            }
        }
        return closest;
    }
    private static List<EnemySpawn> enemyList = new List<EnemySpawn>();
    private void Awake()
    {
        enemyList.Add(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterObject>();
        enemyHealth = GetComponent<HealthManager>();
        if (!IsSpawned && (character.controlType==CharacterObject.ControlType.AI|| character.controlType == CharacterObject.ControlType.BOSS))
        {
            gameObject.SetActive(false);
        }
        
        spawnPos = transform.position;
    }
    private void Update()
    {
        if (!IsAlive()&&IsSpawned==true)
        {
            IsSpawned = false;
        }
    }
    public void Spawn()
    {
        gameObject.SetActive(true);

        if (enemyHealth==null)
            enemyHealth = GetComponent<HealthManager>();
        if (character==null)
            character = GetComponent<CharacterObject>();
        character.OnSpawn();
        enemyHealth.SetMaxHealth();
        enemyHealth.IsDead=false;
        IsSpawned = true;
        transform.SetParent(null);
        transform.position = spawnPos;
    }
    public bool IsAlive()
    {
        return !enemyHealth.IsDead;
    }
}
