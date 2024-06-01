using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum StatusEffectStackType { Duration, Intensity, IntensityAndDuration, Counter, No }
public enum StatusEffectCalType { Accumulate, Initialization, Each }

[Serializable]
public class StatusEffectData
{
    public StatusEffectSO effectSO;
    public int stack;
}

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager inst;
    private void Awake() => inst = this;

    public IEnumerator AddStatusEffects(List<StatusEffectData> statuses, Unit targetUnit)
    {
        foreach (var status in statuses)
        {
            StatusEffectSO newStatusEffect = null;
            switch (status.effectSO.calculateType)
            {
                case StatusEffectCalType.Accumulate:
                    if (targetUnit.statuses.Exists(item => item.name.Equals(status.effectSO.name)))
                    {
                        newStatusEffect = targetUnit.statuses.Find(item => item.name.Equals(status.effectSO.name));
                        newStatusEffect.AddStack(status.stack);
                        newStatusEffect.InitEffect(targetUnit);
                    }
                    else
                        newStatusEffect = Instantiate(status.effectSO);
                    break;
                case StatusEffectCalType.Initialization:
                    if (targetUnit.statuses.Exists(item => item.name.Equals(status.effectSO.name)))
                    {
                        newStatusEffect = targetUnit.statuses.Find(item => item.name.Equals(status.effectSO.name));
                        newStatusEffect.SetStack(status.stack);
                        newStatusEffect.InitEffect(targetUnit);
                    }
                    else
                        newStatusEffect = Instantiate(status.effectSO);
                    break;
                case StatusEffectCalType.Each:
                    newStatusEffect = Instantiate(status.effectSO);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (newStatusEffect)
            {
                targetUnit.statuses.Add(newStatusEffect);
                newStatusEffect.AddEffect(targetUnit);
                newStatusEffect.SetStack(status.stack);
            }
            
            UIManager.inst.ShowStatusTMP(targetUnit, status.effectSO);
            yield return YieldInstructionCache.WaitForSeconds(0.33f);
        }
        targetUnit.SetActionText();
    }
    
    public void UpdateStatusEffects(Unit unit)
    {
        for(var i = unit.statuses.Count - 1; i >= 0; i--)
        {
            var status = unit.statuses[i];
            status.UpdateEffect(unit);
            
            if (status.Stack > 0)
                continue;
            
            unit.statuses.Remove(status);
            status.RemoveEffect(unit);
        }
    }
    
    public void UpdateRenewals(Unit unit)
    {
        for(var i = unit.statuses.Count - 1; i >= 0; i--)
        {
            var status = unit.statuses[i];
            status.UpdateRenewal(unit);
            
            print(status.name + ": " + status.Stack);
        }
    }

    /*public static bool CanMove(Unit unit)
    {
        var root = unit.statuses.Exists(item => item.name.Equals("Root"));
        var stun = unit.statuses.Exists(item => item.name.Equals("Stun"));

        return !(root || stun);
    }
    public static bool CanAction(Unit unit)
    {
        var stun = unit.statuses.Exists(item => item.name.Equals("Stun"));

        return !stun;
    }*/

    #region Calculate
    public static int Calculate(Unit unit, CardSO cardSO, int value = -999)
    {
        value = value == -999 ? cardSO.value : value;
        switch (cardSO.activeType)
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
    public static int Calculate(Unit unit, Unit targetUnit, CardSO cardSO)
    {
        switch (cardSO.activeType)
        {
            case ActiveType.Attack:
                return CalculateDamage(unit, targetUnit, cardSO.value);
            case ActiveType.Defence:
                return CalculateDefence(unit, targetUnit, cardSO.value);
            case ActiveType.Recovery:
                return CalculateHealth(unit, targetUnit, cardSO.value);
            default:
                break;
        }
        return 0;
    }
    public static int CalculateDamage(Unit unit, int value)
    {
        value = unit.Stats[StatType.GetDamage].GetValue(value);
        
        return value;
    }
    public static int CalculateDamage(Unit unit, Unit targetUnit, int value)
    {
        value = CalculateDamage(unit, value);
        value = targetUnit.Stats[StatType.TakeDamage].GetValue(value);

        return value;
    }
    public static int CalculateDefence(Unit unit, int value)
    {
        return value;
    }
    public static int CalculateDefence(Unit unit, Unit targetUnit, int value)
    {
        value = CalculateDefence(unit, value);
        value = targetUnit.Stats[StatType.TakeDefence].GetValue(value);

        return value;
    }
    public static int CalculateHealth(Unit unit, int value)
    {
        return value;
    }
    public static int CalculateHealth(Unit unit, Unit targetUnit, int value)
    {
        value = CalculateHealth(unit, value);
        value = targetUnit.Stats[StatType.TakeRecovery].GetValue(value);

        return value;
    }
    #endregion
}