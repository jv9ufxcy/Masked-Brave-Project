using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {
    [SerializeField]
    public float bulletSpeed;

    Rigidbody2D myRigidbody2D;
	// Use this for initialization
	void Awake ()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        // if (transform.localRotation.z > 0)
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 desiredDir = targetPos - transform.position; 
        myRigidbody2D.AddForce(desiredDir* bulletSpeed, ForceMode2D.Impulse);
        Debug.Log("Shots Fired");
        //else myRigidbody2D.AddForce(transform.right * bulletSpeed, ForceMode2D.Impulse);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void RemoveForce()
    {
        myRigidbody2D.velocity = new Vector2(0, 0);
    }
}
