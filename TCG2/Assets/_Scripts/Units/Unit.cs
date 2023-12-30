using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitType unitType;

    SpriteRenderer spriteRenderer;

    public HexCoords hexCoords = new HexCoords(0, 0);

    [HideInInspector] public List<HexNode> selectAbleNodes;
    [HideInInspector] public List<HexNode> blindNodes;
     
    void Start()
    {
        if (transform.GetChild(0).TryGetComponent(out SpriteRenderer spriteRenderer))
            this.spriteRenderer = spriteRenderer;

        transform.position = hexCoords.Pos;
    }
    public void Init(Sprite sprite = null)
    {
        if (sprite)
            spriteRenderer.sprite = sprite;
    }
}