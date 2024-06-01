using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectSO", menuName = "Scriptable Object/StatusEffectSO/TickDamageStatusEffectSO")]
public class TickDamageStatusEffectSO : StatusEffectSO
{
    public override void UpdateEffect(Unit unit)
    {
        unit.OnDamage(Stack);
        base.UpdateEffect(unit);
    }
}
