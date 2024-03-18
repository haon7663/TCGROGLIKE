using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayNode : MonoBehaviour
{
    public AreaType areaType;
    public bool canSelect;
    public Unit unit;

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
