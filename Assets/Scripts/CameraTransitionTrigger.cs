using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransitionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject oldVirtualCamera;
    [SerializeField] private GameObject newVirtualCamera;
    [SerializeField] private GameObject oldTransitionCollider;
    [SerializeField] private string animationTrigger;
    private Animator stageAnim;
    // Use this for initialization
    void Start()
    {
        stageAnim = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CameraTransition();
        }
    }
    private void CameraTransition()
    {
        if (animationTrigger!=null)
            stageAnim.SetTrigger(animationTrigger);

        oldVirtualCamera.SetActive(false);
        newVirtualCamera.SetActive(true);
        oldTransitionCollider.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
