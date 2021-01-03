using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public Transform crateContainer;
    public EnemySpawn[] crates;
    public List<EnemySpawn> soloCrates = new List<EnemySpawn>();
    void Start()
    {
        crates = crateContainer.GetComponentsInChildren<EnemySpawn>();
        StartCoroutine(SetPlayerCoRoutine());
    }
    private IEnumerator SetPlayerCoRoutine()
    {
        yield return new WaitForFixedUpdate();
        SpawnCrates();
    }
    private void SpawnCrates()
    {
        foreach (EnemySpawn e in crates)
        {
            //EnemySpawn e = t.GetComponent<EnemySpawn>();
            //if (e != null)
            //{
                e.Spawn(2);
                soloCrates.Add(e);
            //}
        }
    }
}
