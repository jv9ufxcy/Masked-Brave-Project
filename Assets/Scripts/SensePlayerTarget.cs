using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensePlayerTarget : MonoBehaviour
{
    private enum TargetSensor { isTopEnabled, isBottomEnabled };
    [SerializeField] private TargetSensor targetSensor;
    private Player thePlayerStats;
    private LockOnImageActive lockOn;
    // Use this for initialization
    void Start ()
    {
        thePlayerStats = GetComponentInParent<Player>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.CompareTag("Enemy") || collision.CompareTag("Boss")) && !thePlayerStats.PlayerIsOnGround)
        {
            lockOn = collision.gameObject.GetComponentInChildren<LockOnImageActive>();
            
            switch (targetSensor)
            {
                case TargetSensor.isTopEnabled:
                    thePlayerStats.EnemyTargetedUpDash();
                    if (lockOn!=null)
                        lockOn.ActivateUpSlash();
                    break;
                case TargetSensor.isBottomEnabled:
                    thePlayerStats.EnemyTargetedDownDash();
                    if (lockOn != null)
                        lockOn.ActivateDownSlash();
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
            lockOn = collision.gameObject.GetComponentInChildren<LockOnImageActive>();
            if (lockOn != null)
                lockOn.StopSlash();
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
