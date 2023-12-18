using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Item
{
    public string name;
    public Sprite sprite;
    public int energy;

    [Header("���� ǥ��")]
    public RangeType rangeType;
    public int range;

    [Header("���� Ÿ��")]
    public int attackDamage;
    public AttackType attackType;
    [DrawIf("attackType", AttackType.Splash)] public int splashRange = 1;
    [DrawIf("attackType", AttackType.Liner)] public int lineWide = 1;

    [Header("ī�� ����")]
    public int cardCount;
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
