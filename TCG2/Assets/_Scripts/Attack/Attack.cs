using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class Attack : MonoBehaviour
{
    [HideInInspector] public HexCoords coords;
    [HideInInspector] public CardSO card;

    public abstract void Init(Unit unit, HexDirection direction, CardSO card);
    public abstract void Init(Unit unit, HexNode hexNode, CardSO card);

    public bool ActiveEventValue(HexCoords coords, CardSO card)
    {
        this.coords = coords;
        this.card = card;
        return ActiveEvent();
    }
    public bool ActiveEvent()
    {
        var onUnit = GridManager.Inst.GetUnit(coords);
        if (onUnit != null)
        {
            switch (card.activeType)
            {
                case ActiveType.Damage:
                    onUnit.OnDamage(card.value);
                    break;
                case ActiveType.Defence:
                    onUnit.OnDefence(card.value);
                    break;
                case ActiveType.Health:
                    onUnit.OnDefence(card.value);
                    break;
            }
            return true;
        }
        return false;
    }

    public void DestroyEvent() => Destroy(gameObject);
}
