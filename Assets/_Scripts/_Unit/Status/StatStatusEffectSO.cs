using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectSO", menuName = "Scriptable Object/StatusEffectSO/StatStatusEffectSO")]
public class StatStatusEffectSO : StatusEffectSO
{
    [Header("능력치 변경")]
    [SerializeField] private StatType statType;
    [SerializeField] private StatModifierType statModifierType;
    [DrawIf("statModifierType", StatModifierType.Multiply)] [SerializeField] private float value;

    private StatModifier _statModifier;
    
    public override void AddEffect(Unit unit)
    {
        SetModifier();
        unit.Stats[statType].AddModifier(_statModifier);
        base.AddEffect(unit);
    }

    public override void RemoveEffect(Unit unit)
    {
        unit.Stats[statType].RemoveModifier(_statModifier);
        base.RemoveEffect(unit);
    }

    public override void InitEffect(Unit unit)
    {
        unit.Stats[statType].RemoveModifier(_statModifier);
        SetModifier();
        unit.Stats[statType].AddModifier(_statModifier);
        base.InitEffect(unit);
    }

    private void SetModifier()
    {
        _statModifier = statModifierType switch
        {
            StatModifierType.Multiply => new StatModifier(statModifierType, value),
            StatModifierType.Add => new StatModifier(statModifierType, Stack),
            _ => _statModifier
        };
    }
}
