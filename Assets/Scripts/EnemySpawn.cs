using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    private EnemyHealthManager enemyHP;
    public bool isHit;
    // Start is called before the first frame update
    void Start()
    {
        enemyHP=GetComponent<EnemyHealthManager>();
        gameObject.SetActive(false);
    }

    public void Spawn()
    {
        gameObject.SetActive(true);
        transform.SetParent(null);
    }
    public bool IsAlive()
    {
        return !enemyHP.IsEnemyDead();
    }
}
