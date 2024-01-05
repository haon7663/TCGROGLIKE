using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit_Move : MonoBehaviour
{
    Unit unit;
    void Awake() => unit = GetComponent<Unit>();

    public int range;
    public RangeType rangeType;

    void OnEnable()
    {
        if (rangeType == RangeType.Liner)
        {
            for (int i = 1; i <= range; i++)
            {
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    GridManager.Inst.OnMoveSelect(unit.coords + hexDirection.Coords() * i);
                }
            }
        }
        else if (rangeType == RangeType.Area)
        {
            foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, range))
            {
                GridManager.Inst.OnMoveSelect(hexNode.Coords);
            }
        }
    }
    void OnDisable()
    {
        GridManager.Inst.RevertTiles();
    }

    public HexNode GetArea(HexNode hexNode)
    {
        GridManager.Inst.RevertAbles();

        return hexNode;
    }

    public void OnMove(HexCoords targetCoords, bool useDotween = true, float dotweenTime = 0.2f, Ease ease = Ease.Linear)
    {
        GridManager.Inst.RevertTiles();
        if (useDotween)
            transform.DOMove(targetCoords.Pos, dotweenTime).SetEase(ease);
        else
            transform.position = targetCoords.Pos;

        GridManager.Inst.OnTileMove(unit.coords, targetCoords, unit);
        unit.coords = targetCoords;
    }
}
