using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooting : MonoBehaviour
{
    //Bullet
    Rigidbody2D projectileRB;
    [SerializeField] private GameObject[] projectilePrefab;
    [SerializeField] private Vector2 launchForce;

    //Range
    [SerializeField] private LayerMask whatIsShootable;
    [SerializeField] private bool isPlayerInRange;
    [SerializeField] private float detectionRange;

    [SerializeField] private bool isLeftEnabled, isRightEnabled, isTopEnabled, isBottomEnabled;
    //Bottom
    [SerializeField] private float projectileIntervalBottom = 1f;
    private float projectileSpawnTimerBottom = 0f;
    //Left
    [SerializeField] private float projectileIntervalLeft = 1f;
    private float projectileSpawnTimerLeft = 0f;
    //Top
    [SerializeField] private float projectileSpawnIntervalTop = 1f;
    private float projectileSpawnTimerTop = 0f;
    //Right
    [SerializeField] private float projectileSpawnIntervalRight = 1f;
    private float projectileSpawnTimerRight = 0f;

    private int randomInt;

    ObjectPooler objectPooler;
    // Use this for initialization
    void Start()
    {
        objectPooler = ObjectPooler.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        isPlayerInRange = Physics2D.OverlapCircle(transform.position, detectionRange, whatIsShootable);
        if (isPlayerInRange)
        {
            //bottom timer
            if (isBottomEnabled)
                SpawnProjectilesFromBottom();
            //left timer
            if (isLeftEnabled)
                SpawnProjectilesFromLeft();
            //top timer
            if (isTopEnabled)
                SpawnProjectilesFromTop();
            //right timer
            if (isRightEnabled)
                SpawnProjectilesFromRight();
        }
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
            GameObject projectileInstance = Instantiate(foodToSpawn, transform.position, Quaternion.identity);
            //objectPooler.SpawnFromPool("EnemyBullet", transform.position, Quaternion.identity);
            projectileRB = projectileInstance.GetComponent<Rigidbody2D>();
            projectileRB.velocity = new Vector2(0, launchForce.y);

        }
    }
    public void SpawnProjectileLeft(GameObject projectileToSpawn, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject projectileInstance = Instantiate(projectileToSpawn, transform.position, Quaternion.identity);
            projectileRB = projectileInstance.GetComponent<Rigidbody2D>();
            projectileRB.velocity = new Vector2(launchForce.x, 0);
        }
    }
    public void SpawnProjectileTop(GameObject foodToSpawn, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject projectileInstance = Instantiate(foodToSpawn, transform.position, Quaternion.identity);
            projectileRB = projectileInstance.GetComponent<Rigidbody2D>();
            projectileRB.velocity = new Vector2(0, -launchForce.y);

        }
    }
    public void SpawnProjectileRight(GameObject foodToSpawn, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject projectileInstance = Instantiate(foodToSpawn, transform.position, Quaternion.identity);
            projectileRB = projectileInstance.GetComponent<Rigidbody2D>();
            projectileRB.velocity = new Vector2(-launchForce.x, 0);
        }
    }
}
