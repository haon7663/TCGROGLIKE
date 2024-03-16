using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit_Move : MonoBehaviour
{
    Unit unit;
    void Awake() => unit = GetComponent<Unit>();

    public List<HexCoords> DrawArea(bool canSelect = true)
    {
        if(!StatusManager.CanMove(unit)) return null;

        var selectCoords = GetArea();
        GridManager.Inst.SelectNodes(selectCoords, canSelect ? SelectOutline.MoveSelect : SelectOutline.MoveAble);
        return selectCoords;
    }

    public List<HexCoords> GetArea(bool onSelf = false)
    {
        List<HexCoords> selectCoords = new();
        switch (unit.data.rangeType)
        {
            case RangeType.Liner:
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    for (int i = 1; i <= unit.data.range; i++)
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
                foreach (HexNode hexNode in HexDirectionExtension.ReachArea(unit.coords, unit.data.range))
                {
                    selectCoords.Add(hexNode.coords);
                }
                break;
            case RangeType.TransitLiner:
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var coords = unit.coords + hexDirection.Coords() * unit.data.range;
                    if (GridManager.Inst.GetTile(coords)?.CanWalk() == true)
                        selectCoords.Add(coords);
                }
                break;
            case RangeType.TransitAround:
                foreach (HexNode hexNode in HexDirectionExtension.TransitArea(unit.coords, unit.data.range))
                {
                    if (GridManager.Inst.GetTile(hexNode.coords)?.CanWalk() == true)
                        selectCoords.Add(hexNode.coords);
                }
                break;
        }
        if (onSelf)
            selectCoords.Add(unit.coords);
        return selectCoords;
    }

    public HexNode TouchArea(HexNode selected)
    {
        //Debug.Log("TouchArea");
        //transform.position = selected.coords.Pos + Vector3.forward;
        return selected;
    }

    public void OnMove(HexCoords targetCoords, bool? isJump = null, bool useDotween = true, float dotweenTime = 0.05f, Ease ease = Ease.Linear)
    {
        GridManager.Inst.RevertTiles();
        TurnManager.UseMoveCost(unit.data.cost);

        if (!GridManager.Inst.SetTileUnit(unit.coords, targetCoords, unit))
        {
            print("KnockBack");
        }
        else if (GridManager.Inst.Tiles.ContainsKey(targetCoords.Pos))
        {
            if (useDotween)
            {
                if (isJump == null ? unit.data.isJump : (bool)isJump)
                {
                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(transform.DOMove(targetCoords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
                    sequence.AppendCallback(() => unit.coords = targetCoords);
                }
                else
                {
                    var path = Pathfinding.FindPath(GridManager.Inst.GetTile(unit), GridManager.Inst.GetTile(targetCoords));
                    Sequence sequence = DOTween.Sequence();
                    foreach (HexNode tile in path)
                    {
                        sequence.Append(transform.DOMove(tile.coords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
                        sequence.AppendCallback(() => unit.coords = tile.coords);
                    }
                }
            }
            else
                transform.position = targetCoords.Pos - Vector3.forward;
        }
        UnitManager.Inst.SetOrder(true);
    }

    public void OnMoved(HexDirection direction, int range, float dotweenTime = 0.05f, Ease ease = Ease.Linear)
    {
        GridManager.Inst.RevertTiles();
        TurnManager.UseMoveCost(unit.data.cost);

        Sequence sequence = DOTween.Sequence();
        for (int i = 1; i <= range; i++)
        {
            var targetCoords = unit.coords + direction.Coords() * i;
            if (!GridManager.Inst.SetTileUnit(targetCoords - direction, targetCoords, unit))
            {
                break;
            }
            else
            {
                sequence.Append(transform.DOMove(targetCoords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
                sequence.AppendCallback(() => unit.coords = targetCoords);
            }
        }
        UnitManager.Inst.SetOrder(true);
    }

    public IEnumerator OnMoveInRange(HexCoords targetCoords, int range, bool useDotween = true, float dotweenTime = 0.05f, Ease ease = Ease.Linear)
    {
        GridManager.Inst.RevertTiles();
        TurnManager.UseMoveCost(unit.data.cost);

        var targetTile = GridManager.Inst.GetTile(targetCoords);
        var path = Pathfinding.FindPath(GridManager.Inst.GetTile(unit), targetTile, unit.coords == targetCoords || !targetTile.onUnit);

        if(path.Count > 0)
        {
            int i = 0;
            foreach (HexNode tile in path)
            {
                if (i++ >= range)
                    break;

                transform.DOMove(tile.coords.Pos - Vector3.forward, dotweenTime).SetEase(ease);
                GridManager.Inst.SetTileUnit(unit.coords, tile.coords, unit);
                unit.coords = tile.coords;

                yield return YieldInstructionCache.WaitForSeconds(dotweenTime);
            }
        }
        UnitManager.Inst.SetOrder(true);
    }
}
