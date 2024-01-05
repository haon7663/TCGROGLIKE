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

    public List<Unit> Commanders;
    public List<Unit> Mercenaries;
    public List<Unit> Enemies;

    public static Unit sUnit;
    public static Unit_Move sUnit_Move;
    public static Unit_Attack sUnit_Attack;

    void Start()
    {
        foreach(Unit unit in FindObjectsOfType<Unit>())
        {
            switch(unit.unitType)
            {
                case UnitType.Commander:
                    Commanders.Add(unit);
                    break;
                case UnitType.Mercenary:
                    Mercenaries.Add(unit);
                    break;
                case UnitType.Enemy:
                    Enemies.Add(unit);
                    break;
            }
        }
        SelectUnit(Commanders[0]);
    }

    public void SelectUnit(Unit unit, bool isEnemy = false)
    {
        sUnit = unit;
        sUnit_Move = sUnit.GetComponent<Unit_Move>();
        sUnit_Attack = sUnit.GetComponent<Unit_Attack>();

        if (isEnemy) return;
        cinevirtual.Follow = sUnit.transform;
    }
}
