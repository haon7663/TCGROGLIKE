using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single : Attack
{
    public override void Init(Unit unit, HexDirection direction, CardSO card)
    {
        base.Init(unit, direction, card);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, CardSO card)
    {
        base.Init(unit, node, card);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO card)
    {
        base.Init(unit, node, nodes, card);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
}
