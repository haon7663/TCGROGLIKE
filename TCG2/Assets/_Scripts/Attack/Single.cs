using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Single : Attack
{
    public override void Init(Unit unit, HexDirection direction, CardSO data, int value = -999)
    {
        base.Init(unit, direction, data, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, CardSO data, int value = -999)
    {
        base.Init(unit, node, data, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
    public override void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO data, int value = -999)
    {
        base.Init(unit, node, nodes, data, value);
        if (!TryGetComponent(out Animator animator))
            ActiveEvent();
    }
}
