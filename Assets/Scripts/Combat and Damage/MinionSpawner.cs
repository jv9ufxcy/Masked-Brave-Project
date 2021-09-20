using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    public GameObject minion, greaterMinion;
    public int summonMax = 2, summonID;
    private List<GameObject> summons = new List<GameObject>();
    private float time=300;
    [SerializeField] private float timeToSpawn=300;
    [SerializeField] private RuntimeAnimatorController[] anims;
    private void FixedUpdate()
    {
        if (time>0)
        {
            time--;
        }
        else
        {
            SummonMinion();
        }
    }
    public void UpgradeHouse()
    {
        summonID = 1;
        summons.Clear();
        GetComponentInChildren<Animator>().runtimeAnimatorController = anims[summonID];
    }
    public void SummonMinion()
    {
        time = timeToSpawn;
        if (summons.Count >= summonMax)
        {
            foreach (GameObject sum in summons)
            {
                if (sum.GetComponent<CharacterObject>().controlType != CharacterObject.ControlType.AI)
                {
                    sum.GetComponent<EnemySpawn>().Spawn(0);
                    break;
                }
            }
        }
        else
        {
            GameObject clone;
            if (summonID > 0)
                clone = Instantiate(greaterMinion, transform.position, Quaternion.identity) as GameObject;
            else
                clone = Instantiate(minion, transform.position, Quaternion.identity) as GameObject;

            summons.Add(clone);
            clone.GetComponent<EnemySpawn>().Spawn(0);
        }
    }
    void OnEnable()
    {
        summonID = 0;
        //SummonMinion();
        GetComponentInChildren<Animator>().runtimeAnimatorController = anims[summonID];
    }
    void OnDisable()
    {
        summonID = 0;
        summons.Clear();
        gameObject.SetActive(false);
    }
    public void KillThemAll()
    {
        foreach (GameObject enemy in summons)
        {
            enemy.GetComponent<EnemySpawn>().DeSpawn();
        }
    }
}
