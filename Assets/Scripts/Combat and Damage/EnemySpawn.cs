using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public LayerMask defaultLayer, hiddenLayer;
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
        //if (defaultLayer == 0)
        //{
        //    defaultLayer = gameObject.layer;
        //}
        if (!IsSpawned && !(character.controlType==CharacterObject.ControlType.PLAYER || character.controlType == CharacterObject.ControlType.OBJECT))
        {
            HideCharacter();
        }
        
        spawnPos = transform.position;
    }

    private void HideCharacter()
    {
        character.character.gameObject.SetActive(false);
        gameObject.layer = (int)Mathf.Log(hiddenLayer.value, 2);
    }
    private void ShowCharacter()
    {
        character.character.gameObject.SetActive(true);
        gameObject.layer = (int)Mathf.Log(defaultLayer.value, 2);
    }
    public void ShowCharacterCutscene()
    {
        character.character.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!IsAlive()&&IsSpawned==true)
        {
            IsSpawned = false;
        }
    }
    public void Spawn(int type)
    {
        ShowCharacter();

        if (enemyHealth==null)
            enemyHealth = GetComponent<HealthManager>();
        if (character==null)
            character = GetComponent<CharacterObject>();
        enemyHealth.SetMaxHealth();
        enemyHealth.IsDead=false;
        IsSpawned = true;

        switch (type)
        {
            case 0://Enemy
                character.OnEnemySpawn();
                break;
            case 1://Boss
                character.OnBossSpawn();
                enemyHealth.ShowHealth();
                break;
            case 2://Crate
                character.OnObjectSpawn();
                break;
            case 3:
                break;
        }
        transform.SetParent(null);
        transform.position = spawnPos;
    }
    public void DeSpawn()
    {
        character.OnObjectSpawn();
        HideCharacter();
        enemyHealth.SetMaxHealth();
        enemyHealth.IsDead = true;
        IsSpawned = false;
        //transform.SetParent(null);
        transform.position = spawnPos;
    }
    public bool IsAlive()
    {
        return !enemyHealth.IsDead;
    }
}
