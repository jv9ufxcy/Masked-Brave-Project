using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class WindTrigger : MonoBehaviour
{
    CharacterObject player;
    [SerializeField] private int cyclonePow;
    [SerializeField] private float lifeTime = 3f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameEngine.hitStop<=0)
        {
            if (lifeTime > 0)
            {
                lifeTime -= Time.deltaTime;
            }
            else
                Destroy(gameObject);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        player = collision.gameObject.GetComponent<CharacterObject>();
        if (collision.CompareTag("Player"))
        {
            player.OnCyclonePower(cyclonePow);
        }
    }
}
