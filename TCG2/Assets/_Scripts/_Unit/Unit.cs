using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public HexCoords coords;

    public UnitSO unitData;

    [Header("Stats")]
    public int hp;
    public int defence;

    void Start()
    {
        if (transform.GetChild(0).TryGetComponent(out SpriteRenderer spriteRenderer))
            this.spriteRenderer = spriteRenderer;

        Init(spriteRenderer.sprite);
    }
    public void Init(Sprite sprite)
    {
        if (sprite)
            spriteRenderer.sprite = sprite;

        hp = unitData.hp;
        HealthManager.Inst.GenerateHealthBar(this);

        coords = new HexCoords(Random.Range(-3, 4), Random.Range(-3, 4));
        transform.position = (Vector3)coords.Pos - Vector3.forward;
        GridManager.Inst.OnTile(coords, this);

        spriteRenderer.sortingOrder = -coords._r;
    }

    public void Repeat(HexNode hexNode)
    {
        spriteRenderer.flipX = hexNode.Coords.Pos.x > transform.position.x;
    }

    public bool OnDamage(int value)
    {
        if(true)
        {
            print("Damage");
            var overDamage = defence -= value;
            hp -= overDamage;
            HealthManager.Inst.SetFilled(this);
            return true;
        }
    }
    public bool OnHealth(int value)
    {
        if (true)
        {
            print("Health");
            hp += value;
            HealthManager.Inst.SetFilled(this);
            return true;
        }
    }
    public bool OnDefence(int value)
    {
        if (true)
        {
            print("Defence");
            defence += value;
            HealthManager.Inst.SetFilled(this);
            return true;
        }
    }

    void OnMouseDown()
    {
        UnitManager.Inst.SelectUnit(this);
    }
}
