using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeTime : MonoBehaviour
{
    [SerializeField] private bool isFrozen=false;
    [SerializeField] private float maxFreezeDuration, freezeTimeScale = 0f;
    private float freezeDuration;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (freezeDuration > 0 && !isFrozen)
        {
            StartCoroutine(DoFreeze());
        }
    }
    public void FreezeFrame()
    {
        freezeDuration = maxFreezeDuration;
    }
    IEnumerator DoFreeze()
    {
        isFrozen = true;
        var originalTimeScale = Time.timeScale;
        Time.timeScale = freezeTimeScale;

        yield return new WaitForSecondsRealtime(maxFreezeDuration);

        Time.timeScale = originalTimeScale;
        freezeDuration = 0;
        isFrozen = false;
    }
}
