using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class StatusEffectSO : ScriptableObject
{
    [Header("타입")]
    public StatusEffectStackType stackType;
    public StatusEffectCalType calculateType;

    [Header("표기")]
    public Sprite sprite;
    public new string name;
    public string explain;
    public string displayExplain;

    public int Stack { get; private set; }

    public virtual void AddEffect(Unit unit)
    {
    }
    public virtual void UpdateEffect(Unit unit)
    {
        switch (stackType)
        {
            case StatusEffectStackType.Duration:
                Stack--;
                break;
            case StatusEffectStackType.IntensityAndDuration:
                Stack--;
                break;
            case StatusEffectStackType.Intensity:
                break;
            case StatusEffectStackType.Counter:
                break;
            case StatusEffectStackType.No:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public virtual void UpdateRenewal(Unit unit)
    {
    }
    public virtual void RemoveEffect(Unit unit)
    {
    }
    public virtual void InitEffect(Unit unit)
    {
    }
    
    public void SetStack(int stack) => Stack = stack;
    public void AddStack(int stack) => Stack += stack;
}