using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBehavior : StateMachineBehaviour
{
    private BossPatrolManager bossHealthManager;
    private Rigidbody2D enemyRB;

    [SerializeField] private float timerToTransition;
    [SerializeField] private float minTime = 4;
    [SerializeField] private float maxTime = 8;
    [SerializeField] private int whichStateToTransitionTo;
    [SerializeField] private int minChanceToCharge = 4;
    [SerializeField] private int maxChanceToCharge = 8;

    //jumping
    private bool isFacingRight;
    private bool isOnGround;
    private Vector2 jumpForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpStrength;
    [SerializeField] private bool moveRight;
    [SerializeField] private bool shouldEnemyWalkOffEdge;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyRB = animator.GetComponent<Rigidbody2D>();
        bossHealthManager = animator.GetComponent<BossPatrolManager>();
        timerToTransition = Random.Range(minTime, maxTime);
        whichStateToTransitionTo = Random.Range(0, maxChanceToCharge);
        Initialize(animator);
            bossHealthManager.Jump();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timerToTransition <= 0)
        {
            if (whichStateToTransitionTo <= minChanceToCharge)
                animator.SetTrigger("Charge");
            else
                animator.SetTrigger("Run");
        }
        else
        {
            timerToTransition -= Time.deltaTime;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
    private void Initialize(Animator animator)
    {
        //jumpForce = new Vector2(0, jumpStrength);
        
        enemyRB = animator.GetComponent<Rigidbody2D>();
        bossHealthManager = animator.GetComponent<BossPatrolManager>();
    }
    private void PassAnimationSpeed(Animator animator)
    {
        animator.SetFloat("vSpeed", enemyRB.velocity.y);
        animator.SetFloat("hSpeed", enemyRB.velocity.x);

        isOnGround = bossHealthManager.IsOnGround;
    }
    private void EnemyMovement(Animator animator)
    {
        
            if (moveRight)
            {
                bossHealthManager.FlipFacingRight();
                enemyRB.velocity = new Vector2(moveSpeed, enemyRB.velocity.y);
            }
            else
            {
                bossHealthManager.FlipFacingLeft();
                enemyRB.velocity = new Vector2(-moveSpeed, enemyRB.velocity.y);
            }
       
    }
    private void Flip()
    {
        moveRight = !moveRight;
        isFacingRight = !isFacingRight;
    }
    private void TurnEnemyAround()
    {
        bool notAtEdge = bossHealthManager.IsNotAtEdge;
        bool hittingWall = bossHealthManager.IsHittingWall;
        if (hittingWall || (!notAtEdge && !shouldEnemyWalkOffEdge))
            Flip();

    }
}
