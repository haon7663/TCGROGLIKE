using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CardType { Attack, Buff, }
public enum ActiveType { Attack, Defence, Recovery, }
public enum RangeType { Liner, Area, TransitLiner, TransitDiagonal, TransitAround, OurArea, Self }
public enum SelectType { Single, Wide, Splash, Liner, Emission, Entire, }
public enum KnockbackType { FromUnit, FromPoint }
public enum RecommendedDistanceType { Far, Close, Custom }
public enum ActionTriggerType { Instant, Custom }

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Object/CardData")]
public class CardData : ScriptableObject
{
    public Sprite sprite;
    public GameObject prefab;
    [Space]
    public int energy;
    public int value;
    [Space]
    public CardType cardType;

    [Header("카드 타입")]
    public ActiveType activeType;

    [Header("사거리")]
    public RangeType rangeType;
    public bool onSelf;
    public int range;
    [DrawIf("rangeType", RangeType.Area)] public bool canSelectAll = true;
    [DrawIf("rangeType", RangeType.Liner)] public int lineWidth = 1;

    [Header("사용 효과")]
    public SelectType selectType;
    public int multiShot = 1;
    [DrawIf("selectType", SelectType.Splash)] public int splashRange = 1;
    [DrawIf("selectType", SelectType.Liner)] public int realRange;
    [DrawIf("selectType", SelectType.Liner)] public int bulletNumber = 1;
    [DrawIf("selectType", SelectType.Liner)] public bool isPenetrate = false;

    [Header("특수 효과")]
    public List<StatusInfo> statuses;
    public bool isMove;
    [DrawIf("isMove", true)] public bool isJump;
    public bool isKnockback;
    [DrawIf("isKnockback", true)] public KnockbackType knockbackType;
    [DrawIf("isKnockback", true)] public int knockbackPower;

    [Header("인공지능")]
    public UseType useType;
    public bool compareByMove;
    public RecommendedDistanceType recommendedDistanceType = RecommendedDistanceType.Close;
    [DrawIf("recommendedDistanceType", RecommendedDistanceType.Custom)] public int recommendedDistance = 1;
    [DrawIf("useType", UseType.Should)] public bool isBeforeMove;
    [DrawIf("useType", UseType.Should)] public bool isAfterMove;
    public List<Condition> conditions;

    [Header("애니메이션")]
    public ActionTriggerType actionTriggerType;
    [DrawIf("actionTriggerType", ActionTriggerType.Custom)] public float actionTriggerTime = 0.1f;
}

[Serializable]
public class CardInfo
{
    [HideInInspector] public Unit unit;
    public CardData data;
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