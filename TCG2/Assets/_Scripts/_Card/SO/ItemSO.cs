using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Item
{
    public string name;
    public Sprite sprite;
    public int energy;

    [Header("범위 표시")]
    public RangeType rangeType;
    public int range;

    [Header("공격 타입")]
    public int attackDamage;
    public AttackType attackType;
    [DrawIf("attackType", AttackType.Splash)] public int splashRange = 1;
    [DrawIf("attackType", AttackType.Liner)] public int lineWide = 1;

    [Header("카드 개수")]
    public int cardCount;
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
