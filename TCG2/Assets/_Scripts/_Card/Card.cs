using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer character;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text energyTMP;
    [SerializeField] Sprite cardFront;
    [SerializeField] Sprite cardBack;

    public Item item;
    public PRS originPRS;

    Camera  mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetUp(Item item)
    {
        this.item = item;

        character.sprite = this.item.sprite;
        nameTMP.text = this.item.name;
        attackTMP.text = this.item.attackDamage.ToString();
        energyTMP.text = this.item.energy.ToString();
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
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }

    public void ShowLiner()
    {
        lineRenderer.SetPosition(0, new Vector2(transform.position.x, transform.position.y + 1.75f));
        lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)));
    }
}
