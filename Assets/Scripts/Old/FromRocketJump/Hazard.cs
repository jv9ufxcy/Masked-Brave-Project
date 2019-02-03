using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hazard : MonoBehaviour {


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //TODO: replace with respawn
            collision.gameObject.GetComponent<RespawnPlayer>().Respawn();
        }
        //else
        //{
        //    collision.gameObject.transform.position = Checkpoint.currentlyActiveCheckpoint.transform.position;
        //}
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
