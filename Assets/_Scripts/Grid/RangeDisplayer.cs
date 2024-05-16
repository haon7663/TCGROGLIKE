using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeDisplayer : MonoBehaviour
{
    [HideInInspector] public AreaType areaType;
    [HideInInspector] public Unit unit;
    [HideInInspector] public bool canSelect;

    public void Get(AreaType areaType, bool canSelect, Unit unit)
    {
        this.areaType = areaType;
        this.canSelect = canSelect;
        this.unit = unit;
    }

    public void Release(Unit unit = null)
    {
        if (this.unit == unit || !unit)
            gameObject.SetActive(false);
    }
}
