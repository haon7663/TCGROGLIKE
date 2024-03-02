using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { Attack, Buff, }
public enum ActiveType { Attack, Defence, Recovery, }
public enum RangeType { Liner, Area, TransitLiner, TransitDiagonal, TransitAround, OurArea, Self }
public enum SelectType { Single, Wide, Splash, Liner, Emission, Entire, }
public enum KnockbackType { FromUnit, FromPoint }
public enum RecommendedDistanceType { Far, Close, Custom }
public enum ActionTriggerType { Instant, Custom }

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
    public int multiShot = 1;
    [DrawIf("selectType", SelectType.Splash)] public int splashRange = 1;
    [DrawIf("selectType", SelectType.Liner)] public int realRange;
    [DrawIf("selectType", SelectType.Liner)] public int bulletNumber = 1;
    [DrawIf("selectType", SelectType.Liner)] public bool isPenetrate = false;

    [Header("특수효과")]
    public List<StatusInfo> statuses;
    public bool isMove;
    [DrawIf("isMove", true)] public bool isJump;
    public bool isKnockback;
    [DrawIf("isKnockback", true)] public KnockbackType knockbackType;
    [DrawIf("isKnockback", true)] public int knockbackPower;

    [Header("추천 선택 경로")]
    public UseType useType;
    public RecommendedDistanceType recommendedDistanceType = RecommendedDistanceType.Close;
    [DrawIf("recommendedDistanceType", RecommendedDistanceType.Custom)] public int recommendedDistance = 1;
    [DrawIf("useType", UseType.Should)] public bool isBeforeMove;
    [DrawIf("useType", UseType.Should)] public bool isAfterMove;
    public List<Condition> conditions;

    [Header("비주얼")]
    public ActionTriggerType actionTriggerType;
    [DrawIf("actionTriggerType", ActionTriggerType.Custom)] public float actionTriggerTime = 0.1f;
}

[Serializable]
public class CardInfo
{
    [HideInInspector] public Unit unit;
    public CardSO data;
    public int count;
    public int priority;
    public int turnCount;

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
    [DrawIf("activatedType", ActivatedType.Count)] public int turnCount;
    [DrawIf("activatedType", ActivatedType.Health)] public ConditionType conditionType;
    [DrawIf("activatedType", ActivatedType.Health)] public int value;
}
public enum ActivatedType { Count, Health, Range }
public enum ConditionType { Less, Greater, Equal }