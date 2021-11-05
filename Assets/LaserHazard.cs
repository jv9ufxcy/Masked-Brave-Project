using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserHazard : MonoBehaviour
{
    public ParticleSystem hitEffect;

    public LineRenderer lineRend;
    public LayerMask collisionMask;
    public float laserDuration = 1f, maxDist = 16f;
    public float timer, timeToFire=4;
    public Transform firePoint;
    // Start is called before the first frame update
    void Start()
    {
        lineRend = GetComponentInChildren<LineRenderer>();
        timer = timeToFire;
        lineRend.enabled = false;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.DrawRay(firePoint.position, -Vector2.up * maxDist, Color.red);
        if (timer <= 0)
        {
            FireRay();
        }
        else
        {
            timer -= Time.fixedDeltaTime;
            if (timer < .5f && timer > 0.12f)
            {
                DisplayLaser();
                lineRend.startWidth = 0.2f;
                lineRend.endWidth = 0.2f;
            }
            else
            {
                lineRend.startWidth = 1f;
                lineRend.endWidth = 1f;
                lineRend.enabled = false;
            }
        }
    }
    public float laserActiveTime = 1;
    public void FireRay()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(firePoint.position, -Vector2.up, maxDist, collisionMask);

        lineRend.enabled = true;
        if (hitInfo)
        {
            lineRend.SetPosition(0, firePoint.position);
            lineRend.SetPosition(1, hitInfo.point);
        }
        else
        {
            lineRend.SetPosition(0, firePoint.position);
            lineRend.SetPosition(1, -Vector2.up * 100);
        }

        if (hitInfo)
        {
            HitCharacter(hitInfo.transform.root.GetComponent<IHittable>());
            //Instantiate(hitEffect, hitInfo.point, Quaternion.identity);
            hitEffect.Play();
            hitEffect.transform.position = hitInfo.point;
        }
        laserDuration -= Time.fixedDeltaTime;
        if (laserDuration <= 0)
        {
            lineRend.enabled = false;
            timer = timeToFire;
            laserDuration = laserActiveTime;
        }
    }

    private void DisplayLaser()
    {
        RaycastHit2D hitInfo = Physics2D.Raycast(firePoint.position, -Vector2.up, maxDist, collisionMask);

        lineRend.enabled = true;
        if (hitInfo)
        {
            //Instantiate(hitEffect, hitInfo.point, Quaternion.identity);
            lineRend.SetPosition(0, firePoint.position);
            lineRend.SetPosition(1, hitInfo.point);
        }
        else
        {
            lineRend.SetPosition(0, firePoint.position);
            lineRend.SetPosition(1, -Vector2.up * 100);
        }
    }

    private IEnumerator DisplayLineRend(float frames, IHittable coll)
    {
        
        for (int i = 0; i < frames; i++)
        {
            
            yield return new WaitForFixedUpdate();
        }
        
    }
    [SerializeField] private int projectileIndex = 61;
    public CharacterObject character;
    //public LayerMask nonInteractableLayer;
    private void HitCharacter(IHittable victim)
    {
        //IHittable victim = collision.transform.root.GetComponent<IHittable>();
        if (victim != null)
        {
            victim.Hit(character, projectileIndex, 0);
        }
    }
}
