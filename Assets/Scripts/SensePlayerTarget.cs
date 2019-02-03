using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensePlayerTarget : MonoBehaviour
{
    [SerializeField] private bool isTopEnabled, isBottomEnabled;
    private Player thePlayerStats;
    // Use this for initialization
    void Start ()
    {
        thePlayerStats = GetComponentInParent<Player>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.CompareTag("Enemy") || collision.CompareTag("Boss")) && !thePlayerStats.PlayerIsOnGround)
        {
            if (isTopEnabled)
                thePlayerStats.EnemyTargetedUpDash();
            else if (isBottomEnabled)
                thePlayerStats.EnemyTargetedDownDash();
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.CompareTag("Enemy") || collision.CompareTag("Boss")) || thePlayerStats.PlayerIsOnGround)
        {
            if (isTopEnabled)
                thePlayerStats.UpDashEmpty();
            else if (isBottomEnabled)
                thePlayerStats.DownDashEmpty();
        }
    }
}
