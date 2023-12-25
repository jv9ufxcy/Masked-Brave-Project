using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardSpawner : MonoBehaviour
{
    public GameObject hazardToSpawn;
    public InteractableObject hazardObject;
    public Crate crateObject;
    public float cooldown = 3f, maxCooldown = 3f;
    public Vector3 offset;
    [SerializeField] private ParticleSystem spawnDust;
    public enum SpawnLogic { hazard,crate};
    [SerializeField] private SpawnLogic _spawnLogic;
    // Start is called before the first frame update
    void Start()
    {
        cooldown = maxCooldown;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (_spawnLogic)
        {
            case SpawnLogic.hazard:
                HazardSpawning();
                break;
            case SpawnLogic.crate:
                CrateSpawning();
                break;
            default:
                break;
        }
    }

    private void CrateSpawning()
    {
        if (crateObject != null)//has the crate object been instantiated
        {
            if (!crateObject.IsSpawned)//is crate InActive
            {
                if (cooldown > 0)
                {
                    cooldown -= Time.fixedDeltaTime;
                    if ((cooldown < 1 && cooldown > 0.95) && spawnDust != null)
                    {
                        spawnDust.Play();
                        //Debug.Log("Particles");
                    }
                }
                else
                {
                    SpawnCrate();
                    cooldown = maxCooldown;
                    spawnDust.Stop();
                }
            }
        }
        else//instantiate crate
        {
            SpawnCrate();
            cooldown = maxCooldown;
            //Debug.Log("Initial Spawn");
        }
    }

    private void HazardSpawning()
    {
        if (hazardObject != null)
        {
            if (!hazardObject.IsSpawned)
            {
                if (cooldown > 0)
                {
                    cooldown -= Time.fixedDeltaTime;
                    if ((cooldown < 1 && cooldown > 0.95) && spawnDust != null)
                    {
                        spawnDust.Play();
                    }
                }
                else
                {
                    SpawnHazard();
                    cooldown = maxCooldown;
                    spawnDust.Stop();
                }
            }
        }
        else//instantiate hazard
        {
            SpawnHazard();
            cooldown = maxCooldown;
            //Debug.Log("Initial Spawn");
        }
    }

    void SpawnHazard()
    {
        if (hazardObject != null)
        {
            hazardObject.Spawn(2);
        }
        else
        {
            GameObject spawned = Instantiate(hazardToSpawn, transform.position + offset, Quaternion.identity);
            hazardObject = spawned.GetComponent<InteractableObject>();
            StartCoroutine(Spawn());
        }
    }
    void SpawnCrate()
    {
        if (crateObject != null)
        {
            crateObject.OnSpawn();
        }
        else
        {
            GameObject spawned = Instantiate(hazardToSpawn, transform.position + offset, Quaternion.identity);
            crateObject = spawned.GetComponent<Crate>();
            StartCoroutine(SpawnACrate());
        }
    }
    private IEnumerator Spawn()
    {
        yield return new WaitForFixedUpdate();
        hazardObject.Spawn(2);
    }
    private IEnumerator SpawnACrate()
    {
        yield return new WaitForFixedUpdate();
        crateObject.OnSpawn();
    }
}
