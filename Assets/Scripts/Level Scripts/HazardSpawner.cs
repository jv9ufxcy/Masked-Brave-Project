using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardSpawner : MonoBehaviour
{
    public GameObject hazardToSpawn;
    public InteractableObject spawnedHazard;
    public float cooldown = 3f, maxCooldown = 3f;
    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        cooldown = maxCooldown;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (spawnedHazard != null)
        {
            if (!spawnedHazard.IsSpawned)
            {
                if (cooldown > 0)
                {
                    cooldown -= Time.fixedDeltaTime;
                }
                else
                {
                    SpawnHazard();
                    cooldown = maxCooldown;
                }
            }
        }
        else
        {
            SpawnHazard();
            cooldown = maxCooldown;
        }
    }
    void SpawnHazard()
    {
        if (spawnedHazard != null)
        {
            spawnedHazard.Spawn(2);
        }
        else
        {
            GameObject spawned = Instantiate(hazardToSpawn, transform.position + offset, Quaternion.identity);
            spawnedHazard = spawned.GetComponent<InteractableObject>();
            StartCoroutine(Spawn());
        }
    }
    private IEnumerator Spawn()
    {
        yield return new WaitForFixedUpdate();
        spawnedHazard.Spawn(2);
    }
}
