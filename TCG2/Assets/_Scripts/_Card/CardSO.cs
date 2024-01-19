using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CardType { Attack, Buff, }
public enum ActiveType { Damage, Defence, Health }
public enum RangeType { Liner, Area, TransitLiner, TransitDiagonal, TransitAround, OurArea, Self }
public enum SelectType { Single, Wide, Splash, Liner, Emission, Entire, }

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

    [Header("ȿ��")]
    public ActiveType activeType;

    [Header("����")]
    public RangeType rangeType;
    public bool onSelf;
    public int range;
    [DrawIf("rangeType", RangeType.Area)] public bool canSelectAll = true;
    [DrawIf("rangeType", RangeType.Liner)] public int lineWidth = 1;

    [Header("����")]
    public SelectType selectType;
    [DrawIf("selectType", SelectType.Splash)] public int splashRange = 1;
    [DrawIf("selectType", SelectType.Liner)] public int multiShot = 1;
    [DrawIf("selectType", SelectType.Liner)] public bool isPenetrate = false;

    [Header("ī�� ����")]
    public int cardCount;

    [Header("��õ ���� ���")]
    public bool shouldClose = true;
}
