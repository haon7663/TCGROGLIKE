using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CardType { Attack, Defence, }
public enum RangeType { Liner, Area, Entire, Our }
public enum AttackType { Single, Wide, Splash, Liner, Emission, Entire, }


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

    [Header("범위 표시")]
    public RangeType rangeType;
    [DrawIf("cardType", CardType.Attack)] [DrawIf("rangeType", RangeType.Area | RangeType.Liner)] public int range;
    [DrawIf("cardType", CardType.Attack)] [DrawIf("rangeType", RangeType.Area)] public bool canSelectAll = true;

    [Header("공격 타입")]
    [DrawIf("cardType", CardType.Attack)] public AttackType attackType;
    [DrawIf("attackType", AttackType.Splash)] [DrawIf("cardType", CardType.Attack)] public int splashRange = 1;
    [DrawIf("attackType", AttackType.Liner)] [DrawIf("cardType", CardType.Attack)] public int lineWidth = 1;
    [DrawIf("attackType", AttackType.Liner)] [DrawIf("cardType", CardType.Attack)] public int multiShot = 1;
    [DrawIf("attackType", AttackType.Liner)] [DrawIf("cardType", CardType.Attack)] public bool isPenetrate = false;

    [Header("카드 개수")]
    public int cardCount;
}
