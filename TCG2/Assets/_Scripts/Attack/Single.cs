using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single : Attack
{
    public override void Init(Unit unit, HexDirection direction, CardSO data)
    {
        base.Init(unit, direction, data);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, CardSO data)
    {
        base.Init(unit, node, data);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO data)
    {
        base.Init(unit, node, nodes, data);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
}
