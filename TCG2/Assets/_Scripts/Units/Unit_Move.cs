using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RangeType { Liner, Area }

public class Unit_Move : MonoBehaviour
{
    public HexCoords hexCoords = new HexCoords(0, 0);
    public int range;
    public RangeType rangeType;

    void Start()
    {
        transform.position = hexCoords.Pos;
        Invoke(nameof(OnMove), 0.1f);
    }

    void OnMove()
    {
        if(rangeType == RangeType.Liner)
        {
            for (int i = 1; i <= range; i++)
            {
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    GridManager.Inst.OnMoveSelect(hexCoords + hexDirection.Coords() * i);
                }
            }
        }
        else if (rangeType == RangeType.Area)
        {
            for (int i = 1; i <= range; i++)
            {
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    GridManager.Inst.OnMoveSelect(hexCoords + hexDirection.Coords() * i);
                }
            }
        }
    }

    void Update()
    {
        
    }
}
