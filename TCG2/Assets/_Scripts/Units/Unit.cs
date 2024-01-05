using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public UnitType unitType;
    public HexCoords coords;

    public int maxHp;
    public int curHp;

    void Start()
    {
        if (transform.GetChild(0).TryGetComponent(out SpriteRenderer spriteRenderer))
            this.spriteRenderer = spriteRenderer;

        Init(spriteRenderer.sprite, 50);
    }
    public void Init(Sprite sprite, int hp)
    {
        if (sprite)
            spriteRenderer.sprite = sprite;

        maxHp = hp;
        curHp = hp;
        HealthManager.Inst.GenerateHealthBar(this);

        coords = new HexCoords(Random.Range(-3, 4), Random.Range(-3, 4));
        transform.position = coords.Pos;
        GridManager.Inst.OnTile(coords, this);
    }

    public void Repeat(HexNode hexNode)
    {
        spriteRenderer.flipX = hexNode.Coords.Pos.x > transform.position.x;
    }

    public bool OnDamage(int damamge)
    {
        if(true)
        {
            print("Damage");
            curHp -= damamge;
            HealthManager.Inst.SetFilled(this);
            return true;
        }
    }
}
