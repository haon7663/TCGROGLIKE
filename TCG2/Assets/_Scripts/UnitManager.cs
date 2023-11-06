using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType { Mercenary, Enemy, Commander }

public class UnitManager : MonoBehaviour
{
    public static UnitManager Inst;
    void Awake() => Inst = this;

    public List<Unit> Commanders;
    public List<Unit> Mercenaries;
    public List<Unit> Enemies;
}
