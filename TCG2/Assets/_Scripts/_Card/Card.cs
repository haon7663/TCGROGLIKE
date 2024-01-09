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
    [SerializeField] Sprite cardFront;
    [SerializeField] Sprite cardBack;

    public CardSO card;
    public PRS originPRS;

    Camera  mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetUp(CardSO card)
    {
        this.card = card;

        character.sprite = this.card.sprite;
        nameTMP.text = this.card.name;
        attackTMP.text = this.card.value.ToString();
        energyTMP.text = this.card.energy.ToString();
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
}
