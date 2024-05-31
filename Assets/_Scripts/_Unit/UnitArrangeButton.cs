using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitArrangeButton : MonoBehaviour
{
    [FormerlySerializedAs("unitData")] public UnitSO unitSO;

    public void ButtonExit()
    {
        ArrangeManager.inst.MouseExit(unitSO);
    }
    public void ButtonDown()
    {
        ArrangeManager.inst.MouseDown(unitSO);
    }
    public void ButtonUp()
    {
        ArrangeManager.inst.MouseUp(unitSO);
    }

}
