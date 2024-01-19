using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single : Attack
{
    public override void Init(Unit unit, HexDirection direction, CardSO card)
    {
        coords = unit.coords + direction;
        this.card = card;
        transform.position = coords.Pos;

        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, CardSO card)
    {
        coords = node.coords;
        this.card = card;
        transform.position = coords.Pos;

        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
}
