using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
    [HideInInspector] public Unit_Move move;
    [HideInInspector] public Unit_Card card;

    SpriteRenderer spriteRenderer;
    Animator animator;
    GameObject actionObject;
    SpriteRenderer actionSpriteRenderer;
    TMP_Text actionText;

    public HexCoords coords;
    public UnitSO data;

    [Header("Stats")]
    public int hp;
    public int defence;
    int value;

    [Header("Systems")]
    public Unit targetUnit;
    public HexCoords targetCoords;
    public List<StatusInfo> statuses;

    void Awake()
    {
        if (transform.TryGetComponent(out Unit_Move unit_Move))
            move = unit_Move;
        if (transform.TryGetComponent(out Unit_Card unit_Card))
            card = unit_Card;

        if (transform.GetChild(0).TryGetComponent(out SpriteRenderer spriteRenderer))
            this.spriteRenderer = spriteRenderer;
        if (transform.GetChild(0).TryGetComponent(out Animator animator))
            this.animator = animator;

        actionObject = transform.GetChild(3).gameObject;
        if (actionObject.transform.GetChild(0).TryGetComponent(out SpriteRenderer renderer))
            actionSpriteRenderer = renderer;
        if (actionObject.transform.GetChild(1).TryGetComponent(out TMP_Text text))
            actionText = text;
    }

    public void Init(UnitSO data, HexCoords coords)
    {
        this.data = Instantiate(data);

        animator.runtimeAnimatorController = this.data.animatorController;

        hp = this.data.hp;
        HealthManager.Inst.GenerateHealthBar(this);

        this.coords = coords;
        transform.position = coords.Pos - Vector3.forward;
        GridManager.Inst.SetTileUnit(coords, this);
    }

    public void Repeat(HexNode hexNode)
    {
        spriteRenderer.flipX = hexNode.coords.Pos.x > transform.position.x;
    }

    #region OnReceive
    public bool OnDamage(int value)
    {
        if(true)
        {
            if (defence >= value)
                defence -= value;
            else
            {
                var overValue = defence - value;
                hp += overValue;
                defence = 0;
                StartCoroutine(HealthManager.Inst.WhiteMaterial(this));
                Anim_SetTrigger("hit");
            }
            HealthManager.Inst.SetHealthBar(this);

            if (hp <= 0)
            {
                UnitManager.Inst.Death(this);
            }
            return true;
        }
    }
    public bool OnHealth(int value)
    {
        if (true)
        {
            print("Health");
            hp += value;
            if (hp >= data.hp)
                hp = data.hp;
            HealthManager.Inst.SetHealthBar(this);
            return true;
        }
    }
    public bool OnDefence(int value)
    {
        if (true)
        {
            print("Defence");
            defence += value;
            HealthManager.Inst.SetHealthBar(this);
            return true;
        }
    }
    #endregion

    void OnMouseOver()
    {
        UnitManager.Inst.UnitMouseOver(this);
    }
    void OnMouseExit()
    {
        UnitManager.Inst.UnitMouseExit(this);
    }
    void OnMouseDown()
    {
        UnitManager.Inst.UnitMouseDown(this);
    }

    #region Animations
    public void Anim_SetTrigger(string name) => animator.SetTrigger(name);
    public void Anim_SetBool(string name, bool value) => animator.SetBool(name, value);
    #endregion

    public void SetMaterial(Material material) => spriteRenderer.material = material;
    public void SetFlipX(bool value) => spriteRenderer.flipX = value;

    public void ShowAction(Sprite sprite, int value)
    {
        this.value = value;
        actionObject.SetActive(true);
        actionSpriteRenderer.sprite = sprite;
        SetActionText();
    }
    public void SetActionText()
    {
        //if(actionObject.activeSelf)
            //actionText.text = StatusManager.Calculate(this, card.data, value).ToString();
    }
    public void HideAction()
    {
        actionObject.SetActive(false);
    }
}
