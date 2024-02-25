using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
    #region UnitComponent
    [HideInInspector] public Unit_Move move;
    [HideInInspector] public Unit_Card card;
    void Awake()
    {
        if (transform.TryGetComponent(out Unit_Move unit_Move))
            move = unit_Move;
        if (transform.TryGetComponent(out Unit_Card unit_Card))
            card = unit_Card;
    }
    #endregion

    SpriteRenderer spriteRenderer;
    Animator animator;

    public HexCoords coords;
    public UnitSO data;

    [Header("Stats")]
    public int hp;
    public int defence;

    [Header("Systems")]
    public Unit target;
    public HexCoords targetCoords;
    public List<StatusInfo> statuses;

    void Start()
    {
        if (transform.GetChild(0).TryGetComponent(out SpriteRenderer spriteRenderer))
            this.spriteRenderer = spriteRenderer;
        if (transform.GetChild(0).TryGetComponent(out Animator animator))
            this.animator = animator;

        Init();
    }
    public void Init()
    {
        animator.runtimeAnimatorController = data.animatorController;

        hp = data.hp;
        HealthManager.Inst.GenerateHealthBar(this);

        coords = GridManager.Inst.GetRandomNode().coords;
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
}
