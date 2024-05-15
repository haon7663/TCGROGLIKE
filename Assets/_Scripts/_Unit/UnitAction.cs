using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAction : MonoBehaviour
{
    public Unit unit;
    public bool isShowed;

    public void ShowAction()
    {
        isShowed = true;
        unit.card.DrawRange(null, false);
    }

    public void HideAction()
    {
        isShowed = false;
        GridManager.inst.RevertTiles(unit);
    }
}
