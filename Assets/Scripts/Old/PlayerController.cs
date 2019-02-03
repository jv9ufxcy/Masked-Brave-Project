using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    
    [SerializeField] private float speed;
    [SerializeField] private Animator anim;
    //private bool isFacingRight;
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        float dirx = Input.GetAxis("Horizontal");
        transform.position += new Vector3(dirx, 0) * speed * Time.deltaTime;
        anim.SetFloat("Speed", Mathf.Abs(dirx));
        if (dirx > 0)
            this.GetComponent<SpriteRenderer>().flipX =false;
        else if (dirx < 0)
            this.GetComponent<SpriteRenderer>().flipX = true;
    }


}
