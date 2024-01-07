using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit_Move : MonoBehaviour
{
    Unit unit;
    void Awake() => unit = GetComponent<Unit>();

    public int range;
    public int cost;
    public RangeType rangeType;

    public void DrawArea(bool canMove = true)
    {
        if(canMove)
            GridManager.Inst.RevertTiles();

        switch(rangeType)
        {
            case RangeType.Liner:
                for (int i = 1; i <= range; i++)
                {
                    foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                    {
                        GridManager.Inst.OnMove(unit.coords + hexDirection.Coords() * i, canMove);
                    }
                }
                break;
            case RangeType.Area:
                for (int i = 1; i <= range; i++)
                {
                    foreach (HexNode hexNode in HexDirectionExtension.ReachArea(unit.coords, range))
                    {
                        GridManager.Inst.OnMove(hexNode.Coords, canMove);
                    }
                }
                break;
        }
    }

    public HexNode TouchArea(HexNode selected)
    {
        return selected;
    }

    public void OnMove(HexCoords targetCoords, bool useDotween = true, float dotweenTime = 0.2f, Ease ease = Ease.Linear)
    {
        GridManager.Inst.RevertTiles();
        TurnManager.UseMoveCost(cost);
        Debug.Log(TurnManager.Inst.MoveCost);

        var targetPos = (Vector3)targetCoords.Pos - Vector3.forward;
        if (useDotween)
            transform.DOMove(targetPos, dotweenTime).SetEase(ease);
        else
            transform.position = targetPos;

        GridManager.Inst.OnTileMove(unit.coords, targetCoords, unit);
        unit.coords = targetCoords;
    }
}
