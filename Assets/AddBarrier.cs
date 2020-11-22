using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBarrier : MonoBehaviour
{
    [SerializeField] private string tagToCollide;
    [SerializeField] private int barrierHealth=2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(tagToCollide))
        {
            HealthManager hp = collision.GetComponent<HealthManager>();
            if (hp!=null)
            {
                hp.AddShield(barrierHealth);
                Destroy(gameObject);
            }
        }
    }
}
