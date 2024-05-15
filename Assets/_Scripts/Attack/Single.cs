using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single : Action
{
    public override void Init(Unit unit, HexDirection direction, CardData data, int value = -999)
    {
        base.Init(unit, direction, data, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, CardData data, int value = -999)
    {
        base.Init(unit, node, data, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, List<HexNode> nodes, CardData data, int value = -999)
    {
        base.Init(unit, node, nodes, data, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
}
