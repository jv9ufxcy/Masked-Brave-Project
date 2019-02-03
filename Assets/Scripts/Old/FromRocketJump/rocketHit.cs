using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rocketHit : MonoBehaviour {


    //[SerializeField]
    //public float explosionForce;
    //[SerializeField]
    //public float radius;
    //[SerializeField]
    //public float power;
    //[SerializeField]
    //public float explosionRadius;
    //[SerializeField]
    //public float upliftModifier;


    //[SerializeField]
    //private PointEffector2D pointEffector;

    Rigidbody2D myRigidbody;
    ProjectileController myPC;
    public GameObject Explosion;
    private Vector3 explosionPosition;

    public string explodesound;
    private AudioManager audioManager;
    // Use this for initialization
    private void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    void Awake()
    {
        myPC = GetComponentInParent<ProjectileController>();
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    /*
    private void ConcussiveBlast()
    {
        var dir = (myRigidbody.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        Vector3 baseForce = dir.normalized * explosionForce * wearoff;
        myRigidbody.AddForce(baseForce);

        float upliftWearoff = 1 - upliftModifier / explosionRadius;
        Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
        myRigidbody.AddForce(upliftForce);
    }*/

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer==LayerMask.NameToLayer("Shootable")|| other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            myPC.RemoveForce();
            //pointEffector.enabled = true;
            //ConcussiveBlast(GetComponent<Rigidbody2D>(), power * 100, transform.position, radius);
            Instantiate(Explosion, transform.position, transform.rotation);
            Debug.Log(Explosion);
            audioManager.PlaySound(explodesound);
            FindObjectOfType<AudioManager>().PlaySound("explodesound");
            Destroy(gameObject);
        }
    }

    //private void ConcussiveBlast(Rigidbody2D myRigidbody, float v, Vector2 position, float radius)
    //{
    //    var dir = (myRigidbody.transform.position - explosionPosition);
    //    float wearoff = 1 - (dir.magnitude / explosionRadius);
    //    Vector2 baseForce = (dir.normalized * explosionForce) * wearoff;
    //    myRigidbody.AddForce(baseForce);
    //    Debug.Log(baseForce);

    //    /*float upliftWearoff = 1 - upliftModifier / explosionRadius;
    //    Vector2 upliftForce = Vector2.up * (explosionForce * upliftWearoff);
    //    myRigidbody.AddForce(upliftForce);
    //    Debug.Log(upliftForce);*/
    //}

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Shootable"))
        {
            myPC.RemoveForce();
            //ConcussiveBlast(GetComponent<Rigidbody2D>(), power * 100, transform.position, radius);
            Instantiate(Explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
