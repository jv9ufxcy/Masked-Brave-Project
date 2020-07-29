using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    private HealthManager enemyHealth;
    public static EnemySpawn GetClosestEnemy(Vector3 position, float maxRange)
    {
        EnemySpawn closest = null;
        foreach (EnemySpawn enemy in enemyList)
        {
            if (enemy != null)
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
        enemyHealth = GetComponent<HealthManager>();
        //gameObject.SetActive(false);
    }

    public void Spawn()
    {
        gameObject.SetActive(true);
        transform.SetParent(null);
    }
    public bool IsAlive()
    {
        return !enemyHealth.IsDead;
    }
}
