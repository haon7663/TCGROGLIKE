using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Object/UnitSO")]
public class UnitSO : ScriptableObject
{
    public UnitType type;

    [Header("��������Ʈ")]
    public AnimatorOverrideController animatorController;
    [Header("ü��")]
    public int hp;
    [Header("�̵�")]
    public RangeType rangeType;
    public int range;
    public int cost;
    public bool isJump = false;

    [Space]
    public List<CardInfo> _CardInfo;
}