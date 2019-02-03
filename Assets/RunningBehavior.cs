using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningBehavior : StateMachineBehaviour
{
    [SerializeField] private float timerToTransition;
    [SerializeField] private float minTime=4;
    [SerializeField] private float maxTime=8;
    [SerializeField] private int whichStateToTransitionTo;
    [SerializeField] private int minChanceToCharge = 4;
    [SerializeField] private int maxChanceToCharge = 8;

    private Player thePlayer;
    private BossHealthManager bossHealthManager;
    private Animator enemyAnim;

    private AudioManager audioManager;

    private bool isFacingRight;
    private Rigidbody2D enemyRB;
    private SpriteRenderer enemyRend;

    //grounded
    private bool isOnGround;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpStrength;
    [SerializeField] private bool moveRight;
    [SerializeField] private bool shouldEnemyWalkOffEdge;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateinfo, int layerindex)
    {

        timerToTransition = Random.Range(minTime, maxTime);
        whichStateToTransitionTo = Random.Range(0, maxChanceToCharge);
        Initialize(animator);
        bossHealthManager.ChangeDirection();
        CheckLocationOfPlayer();
    }

    private void CheckLocationOfPlayer()
    {
        if (bossHealthManager.PlayerIsToTheRight)
        {
            moveRight = true;
        }
        else
            moveRight = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timerToTransition <= 0)
        {
            if (whichStateToTransitionTo <= minChanceToCharge)
                animator.SetTrigger("Charge");
            else
            {
                animator.SetTrigger("Jump");
            }
        }
        else
            timerToTransition -= Time.deltaTime;

        TurnEnemyAround();
        EnemyMovement(animator);
        PassAnimationSpeed(animator);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
    private void Initialize(Animator animator)
    {
        thePlayer = FindObjectOfType<Player>();
        enemyRB = animator.GetComponent<Rigidbody2D>();
        enemyRend = animator.GetComponent<SpriteRenderer>();
        bossHealthManager = animator.GetComponent<BossHealthManager>();
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No Audio Manager in Scene");
        }
    }
    private void PassAnimationSpeed(Animator animator)
    {
        animator.SetFloat("vSpeed", enemyRB.velocity.y);
        animator.SetFloat("hSpeed", enemyRB.velocity.x);

        animator.SetBool("Ground", bossHealthManager.IsOnGround);
        if (bossHealthManager.IsOnGround)
        {
            animator.SetBool("Ground", true);
        }
        if (enemyRB.velocity.y < 0)
        {
            animator.SetBool("Ground", false);
        }
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

    public void Flip()
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
