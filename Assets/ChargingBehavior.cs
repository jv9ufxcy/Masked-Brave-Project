using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingBehavior : StateMachineBehaviour
{
    [SerializeField] private float timeToCharge;
    private float startChargeTime=0.8f;
    private bool isCharging;
    private BossPatrolManager bossHealthManager;
    private Rigidbody2D bossRB;
    private float defaultGravityScale;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeToCharge = startChargeTime;
        bossHealthManager = animator.GetComponent<BossPatrolManager>();

        bossRB=animator.GetComponent<Rigidbody2D>();
        defaultGravityScale = bossRB.gravityScale;
        bossHealthManager.ChangeDirection();
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timeToCharge <= 0)
        {
            bossHealthManager.Charge();
        }
        else
        {
            timeToCharge -= Time.deltaTime;
            bossRB.velocity = new Vector2(0, 0);
            bossRB.gravityScale = 0;
        }
    }

    

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bossRB.gravityScale = defaultGravityScale;

    }


}
