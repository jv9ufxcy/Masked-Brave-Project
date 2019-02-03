using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDoorTrigger : MonoBehaviour {
    [SerializeField] private GameObject bossCharacter;
    [SerializeField] private GameObject bossDoorEnter;
    [SerializeField] private GameObject bossDoorExit;

    private Animator stageAnim;
    [SerializeField] private Animator boundingAnim;
    // Use this for initialization
    void Start ()
    {
        stageAnim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SpawnBoss();
        }
    }
    private void SpawnBoss()
    {
        
        stageAnim.SetTrigger("RoyBossTrigger");
        
        bossDoorEnter.SetActive(true);
        bossCharacter.SetActive(true);
    }
    public void DeSpawnBoss()
    {
        //boundingAnim.SetTrigger("RoyBossTrigger");
        Destroy(bossCharacter);
        bossDoorExit.SetActive(true);
    }
}
