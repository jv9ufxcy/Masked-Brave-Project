using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public int direction,bulletType=2;

    public GameObject[] bullets;
    public CharacterObject characterObject, target;
    public Vector3 targetPos;
    public Vector2 moveDirection;
    private Animator turretAnim;
    private HealthManager turretHealth;
    public bool IsParentAlive()
    {
        return characterObject.healthManager.IsDead = false;
    }
    public bool IsActive()
    {
        return turretHealth.IsDead = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        turretAnim = GetComponent<Animator>();
        turretHealth = GetComponent<HealthManager>();
        target = GameEngine.gameEngine.mainCharacter;
        targetPos = target.transform.position;
        moveDirection = (targetPos - transform.position).normalized;
        transform.parent = characterObject.transform;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void FireBullet()
    {
        turretAnim.Play("Shoot");
        var offset = new Vector3(0.5f * direction, 0, 0);
        GameObject bulletGO = Instantiate(bullets[0], transform.position + offset, Quaternion.identity);
        BulletHit bullet = bulletGO.GetComponent<BulletHit>();
        bullet.character = characterObject;
        bullet.bulletType = bulletType;
        bullet.target = target;
        bulletGO.GetComponent<Hitbox>().character = characterObject;
        bulletGO.transform.localScale = new Vector3(direction, 1, 1);
    }
}
