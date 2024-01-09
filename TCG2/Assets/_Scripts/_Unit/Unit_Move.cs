using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit_Move : MonoBehaviour
{
    Unit unit;
    void Awake() => unit = GetComponent<Unit>();

    public void DrawArea(bool canMove = true)
    {
        if(canMove)
            GridManager.Inst.RevertTiles();

        List<HexCoords> selectCoords = new();
        switch(unit.unitData.rangeType)
        {
            case RangeType.Liner:
                for (int i = 1; i <= unit.unitData.range; i++)
                {
                    foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                    {
                        selectCoords.Add(unit.coords + hexDirection.Coords() * i);
                    }
                }
                break;
            case RangeType.Area:
                foreach (HexNode hexNode in HexDirectionExtension.ReachArea(unit.coords, unit.unitData.range))
                {
                    selectCoords.Add(hexNode.Coords);
                }
                break;
        }

        GridManager.Inst.OnMove(selectCoords, canMove);
    }

    public HexNode TouchArea(HexNode selected)
    {
        return selected;
    }

    public void OnMove(HexCoords targetCoords, bool useDotween = true, float dotweenTime = 0.2f, Ease ease = Ease.Linear)
    {
        GridManager.Inst.RevertTiles();
        TurnManager.UseMoveCost(unit.unitData.cost);

        var targetPos = (Vector3)targetCoords.Pos - Vector3.forward;
        if (useDotween)
            transform.DOMove(targetPos, dotweenTime).SetEase(ease);
        else
            transform.position = targetPos;

        GridManager.Inst.OnTileMove(unit.coords, targetCoords, unit);
        unit.coords = targetCoords;
    }
}
