using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public enum StatusStackType { Duration, Intensity, IntensityAndDuration, Counter, No }
public enum StatusCalculateType { Accumulate, Initialization, Each }

public class StatusManager : MonoBehaviour
{
    public static StatusManager Inst;
    void Awake() => Inst = this;

    public void StatusActive(Unit unit)
    {
        for(int i = unit.statuses.Count - 1; i >= 0; i--)
        {
            Type t = this.GetType();
            MethodInfo method = t.GetMethod(unit.statuses[i].data.name);

            method?.Invoke(this, new object[] { unit, unit.statuses[i].stack });

            switch (unit.statuses[i].data.stackType)
            {
                case StatusStackType.Duration:
                    unit.statuses[i].stack--;
                    break;
                case StatusStackType.IntensityAndDuration:
                    unit.statuses[i].stack--;
                    break;
            }

            if (unit.statuses[i].stack <= 0)
                unit.statuses.RemoveAt(i);
        }
    }

    public IEnumerator AddUnitStatus(List<StatusInfo> statuses, Unit targetUnit)
    {
        foreach (var status in statuses)
        {
            var newStatus = new StatusInfo(status.data, status.stack);

            switch (newStatus.data.calculateType)
            {
                case StatusCalculateType.Accumulate:
                    if (targetUnit.statuses.Exists(item => item.data.name.Equals(newStatus.data.name)))
                        targetUnit.statuses.Find(item => item.data.name.Equals(newStatus.data.name)).stack += newStatus.stack;
                    else
                        targetUnit.statuses.Add(newStatus);
                    break;
                case StatusCalculateType.Initialization:
                    if (targetUnit.statuses.Exists(item => item.data.name.Equals(newStatus.data.name)))
                        targetUnit.statuses.Find(item => item.data.name.Equals(newStatus.data.name)).stack = newStatus.stack;
                    else
                        targetUnit.statuses.Add(newStatus);
                    break;
                case StatusCalculateType.Each:
                    targetUnit.statuses.Add(newStatus);
                    break;
            }
            UIManager.inst.ShowStatusTMP(targetUnit, newStatus.data);
            yield return YieldInstructionCache.WaitForSeconds(0.33f);
        }
        targetUnit.SetActionText();
    }

    public void Poison(Unit unit, int stack)
    {
        unit.OnDamage(stack);
    }

    public static bool CanMove(Unit unit)
    {
        var root = unit.statuses.Exists(item => item.data.name.Equals("Root"));
        var stun = unit.statuses.Exists(item => item.data.name.Equals("Stun"));

        return !(root || stun);
    }
    public static bool CanAction(Unit unit)
    {
        var stun = unit.statuses.Exists(item => item.data.name.Equals("Stun"));

        return stun;
    }

    #region Calculate
    public static int Calculate(Unit unit, CardData data, int value = -999)
    {
        value = value == -999 ? data.value : value;
        switch (data.activeType)
        {
            case ActiveType.Attack:
                return CalculateDamage(unit, value);
            case ActiveType.Defence:
                return CalculateDefence(unit, value);
            case ActiveType.Recovery:
                return CalculateHealth(unit, value);
            default:
                break;
        }
        return 0;
    }
    public static int Calculate(Unit unit, Unit targetUnit, CardData data)
    {
        switch (data.activeType)
        {
            case ActiveType.Attack:
                return CalculateDamage(unit, targetUnit, data.value);
            case ActiveType.Defence:
                return CalculateDefence(unit, targetUnit, data.value);
            case ActiveType.Recovery:
                return CalculateHealth(unit, targetUnit, data.value);
            default:
                break;
        }
        return 0;
    }
    public static int CalculateDamage(Unit unit, int value)
    {
        var strength = unit.statuses.Find(item => item.data.name.Equals("Strength"))?.stack;
        var weak = unit.statuses.Exists(item => item.data.name.Equals("Weak"));

        var persentage = 1f;
        persentage *= weak ? 0.75f : 1f;

        return Mathf.RoundToInt((value * persentage) + (strength ?? 0));
    }
    public static int CalculateDamage(Unit unit, Unit targetUnit, int value)
    {
        value = CalculateDamage(unit, value);

        var vulnerable = targetUnit.statuses.Exists(item => item.data.name.Equals("Vulnerable"));
        var solid = targetUnit.statuses.Exists(item => item.data.name.Equals("Solid"));

        var persentage = 1f;
        persentage *= vulnerable ? 1.5f : 1f;
        persentage *= solid ? 0.75f : 1f;

        return Mathf.RoundToInt(value * persentage);
    }
    public static int CalculateDefence(Unit unit, int value)
    {
        var persentage = 1f;

        return Mathf.RoundToInt(value * persentage);
    }
    public static int CalculateDefence(Unit unit, Unit targetUnit, int value)
    {
        value = CalculateDefence(unit, value);

        var grievousWounds = targetUnit.statuses.Exists(item => item.data.name.Equals("GrievousWounds"));

        var persentage = 1f;
        persentage *= grievousWounds ? 0.5f : 1f;

        return Mathf.RoundToInt(value * persentage);
    }
    public static int CalculateHealth(Unit unit, int value)
    {
        var persentage = 1f;

        return Mathf.RoundToInt(value * persentage);
    }
    public static int CalculateHealth(Unit unit, Unit targetUnit, int value)
    {
        value = CalculateHealth(unit, value);

        var grievousWounds = targetUnit.statuses.Exists(item => item.data.name.Equals("GrievousWounds"));

        var persentage = 1f;
        persentage *= grievousWounds ? 0.5f : 1f;

        return Mathf.RoundToInt(value * persentage);
    }
    #endregion
}