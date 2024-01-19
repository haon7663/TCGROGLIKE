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
        move = GetComponent<Unit_Move>();
        card = GetComponent<Unit_Card>();
    }
    #endregion

    SpriteRenderer spriteRenderer;
    Animator animator;

    public HexCoords coords;

    public UnitSO unitData;

    [Header("Stats")]
    public int hp;
    public int defence;

    [Header("Systems")]
    public Unit target;

    void Start()
    {
        if (transform.GetChild(0).TryGetComponent(out SpriteRenderer spriteRenderer))
            this.spriteRenderer = spriteRenderer;
        if (transform.GetChild(0).TryGetComponent(out Animator animator))
            this.animator = animator;

        Init(spriteRenderer.sprite);
    }
    public void Init(Sprite sprite)
    {
        if (sprite)
            spriteRenderer.sprite = sprite;

        hp = unitData.hp;
        HealthManager.Inst.GenerateHealthBar(this);

        coords = GridManager.Inst.GetRandomNode().coords;
        transform.position = (Vector3)coords.Pos - Vector3.forward;
        GridManager.Inst.OnTile(coords, this);

        spriteRenderer.sortingOrder = -coords._r;
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
            print("Damage");
            if (defence >= value)
                defence -= value;
            else if(defence < value)
            {
                var overValue = defence - value;
                hp += overValue;
                defence = 0;
            }
            else
            {
                hp -= value;
            }
            HealthManager.Inst.SetHealthBar(this);

            if(hp <= 0)
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

    void OnMouseDown()
    {
        UnitManager.Inst.SelectUnit(this);
    }

    #region Animations
    public void Anim_SetTrigger(string name) => animator.SetTrigger(name);
    public void Anim_SetBool(string name, bool value) => animator.SetBool(name, value);
    #endregion
}
