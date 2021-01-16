using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform enemyContainer;
    public EnemySpawn[] enemies;
    public List<EnemySpawn> soloEnemies = new List<EnemySpawn>();
    public Vector2 rand = new Vector2(0, 2);
    public Transform playerChar;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetPlayerCoRoutine());
    }
    private void FixedUpdate()
    {
        if (playerChar!=null)
        {
            this.transform.position = playerChar.transform.position;
        }
    }
    private IEnumerator SetPlayerCoRoutine()
    {
        yield return new WaitForFixedUpdate();
        SetPlayer();
    }
    public void SetPlayer()
    {
        playerChar = Camera.main.transform;
        AddEnemiesToList();
    }
    private void AddEnemiesToList()
    {
        foreach (Transform transform in enemyContainer)
        {
            EnemySpawn enemySpawn = transform.GetComponent<EnemySpawn>();
            if (enemySpawn != null)
                soloEnemies.Add(enemySpawn);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemySpawn E = collision.gameObject.GetComponent<EnemySpawn>();
            if (soloEnemies.Contains(E)&&!E.IsSpawned)
            {
                SpawnEnemy(E);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemySpawn E = collision.gameObject.GetComponent<EnemySpawn>();
            if (soloEnemies.Contains(E))
            {
                DeSpawnEnemy(E);
            }
        }
    }
    public void SpawnEnemy(EnemySpawn enemy)
    {
        enemy.Spawn(0);
    }
    public void DeSpawnEnemy(EnemySpawn enemy)
    {
        enemy.DeSpawn();
    }
}
