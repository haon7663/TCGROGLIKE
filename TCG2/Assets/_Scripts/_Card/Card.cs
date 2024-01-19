using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [SerializeField] SpriteRenderer character;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text energyTMP;

    [HideInInspector] public CardSO cardData;
    public PRS originPRS;

    public void SetUp(CardSO cardData)
    {
        this.cardData = cardData;

        character.sprite = this.cardData.sprite;
        nameTMP.text = this.cardData.name;
        attackTMP.text = this.cardData.value.ToString();
        energyTMP.text = this.cardData.energy.ToString();
    }

    void OnMouseOver()
    {
        CardManager.Inst.CardMouseOver(this);
    }

    void OnMouseExit()
    {
        CardManager.Inst.CardMouseExit(this);
    }

    void OnMouseDown()
    {
        CardManager.Inst.CardMouseDown(this);
    }

    void OnMouseUp()
    {
        CardManager.Inst.CardMouseUp(this);
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        if(useDotween)
        {
            transform.DOKill();
            transform.DOLocalMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.DOKill();
            transform.localPosition = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }

    public void ShowLiner(bool isVisible = true)
    {
        lineRenderer.enabled = isVisible;
        if(isVisible)
        {
            lineRenderer.SetPosition(0, new Vector2(transform.position.x, transform.position.y + 1.75f));
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)));
        }
    }
}
