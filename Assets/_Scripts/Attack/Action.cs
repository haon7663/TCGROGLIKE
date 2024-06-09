using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Pool;

public abstract class Action : MonoBehaviour
{
    protected Unit Unit;
    protected CardSO CardSO;
    protected HexCoords TargetCoords;
    protected List<HexNode> TargetNodes = new List<HexNode>();
    protected HexDirection StartDirection = HexDirection.Default;

    public virtual void Init(Unit unit, HexDirection direction, CardSO cardSO)
    {
        Unit = unit;
        CardSO = cardSO;
        TargetCoords = unit.coords;
        transform.position = unit.coords.Pos;
        StartDirection = direction;
    }
    public virtual void Init(Unit unit, HexNode node, CardSO cardSO)
    {
        Unit = unit;
        CardSO = cardSO;
        TargetCoords = node.Coords;
        transform.position = TargetCoords.Pos;
    }
    public virtual void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO cardSO)
    {
        Unit = unit;
        CardSO = cardSO;
        TargetNodes = nodes;
        TargetCoords = node.Coords;
        transform.position = TargetCoords.Pos;
    }

    public bool ActiveEventValue(HexCoords coords, CardSO cardSO)
    {
        TargetCoords = coords;
        CardSO = cardSO;
        return ActiveEvent();
    }
    public bool ActiveEvent()
    {
        List<Unit> afterEventUnits = new();
        if (TargetNodes.Count <= 1)
        {
            var onUnit = GridManager.inst.GetUnit(TargetCoords);
            if (!onUnit)
                return false;
            
            TypeToEffect(onUnit);
            afterEventUnits.Add(onUnit);
            AfterEvent(afterEventUnits);
        }
        else
        {
            afterEventUnits.AddRange(from node in TargetNodes select GridManager.inst.GetUnit(node) into onUnit where onUnit where TypeToEffect(onUnit) select onUnit);
            AfterEvent(afterEventUnits);
        }
        return true;
    }

    private bool TypeToEffect(Unit targetUnit)
    {
        switch (CardSO.activeType)
        {
            case ActiveType.Attack:
                targetUnit.OnDamage(StatusEffectManager.CalculateDamage(Unit, targetUnit, CardSO.value));
                break;
            case ActiveType.Defence:
                targetUnit.OnDefence(StatusEffectManager.CalculateDefence(Unit, targetUnit, CardSO.value));
                break;
            case ActiveType.Recovery:
                targetUnit.OnHealth(StatusEffectManager.CalculateHealth(Unit, targetUnit, CardSO.value));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!targetUnit) 
            return false;

        StartCoroutine(StatusEffectManager.inst.AddStatusEffects(CardSO.statuses, targetUnit));
        return true;
    }
    private void AfterEvent(List<Unit> units)
    {
        units = units.OrderByDescending(unit => unit.coords.GetDistance(this.Unit.coords)).ToList();
        foreach(Unit unit in units)
        {
            if (CardSO.isKnockback)
            {
                var direction = (unit.coords - (CardSO.knockbackType == KnockbackType.FromUnit ? this.Unit.coords : TargetCoords)).GetSignDirection();
                if ((int)direction != -1)
                    StartDirection = direction;
                unit.move.OnMoved(StartDirection, CardSO.knockbackPower);
            }
        }
    }

    public void DestroyEvent() => Destroy(gameObject);
}
