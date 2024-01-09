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
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    for (int i = 1; i <= unit.unitData.range; i++)
                    {
                        var coords = unit.coords + hexDirection.Coords() * i;
                        if (GridManager.Inst.GetUnit(coords) == null)
                            selectCoords.Add(coords);
                        else
                            break;
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

        GridManager.Inst.OnSelect(selectCoords, canMove ? SelectOutline.MoveSelect : SelectOutline.MoveAble);
    }

    public HexNode TouchArea(HexNode selected)
    {
        return selected;
    }

    public void OnMove(HexCoords targetCoords, bool useDotween = true, float dotweenTime = 0.2f, Ease ease = Ease.Linear)
    {
        GridManager.Inst.RevertTiles();
        TurnManager.UseMoveCost(unit.unitData.cost);

        var target = (Vector3)targetCoords.Pos - Vector3.forward;
        var path = Pathfinding.FindPath(GridManager.Inst.GetTile(unit), GridManager.Inst.GetTile(target));

        if (useDotween)
        {
            Sequence sequence = DOTween.Sequence();
            foreach(HexNode tile in path)
            {
                sequence.Append(transform.DOMove((Vector3)tile.Coords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
            }
        }
        else
            transform.position = target;

        GridManager.Inst.OnTileMove(unit.coords, targetCoords, unit);
        unit.coords = targetCoords;
    }
}
