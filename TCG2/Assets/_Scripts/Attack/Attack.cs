using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Pool;

public abstract class Attack : MonoBehaviour
{
    protected Unit unit;
    protected CardSO data;
    protected HexCoords coords;
    protected List<HexNode> nodes = new List<HexNode>();

    HexDirection saveDirection = HexDirection.Default;
    int value;

    public virtual void Init(Unit unit, HexDirection direction, CardSO data, int value = -999)
    {
        this.unit = unit;
        this.data = data;
        coords = unit.coords;
        transform.position = unit.coords.Pos;
        saveDirection = direction;
        this.value = value;
    }
    public virtual void Init(Unit unit, HexNode node, CardSO data, int value = -999)
    {
        this.unit = unit;
        this.data = data;
        coords = node.coords;
        transform.position = coords.Pos;
        this.value = value;
    }
    public virtual void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO data, int value = -999)
    {
        this.unit = unit;
        this.data = data;
        this.nodes = nodes;
        coords = node.coords;
        transform.position = coords.Pos;
        this.value = value;
    }

    public bool ActiveEventValue(HexCoords coords, CardSO data)
    {
        this.coords = coords;
        this.data = data;
        return ActiveEvent();
    }
    public bool ActiveEvent()
    {
        List<Unit> afterEventUnits = new();
        if (nodes.Count <= 1)
        {
            var onUnit = GridManager.Inst.GetUnit(coords);
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
                var onUnit = GridManager.Inst.GetUnit(node);
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
        var lastValue = value == -999 ? data.value : value;
        switch (data.activeType)
        {
            case ActiveType.Attack:
                targetUnit.OnDamage(StatusManager.CalculateDamage(unit, targetUnit, lastValue));
                break;
            case ActiveType.Defence:
                targetUnit.OnDefence(StatusManager.CalculateDefence(unit, targetUnit, lastValue));
                break;
            case ActiveType.Recovery:
                targetUnit.OnHealth(StatusManager.CalculateHealth(unit, targetUnit, lastValue));
                break;
        }

        if (!targetUnit) 
            return false;

        StatusManager.Inst.AddUnitStatus(data.statuses, targetUnit);
        return true;
    }
    void AfterEvent(List<Unit> units)
    {
        units = units.OrderByDescending(unit => unit.coords.GetDistance(this.unit.coords)).ToList();
        foreach(Unit unit in units)
        {
            if (data.isKnockback)
            {
                var direction = (unit.coords - (data.knockbackType == KnockbackType.FromUnit ? this.unit.coords : coords)).GetSignDirection();
                if ((int)direction != -1)
                    saveDirection = direction;
                unit.move.OnMove(saveDirection, data.knockbackPower);
            }
        }
    }

    public void DestroyEvent() => Destroy(gameObject);
}
