using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class Attack : MonoBehaviour
{
    protected Unit unit;
    protected CardSO card;
    protected HexCoords coords;
    protected List<HexNode> nodes = new List<HexNode>();

    public virtual void Init(Unit unit, HexDirection direction, CardSO card)
    {
        this.unit = unit;
        this.card = card;
        coords = unit.coords;
        transform.position = unit.coords.Pos;
    }
    public virtual void Init(Unit unit, HexNode node, CardSO card)
    {
        this.unit = unit;
        this.card = card;
        coords = node.coords;
        transform.position = coords.Pos;
    }
    public virtual void Init(Unit unit, HexNode node, List<HexNode> nodes, CardSO card)
    {
        this.unit = unit;
        this.card = card;
        this.nodes = nodes;
        coords = node.coords;
        transform.position = coords.Pos;
    }

    public bool ActiveEventValue(HexCoords coords, CardSO card)
    {
        this.coords = coords;
        this.card = card;
        return ActiveEvent();
    }
    public bool ActiveEvent()
    {
        if (nodes.Count <= 1)
        {
            var onUnit = GridManager.Inst.GetUnit(coords);
            if (onUnit != null)
            {
                TypeToEffect(onUnit);
                return true;
            }
        }
        else
        {
            foreach(HexNode node in nodes)
            {
                var onUnit = GridManager.Inst.GetUnit(node);
                if (onUnit != null)
                    TypeToEffect(onUnit);
            }
            return true;
        }
        return false;
    }

    void TypeToEffect(Unit unit)
    {
        switch (card.activeType)
        {
            case ActiveType.Damage:
                unit.OnDamage(card.value);
                break;
            case ActiveType.Defence:
                unit.OnDefence(card.value);
                break;
            case ActiveType.Health:
                unit.OnDefence(card.value);
                break;
        }

        foreach(Status status in card.statuses)
        {
            var newStatus = new Status() { data = status.data, stack = status.stack };

            switch(newStatus.data.calculateType)
            {
                case StatusCalculateType.Accumulate:
                    if (unit.statuses.Exists(item => item.data.name.Equals(newStatus.data.name)))
                        unit.statuses.Find(item => item.data.name.Equals(newStatus.data.name)).stack += newStatus.stack;
                    else
                        unit.statuses.Add(newStatus);
                    break;
                case StatusCalculateType.Initialization:
                    if (unit.statuses.Exists(item => item.data.name.Equals(newStatus.data.name)))
                        unit.statuses.Find(item => item.data.name.Equals(newStatus.data.name)).stack = newStatus.stack;
                    else
                        unit.statuses.Add(newStatus);
                    break;
                case StatusCalculateType.Each:
                    unit.statuses.Add(newStatus);
                    break;
            }
        }
    }

    public void DestroyEvent() => Destroy(gameObject);
}
