using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemySpawn[] enemies;
    public Vector2 rand = new Vector2(0, 2);
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SpawnEnemiesInArray()
    {
        foreach (EnemySpawn e in enemies)
        {
            //float x = Random.Range(-rand.y, rand.y);
            //float y = Random.Range(rand.x, rand.y);
            //Vector3 offset = new Vector3(x, y, 0);
            //EnemySpawn newEnemy = Instantiate(e,transform.position+offset,Quaternion.identity);
            e.Spawn();
        }
    }
}
