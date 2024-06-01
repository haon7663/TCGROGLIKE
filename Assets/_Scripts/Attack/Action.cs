using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Pool;

public abstract class Action : MonoBehaviour
{
    protected Unit unit;
    protected CardSO cardSO;
    protected HexCoords coords;
    protected List<HexNode> nodes = new List<HexNode>();

    HexDirection saveDirection = HexDirection.Default;
    int value;

    public virtual void Init(Unit unit, HexDirection direction, CardSO cardSO, int value = -999)
    {
        this.unit = unit;
        this.cardSO = cardSO;
        coords = unit.coords;
        transform.position = unit.coords.Pos;
        saveDirection = direction;
        this.value = value;
    }
    public virtual void Init(Unit unit, HexNode node, CardSO cardSO, int value = -999)
    {
        this.unit = unit;
        this.cardSO = cardSO;
        coords = node.Coords;
        transform.position = coords.Pos;
        this.value = value;
    }
    public virtual void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO cardSO, int value = -999)
    {
        this.unit = unit;
        this.cardSO = cardSO;
        this.nodes = nodes;
        coords = node.Coords;
        transform.position = coords.Pos;
        this.value = value;
    }

    public bool ActiveEventValue(HexCoords coords, CardSO cardSO)
    {
        this.coords = coords;
        this.cardSO = cardSO;
        return ActiveEvent();
    }
    public bool ActiveEvent()
    {
        List<Unit> afterEventUnits = new();
        if (nodes.Count <= 1)
        {
            var onUnit = GridManager.inst.GetUnit(coords);
            if (onUnit != null)
            {
                TypeToEffect(onUnit);
                afterEventUnits.Add(onUnit);
                AfterEvent(afterEventUnits);
                return true;
            }
        }
        else
        {
            foreach(HexNode node in nodes)
            {
                var onUnit = GridManager.inst.GetUnit(node);
                if (onUnit != null)
                {
                    if(TypeToEffect(onUnit))
                        afterEventUnits.Add(onUnit);
                }
            }
            AfterEvent(afterEventUnits);
            return true;
        }
        return false;
    }

    bool TypeToEffect(Unit targetUnit)
    {
        var lastValue = value == -999 ? cardSO.value : value;
        switch (cardSO.activeType)
        {
            case ActiveType.Attack:
                targetUnit.OnDamage(StatusEffectManager.CalculateDamage(unit, targetUnit, lastValue));
                break;
            case ActiveType.Defence:
                targetUnit.OnDefence(StatusEffectManager.CalculateDefence(unit, targetUnit, lastValue));
                break;
            case ActiveType.Recovery:
                targetUnit.OnHealth(StatusEffectManager.CalculateHealth(unit, targetUnit, lastValue));
                break;
        }

        if (!targetUnit) 
            return false;

        StartCoroutine(StatusEffectManager.inst.AddStatusEffects(cardSO.statuses, targetUnit));
        return true;
    }
    void AfterEvent(List<Unit> units)
    {
        units = units.OrderByDescending(unit => unit.coords.GetDistance(this.unit.coords)).ToList();
        foreach(Unit unit in units)
        {
            if (cardSO.isKnockback)
            {
                var direction = (unit.coords - (cardSO.knockbackType == KnockbackType.FromUnit ? this.unit.coords : coords)).GetSignDirection();
                if ((int)direction != -1)
                    saveDirection = direction;
                unit.move.OnMoved(saveDirection, cardSO.knockbackPower);
            }
        }
    }

    public void DestroyEvent() => Destroy(gameObject);
}
