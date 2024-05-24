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

    public PRS originPRS;
    public CardInfo cardInfo;
    public Unit unit;

    public void SetUp(CardInfo cardInfo)
    {
        this.cardInfo = cardInfo;
        unit = this.cardInfo.unit;

        character.sprite = this.cardInfo.data.sprite;
        nameTMP.text = this.cardInfo.data.name;
        attackTMP.text = StatusManager.Calculate(unit, this.cardInfo.data).ToString();
        energyTMP.text = this.cardInfo.data.energy.ToString();
    }

    private void OnMouseOver()
    {
        CardManager.Inst.CardMouseOver(this);
    }

    private void OnMouseExit()
    {
        CardManager.Inst.CardMouseExit(this);
    }

    private void OnMouseDown()
    {
        CardManager.Inst.CardMouseDown(this);
    }

    private void OnMouseUp()
    {
        CardManager.Inst.CardMouseUp(this);
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0, bool isLocal = true)
    {
        if(useDotween)
        {
            transform.DOKill();
            if (isLocal)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, prs.pos.z);
                transform.DOLocalMove(prs.pos, dotweenTime);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, prs.pos.z);
                transform.DOMove(prs.pos, dotweenTime);
            }
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.DOKill();
            if (isLocal)
                transform.localPosition = prs.pos;
            else
                transform.position = prs.pos;
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

    public Unit GetUnit()
    {
        return unit;
    }
}