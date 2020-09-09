using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Mission : MonoBehaviour
{
    public CharacterObject mainChar;
    public TextMeshProUGUI missionStartText;
    public string missionText;
    public Vector2 midScreen, offScreen;
    public float missionStartSeconds = 6f;

    private void Start()
    {
        
    }
    private void FixedUpdate()
    {
        
    }
    public void StartMission()
    {
        StartCoroutine(MissionStart());
    }
    private IEnumerator MissionStart()
    {
        
        yield return new WaitForSeconds(.001f);
        mainChar.controlType = CharacterObject.ControlType.DEAD;
        missionStartText.rectTransform.DOAnchorPos(offScreen, 2);
        mainChar.QuickChangeForm(4);
        mainChar.SetState(37);
        yield return new WaitForSeconds(missionStartSeconds/2);
        missionStartText.DOColor(Color.white, 1);
        missionStartText.rectTransform.DOAnchorPos(midScreen, 2);
        yield return new WaitForSeconds(missionStartSeconds/2);
        missionStartText.transform.DOScale(8, .25f);
        missionStartText.DOColor(Color.clear, 0.25f);
        mainChar.controlType = CharacterObject.ControlType.PLAYER;
    }
}
