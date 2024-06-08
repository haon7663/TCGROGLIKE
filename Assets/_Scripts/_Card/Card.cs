using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Serialization;

public class Card : MonoBehaviour
{
    [SerializeField] private SpriteRenderer character;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private TMP_Text nameTMP;
    [SerializeField] private TMP_Text attackTMP;
    [SerializeField] private TMP_Text energyTMP;

    public PRS originPRS;
    public CardSO CardSO { get; private set; }
    public Unit Unit { get; private set; }

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    public void SetUp(CardSO cardSO)
    {
        CardSO = cardSO;
        Unit = cardSO.Unit;

        character.sprite = CardSO.sprite;
        nameTMP.text = CardSO.name;
        attackTMP.text = StatusEffectManager.Calculate(Unit, CardSO).ToString();
        energyTMP.text = CardSO.energy.ToString();
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
        
        if (!isVisible) return;
        lineRenderer.SetPosition(0, new Vector2(transform.position.x, transform.position.y + 1.75f));
        lineRenderer.SetPosition(1, _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)));
    }

    public Unit GetUnit()
    {
        return Unit;
    }
}