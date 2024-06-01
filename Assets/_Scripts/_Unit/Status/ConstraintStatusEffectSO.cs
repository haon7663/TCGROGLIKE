using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "StatusEffectSO", menuName = "Scriptable Object/StatusEffectSO/ConstraintStatusEffectSO")]
public class ConstraintStatusEffectSO : StatusEffectSO
{
    [Header("Á¦¾à")]
    [SerializeField] private bool unableMove;
    [SerializeField] private bool unableAction;

    public override void AddEffect(Unit unit)
    {
        if (unableMove)
            unit.canMove = false;
        if (unableAction)
            unit.canAction = false;
        base.AddEffect(unit);
    }
    public override void UpdateRenewal(Unit unit)
    {
        if (unableMove)
            unit.canMove = false;
        if (unableAction)
            unit.canAction = false;
        base.UpdateRenewal(unit);
    }
    public override void RemoveEffect(Unit unit)
    {
        if (unableMove)
            unit.canMove = true;
        if (unableAction)
            unit.canAction = true;
        base.RemoveEffect(unit);
    }
}
