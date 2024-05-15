using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Object/UnitData")]
public class UnitData : ScriptableObject
{
    public UnitType type;

    [Header("스프라이트")]
    public Sprite sprite;
    public AnimatorOverrideController animatorController;
    
    [Header("체력")]
    public int hp;
    
    [Header("이동")]
    public RangeType rangeType;
    public int range;
    public int cost;
    public bool isJump = false;

    [Header("카드")]
    public List<CardInfo> cardInfo;
}