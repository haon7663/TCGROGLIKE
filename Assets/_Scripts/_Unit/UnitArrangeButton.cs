using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitArrangeButton : MonoBehaviour
{
    public UnitData unitData;

    public void ButtonExit()
    {
        UnitArrangeManager.inst.MouseExit(unitData);
    }
    public void ButtonDown()
    {
        UnitArrangeManager.inst.MouseDown(unitData);
    }
    public void ButtonUp()
    {
        UnitArrangeManager.inst.MouseUp(unitData);
    }

}
