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
        GridManager.inst.RevertTiles(unit);
        unit.card.Draw(true);
    }

    public void HideAction()
    {
        isShowed = false;
        GridManager.inst.RevertTiles(unit);
        
        if(unit.card.isShouldAction)
            unit.card.DrawSelectedArea();
    }
}
