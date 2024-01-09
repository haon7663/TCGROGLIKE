using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum UnitType { Mercenary, Enemy, Commander }

public class UnitManager : MonoBehaviour
{
    public static UnitManager Inst;
    void Awake() => Inst = this;

    [SerializeField] CinemachineVirtualCamera cinevirtual;

    public Unit Commander;
    public List<Unit> Mercenaries;
    public List<Unit> Enemies;

    public static Unit sUnit;
    public static Unit_Move sUnit_Move;
    public static Unit_Card sUnit_Attack;

    void Start()
    {
        foreach(Unit unit in FindObjectsOfType<Unit>())
        {
            switch(unit.unitData.type)
            {
                case UnitType.Commander:
                    Commander = unit;
                    break;
                case UnitType.Mercenary:
                    Mercenaries.Add(unit);
                    break;
                case UnitType.Enemy:
                    Enemies.Add(unit);
                    break;
            }
        }
        TurnManager.OnTurnStarted += OnTurnStarted;
        SelectUnit(Commander);

        CardManager.Inst.StartSet();
    }

    void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }

    void OnTurnStarted(bool myTurn)
    {

    }


    public void SelectUnit(Unit unit)
    {
        if (Enemies.Contains(unit))
        {
            unit.GetComponent<Unit_Move>().DrawArea(false);
        }
        else
        {
            sUnit = unit;
            sUnit_Move = sUnit.GetComponent<Unit_Move>();
            sUnit_Attack = sUnit.GetComponent<Unit_Card>();
            cinevirtual.Follow = sUnit.transform;
            switch (TurnManager.Inst.paze)
            {
                case Paze.Draw | Paze.End | Paze.Enemy:
                    break;
                case Paze.Move:
                    sUnit_Move.DrawArea();
                    break;
                case Paze.Card:
                    break;
            }
        }
    }
}
