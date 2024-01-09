using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Object/UnitSO")]
public class UnitSO : ScriptableObject
{
    public UnitType type;

    [Header("ü��")]
    public int hp;
    [Header("�̵�")]
    public RangeType rangeType;
    public int range;
    public int cost;

    [Space]
    public List<CardSO> _CardSO;
}
