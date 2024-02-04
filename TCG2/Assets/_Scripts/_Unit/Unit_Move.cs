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
        GridManager.Inst.RevertTiles();

        if(StatusManager.CanMove(unit)) return;

        GridManager.Inst.SelectNodes(GetArea(), canMove ? SelectOutline.MoveSelect : SelectOutline.MoveAble);
    }

    public List<HexCoords> GetArea()
    {
        List<HexCoords> selectCoords = new();
        switch (unit.unitData.rangeType)
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
                    selectCoords.Add(hexNode.coords);
                }
                break;
            case RangeType.TransitLiner:
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var coords = unit.coords + hexDirection.Coords() * unit.unitData.range;
                    if (GridManager.Inst.GetTile(coords)?.CanWalk() == true)
                        selectCoords.Add(coords);
                }
                break;
            case RangeType.TransitAround:
                foreach (HexNode hexNode in HexDirectionExtension.TransitArea(unit.coords, unit.unitData.range))
                {
                    if (GridManager.Inst.GetTile(hexNode.coords)?.CanWalk() == true)
                        selectCoords.Add(hexNode.coords);
                }
                break;
        }
        return selectCoords;
    }

    public HexNode TouchArea(HexNode selected)
    {
        return selected;
    }

    public IEnumerator OnMove(HexCoords targetCoords, bool useDotween = true, float dotweenTime = 0.05f, Ease ease = Ease.Linear)
    {
        GridManager.Inst.RevertTiles();
        TurnManager.UseMoveCost(unit.unitData.cost);

        if (useDotween)
        {
            if (unit.unitData.isJump)
            {
                transform.DOMove(targetCoords.Pos, dotweenTime).SetEase(ease);
                yield return YieldInstructionCache.WaitForSeconds(dotweenTime);
            }
            else
            {
                var path = Pathfinding.FindPath(GridManager.Inst.GetTile(unit), GridManager.Inst.GetTile(targetCoords));
                foreach (HexNode tile in path)
                {
                    transform.DOMove(tile.coords.Pos, dotweenTime).SetEase(ease);
                    yield return YieldInstructionCache.WaitForSeconds(dotweenTime);
                }
            }
        }
        else
            transform.position = targetCoords.Pos;

        GridManager.Inst.OnTileMove(unit.coords, targetCoords, unit);
        unit.coords = targetCoords;
        UnitManager.Inst.SetOrder(true);
    }
}
