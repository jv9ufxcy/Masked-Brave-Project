using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public Transform crateContainer;
    public ISpawnable[] crates;
    public List<ISpawnable> soloCrates = new List<ISpawnable>();
    void Start()
    {
        crates = crateContainer.GetComponentsInChildren<ISpawnable>();
        StartCoroutine(SetPlayerCoRoutine());
    }
    private IEnumerator SetPlayerCoRoutine()
    {
        yield return new WaitForFixedUpdate();
        SpawnCrates();
    }
    private void SpawnCrates()
    {
        foreach (ISpawnable e in crates)
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
