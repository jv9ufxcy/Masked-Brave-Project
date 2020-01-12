using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnImageActive : MonoBehaviour
{

    public bool UpSlashActive, DownSlashActive, isImageActive;
    [SerializeField]private Animator slashAnim;
    private SpriteRenderer slashImage;
    // Start is called before the first frame update
    void Start()
    {
        slashImage = GetComponent<SpriteRenderer>();
        slashAnim = GetComponent<Animator>();
        slashImage.color = Color.clear;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimBools();
    }

    private void UpdateAnimBools()
    {
        slashAnim.SetBool("UpSlash", UpSlashActive);
        slashAnim.SetBool("DownSlash", DownSlashActive);
        slashAnim.SetBool("AnySlash", isImageActive);
    }

    public void ActivateUpSlash()
    {
        slashImage.color = Color.white;
        UpSlashActive = true;
        DownSlashActive = false;
        isImageActive = true;
    }
    public void ActivateDownSlash()
    {
        slashImage.color = Color.white;
        DownSlashActive = true;
        UpSlashActive = false;
        isImageActive = true;
    }
    public void StopSlash()
    {
        StartCoroutine(Disappear());
    }
    private IEnumerator Disappear()
    {
        isImageActive = false;
        DownSlashActive = false;
        UpSlashActive = false;
        yield return new WaitForSeconds(0.2f);
        slashImage.color = Color.clear;
    }
}
