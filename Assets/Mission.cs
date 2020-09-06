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

    public void StartMission()
    {
        StartCoroutine(MissionStart());
    }
    private IEnumerator MissionStart()
    {
        
        yield return new WaitForSeconds(.01f);
        missionStartText.rectTransform.DOAnchorPos(offScreen, 2);
        mainChar.QuickChangeForm(4);
        mainChar.SetState(37);
        yield return new WaitForSeconds(4f);
        missionStartText.DOColor(Color.white, 1);
        missionStartText.rectTransform.DOAnchorPos(midScreen, 2);
        yield return new WaitForSeconds(4f);
        missionStartText.transform.DOScale(8, .25f);
        missionStartText.DOColor(Color.clear, 0.25f);
        mainChar.controlType = CharacterObject.ControlType.PLAYER;
    }
}
