using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single : Action
{
    public override void Init(Unit unit, HexDirection direction, CardSO cardSO)
    {
        base.Init(unit, direction, cardSO);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, CardSO cardSO)
    {
        base.Init(unit, node, cardSO);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO cardSO)
    {
        base.Init(unit, node, nodes, cardSO);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
}
