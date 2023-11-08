using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum RangeType { Liner, Area }

public class Unit_Move : MonoBehaviour
{
    public HexCoords hexCoords = new HexCoords(2, 0);
    public int range;
    public RangeType rangeType;

    void Start()
    {
        transform.position = hexCoords.Pos;

        if (rangeType == RangeType.Liner)
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
            HexDirectionExtension.Area(hexCoords, range);
        }
    }

    public void OnMove(HexCoords targetCoords, bool useDotween = true, float dotweenTime = 0.3f, Ease ease = Ease.InCirc)
    {
        if(useDotween)
        {
            transform.DOMove(targetCoords.Pos, dotweenTime).SetEase(ease);
        }
        else
        {
            transform.position = targetCoords.Pos;
        }
    }
}
