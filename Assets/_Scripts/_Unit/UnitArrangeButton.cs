using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitArrangeButton : MonoBehaviour
{
    public UnitData unitData;

    public void ButtonExit()
    {
        ArrangeManager.inst.MouseExit(unitData);
    }
    public void ButtonDown()
    {
        ArrangeManager.inst.MouseDown(unitData);
    }
    public void ButtonUp()
    {
        ArrangeManager.inst.MouseUp(unitData);
    }

}
