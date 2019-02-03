using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImage : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField]    private float timer = 0.2f;
	// Use this for initialization
	void Start ()
	{
        transform.position = Player.Instance.transform.position;
        transform.localScale = Player.Instance.transform.position;

        spriteRenderer.sprite = Player.Instance.CurrentSpriteRenderer.sprite;
        spriteRenderer.color = new Vector4(50, 50, 50, 0.2f);
	}
	
	// Update is called once per frame
	void Update ()
	{
        timer -= Time.deltaTime;
        if (timer<=0)
        {
            Destroy(gameObject);
        }
	}
}
