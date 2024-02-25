using System;
using System.Collections.Generic;
using UnityEngine;
public enum CardType { Attack, Buff, }
public enum ActiveType { Attack, Defence, Recovery, }
public enum RangeType { Liner, Area, TransitLiner, TransitDiagonal, TransitAround, OurArea, Self }
public enum SelectType { Single, Wide, Splash, Liner, Emission, Entire, }

public enum KnockbackType { FromUnit, FromPoint }

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public Sprite sprite;
    public GameObject prefab;
    [Space]
    public int energy;
    public int value;
    [Space]
    public CardType cardType;

    [Header("효과")]
    public ActiveType activeType;

    [Header("범위")]
    public RangeType rangeType;
    public bool onSelf;
    public int range;
    [DrawIf("rangeType", RangeType.Area)] public bool canSelectAll = true;
    [DrawIf("rangeType", RangeType.Liner)] public int lineWidth = 1;

    [Header("선택")]
    public SelectType selectType;
    [DrawIf("selectType", SelectType.Splash)] public int splashRange = 1;
    [DrawIf("selectType", SelectType.Liner)] public int realRange;
    [DrawIf("selectType", SelectType.Liner)] public int multiShot = 1;
    [DrawIf("selectType", SelectType.Liner)] public bool isPenetrate = false;

    [Header("특수효과")]
    public List<StatusInfo> statuses;
    public bool isMove;
    [DrawIf("isMove", true)] public bool isJump;
    public bool isKnockback;
    [DrawIf("isKnockback", true)] public KnockbackType knockbackType;
    [DrawIf("isKnockback", true)] public int knockbackPower;

    [Header("추천 선택 경로")]
    public bool shouldClose = true;
    public UseType useType;
    public List<Condition> conditions;
}

[Serializable]
public class CardInfo
{
    [HideInInspector] public Unit unit;
    public CardSO data;
    public int count;

    public int priority;

    public CardInfo(CardInfo cardInfo)
    {
        data = cardInfo.data;
        count = cardInfo.count;
        unit = cardInfo.unit;
        priority = cardInfo.priority;
    }
}
public enum UseType { Able, Should }

[Serializable]
public struct Condition
{
    public ActivatedType activatedType;
    [DrawIf("activatedType", ActivatedType.Health)] public ConditionType conditionType;
    [DrawIf("activatedType", ActivatedType.Health | ActivatedType.Count)] public int value;
}
public enum ActivatedType { Count, Health, Range }
public enum ConditionType { Less, Greater, Equal }