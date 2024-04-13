using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Object/UnitSO")]
public class UnitSO : ScriptableObject
{
    public UnitType type;

    [Header("스프라이트")]
    public AnimatorOverrideController animatorController;
    [Header("체력")]
    public int hp;
    [Header("이동")]
    public RangeType rangeType;
    public int range;
    public int cost;
    public bool isJump = false;

    [Space]
    public List<CardInfo> _CardInfo;
}