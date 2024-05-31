using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single : Action
{
    public override void Init(Unit unit, HexDirection direction, CardSO cardSO, int value = -999)
    {
        base.Init(unit, direction, cardSO, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, CardSO cardSO, int value = -999)
    {
        base.Init(unit, node, cardSO, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO cardSO, int value = -999)
    {
        base.Init(unit, node, nodes, cardSO, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
}
