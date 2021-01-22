using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerAfterImage : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private float fadeTime = 0.5f, aIInterval = 0.05f;
    [SerializeField]
    private Transform afterImageParent;
    [SerializeField]
    private Color trailColor = new Vector4(50, 50, 50, 0.2f), fadeColor;
	// Use this for initialization
	void Start ()
	{
        transform.position = Player.Instance.transform.position;
        transform.localScale = Player.Instance.transform.position;

        spriteRenderer.sprite = Player.Instance.CurrentSpriteRenderer.sprite;
	}
	
	// Update is called once per frame
	public void ShowAfterImage()
    {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < afterImageParent.childCount; i++)
        {
            Transform currentGhost = afterImageParent.GetChild(i);
            s.AppendCallback(() => currentGhost.position = Player.Instance.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().sprite = spriteRenderer.sprite);
            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(trailColor, 0));
            s.AppendCallback(() => FadeSprite(currentGhost));
            s.AppendInterval(aIInterval);
        }
    }
    public void FadeSprite(Transform current)
    {
        current.GetComponent<SpriteRenderer>().material.DOKill();
        current.GetComponent<SpriteRenderer>().material.DOColor(fadeColor, fadeTime);
    }
}
