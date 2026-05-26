using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionListener : MonoBehaviour
{
    private Mission curMission;
    // Start is called before the first frame update
    void Start()
    {
        if (Mission.instance!=null)
            curMission = Mission.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DestroyLoseCon()
    {
        if (curMission != null)
        {
            curMission.MoveObjectToScene();
        }
    }
    public void MissionChangeForm(int formIndex)
    {
        if (curMission != null)
        {
            curMission.HumanForm(formIndex);
        }
    }
    public void ChainTimerIncrement(float increment)
    {
        if (curMission != null)
            curMission.RewardChainKillTimer(increment);
    }
}
