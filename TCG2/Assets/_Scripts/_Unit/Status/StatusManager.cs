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

    public void Poison(Unit unit, int stack)
    {
        unit.OnDamage(stack);
    }

    public static bool CanMove(Unit unit)
    {
        var root = unit.statuses.Exists(item => item.data.name.Equals("Root"));
        var stun = unit.statuses.Exists(item => item.data.name.Equals("Stun"));

        return root || stun;
    }
    public static bool CanAction(Unit unit)
    {
        var stun = unit.statuses.Exists(item => item.data.name.Equals("Stun"));

        return stun;
    }
    public static int CalculateValue(Unit unit, int value)
    {
        var strength = unit.statuses.Find(item => item.data.name.Equals("Strength"))?.stack;
        print(strength);

        return value + (strength ?? 0);
    }
}

[Serializable]
public class Status
{
    public StatusSO data;
    public int stack;
}
