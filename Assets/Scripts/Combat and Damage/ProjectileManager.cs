using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private bool isLeftEnabled, isRightEnabled, isTopEnabled, isBottomEnabled;

    [SerializeField] private GameObject[] projectilePrefab;
    [SerializeField] private Vector2 launchForce;

    [SerializeField] private Vector3[] bottomSpawnPoints;
    [SerializeField] private float projectileIntervalBottom = 5f;
    private float projectileSpawnTimerBottom = 0f;

    [SerializeField] private Vector3[] leftSpawnPoints;
    [SerializeField] private float projectileIntervalLeft = 5f;
    private float projectileSpawnTimerLeft = 0f;

    [SerializeField] private Vector3[] topSpawnPoints;
    [SerializeField] private float projectileSpawnIntervalTop = 5f;
    private float projectileSpawnTimerTop = 0f;

    [SerializeField] private Vector3[] rightSpawnPoints;
    [SerializeField] private float projectileSpawnIntervalRight = 5f;
    private float projectileSpawnTimerRight = 0f;

    Rigidbody2D projectileRB;
    private int randomInt;
    // Use this for initialization
    void Start ()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //bottom timer
        if (isBottomEnabled)
            SpawnProjectilesFromBottom();
        //left timer
        if(isLeftEnabled)
            SpawnProjectilesFromLeft();
        //top timer
        if (isTopEnabled)
            SpawnProjectilesFromTop();
        //right timer
        if (isRightEnabled)
            SpawnProjectilesFromRight();
    }

    private void SpawnProjectilesFromRight()
    {
        projectileSpawnTimerRight += Time.deltaTime;
        if (projectileSpawnTimerRight >= projectileSpawnIntervalRight)
        {
            projectileSpawnTimerRight -= projectileSpawnIntervalRight;
            SpawnProjectileRight(projectilePrefab[randomInt], 1);
        }
    }

    private void SpawnProjectilesFromTop()
    {
        projectileSpawnTimerTop += Time.deltaTime;
        if (projectileSpawnTimerTop >= projectileSpawnIntervalTop)
        {
            projectileSpawnTimerTop -= projectileSpawnIntervalTop;
            SpawnProjectileTop(projectilePrefab[randomInt], 1);
        }
    }

    private void SpawnProjectilesFromLeft()
    {
        projectileSpawnTimerLeft += Time.deltaTime;
        if (projectileSpawnTimerLeft >= projectileIntervalLeft)
        {
            projectileSpawnTimerLeft -= projectileIntervalLeft;
            SpawnProjectileLeft(projectilePrefab[randomInt], 1);
        }
    }

    private void SpawnProjectilesFromBottom()
    {
        projectileSpawnTimerBottom += Time.deltaTime;
        randomInt = UnityEngine.Random.Range(0, projectilePrefab.Length);
        if (projectileSpawnTimerBottom >= projectileIntervalBottom)
        {
            projectileSpawnTimerBottom -= projectileIntervalBottom;
            SpawnProjectileBottom(projectilePrefab[randomInt], 1);
        }
    }

    public void SpawnProjectileBottom(GameObject foodToSpawn, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = bottomSpawnPoints[bottomSpawnPoints.Length-1];/*[UnityEngine.Random.Range(0, bottomSpawnPoints.Length)]*/
            GameObject foodInstance = Instantiate(foodToSpawn, spawnPosition, Quaternion.identity);
            projectileRB = foodInstance.GetComponent<Rigidbody2D>();
            projectileRB.velocity = new Vector2(0, launchForce.y);

        }
    }
    public void SpawnProjectileLeft(GameObject projectileToSpawn, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = leftSpawnPoints[UnityEngine.Random.Range(0, leftSpawnPoints.Length)];
            GameObject projectileInstance = Instantiate(projectileToSpawn, spawnPosition, Quaternion.identity);
            projectileRB = projectileInstance.GetComponent<Rigidbody2D>();
            projectileRB.velocity = new Vector2(launchForce.x,0);
        }
    }
    public void SpawnProjectileTop(GameObject foodToSpawn, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = topSpawnPoints[UnityEngine.Random.Range(0, topSpawnPoints.Length)];
            GameObject foodInstance = Instantiate(foodToSpawn, spawnPosition, Quaternion.identity);
            projectileRB = foodInstance.GetComponent<Rigidbody2D>();
            projectileRB.velocity = new Vector2(0, -launchForce.y);

        }
    }
    public void SpawnProjectileRight(GameObject foodToSpawn, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = rightSpawnPoints[UnityEngine.Random.Range(0, leftSpawnPoints.Length)];
            GameObject foodInstance = Instantiate(foodToSpawn, spawnPosition, Quaternion.identity);
            projectileRB = foodInstance.GetComponent<Rigidbody2D>();
            projectileRB.velocity = new Vector2(-launchForce.x, 0);
        }
    }
}
