using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Object/UnitSO")]
public class UnitSO : ScriptableObject
{
    public UnitType type;

    [Header("체력")]
    public int hp;
    [Header("이동")]
    public RangeType rangeType;
    public int range;
    public int cost;

    [Space]
    public List<CardSO> _CardSO;
}
