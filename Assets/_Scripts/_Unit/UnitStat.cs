using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
public class UnitStat
{
    private readonly List<StatModifier> _statModifiers = new List<StatModifier>();
    
    public int GetValue(float value)
    {
        return CalculateFinalValue(value);
    }
    
    public void AddModifier(StatModifier modifier)
    {
        _statModifiers.Add(modifier);
    }
    public void RemoveModifier(StatModifier modifier)
    {
        _statModifiers.Remove(modifier);
    }
    public void RemoveAllModifier()
    {
        _statModifiers.Clear();
    }

    private int CalculateFinalValue(float value)
    {
        var finalValue = value;
        finalValue += _statModifiers.FindAll(stat => stat.statModifierType == StatModifierType.Add).Sum(statModifier => statModifier.value);
        finalValue = _statModifiers.FindAll(stat => stat.statModifierType == StatModifierType.Multiply).Aggregate(finalValue, (current, statModifier) => current * statModifier.value);

        return Mathf.RoundToInt(finalValue);
    }
}
