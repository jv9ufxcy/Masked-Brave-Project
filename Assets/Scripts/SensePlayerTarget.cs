using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensePlayerTarget : MonoBehaviour
{
    private enum TargetSensor { isTopEnabled, isBottomEnabled };
    [SerializeField] private TargetSensor targetSensor;
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
            switch (targetSensor)
            {
                case TargetSensor.isTopEnabled:
                    thePlayerStats.EnemyTargetedUpDash();
                    break;
                case TargetSensor.isBottomEnabled:
                    thePlayerStats.EnemyTargetedDownDash();
                    break;
                default:
                    break;
            }
                
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.CompareTag("Enemy") || collision.CompareTag("Boss")) || thePlayerStats.PlayerIsOnGround)
        {
            switch (targetSensor)
            {
                case TargetSensor.isTopEnabled:
                    thePlayerStats.UpDashEmpty();
                    break;
                case TargetSensor.isBottomEnabled:
                    thePlayerStats.DownDashEmpty();
                    break;
                default:
                    break;
            }
        }
    }
}
