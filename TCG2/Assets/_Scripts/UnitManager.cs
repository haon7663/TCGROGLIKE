using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType { Mercenary, Enemy, Commander }
public enum RangeType { Liner, Area }
public enum AttackType { Single, Wide, Splash, Liner, Emission, Entire,  }

public class UnitManager : Unit
{
    public static UnitManager Inst;
    void Awake() => Inst = this;

    public List<Unit> Commanders;
    public List<Unit> Mercenaries;
    public List<Unit> Enemies;

    public static Unit sUnit;
    public static Unit_Move sUnit_Move;
    public static Unit_Attack sUnit_Attack;

    void Start()
    {
        SelectUnit(Commanders[0]);
    }

    public void SelectUnit(Unit unit)
    {
        sUnit = unit;
        sUnit_Move = sUnit.GetComponent<Unit_Move>();
        sUnit_Attack = sUnit.GetComponent<Unit_Attack>();
    }    
}
