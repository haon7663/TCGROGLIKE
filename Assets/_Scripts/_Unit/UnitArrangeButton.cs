using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitArrangeButton : MonoBehaviour
{
    public UnitSO unitData;

    public void ButtonExit()
    {
        UnitArrangeManager.Inst.MouseExit(unitData);
    }
    public void ButtonDown()
    {
        UnitArrangeManager.Inst.MouseDown(unitData);
    }
    public void ButtonUp()
    {
        UnitArrangeManager.Inst.MouseUp(unitData);
    }

}
