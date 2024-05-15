using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit_Move : MonoBehaviour
{
    private Unit _unit;
    private void Awake() => _unit = GetComponent<Unit>();

    public List<HexCoords> DrawArea(bool canSelect = true)
    {
        if(!StatusManager.CanMove(_unit)) return null;
        
        var selectCoords = GetArea();
        GridManager.inst.AreaDisplay(AreaType.Move, canSelect, GridManager.inst.GetTiles(selectCoords), _unit);
        return selectCoords;
    }

    public List<HexCoords> GetArea(bool onSelf = false)
    {
        List<HexCoords> selectCoords = new();
        switch (_unit.data.rangeType)
        {
            case RangeType.Liner:
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    for (int i = 1; i <= _unit.data.range; i++)
                    {
                        var coords = _unit.coords + hexDirection.Coords() * i;
                        if (GridManager.inst.GetUnit(coords) == null)
                            selectCoords.Add(coords);
                        else
                            break;
                    }
                }
                break;
            case RangeType.Area:
                foreach (HexNode hexNode in HexDirectionExtension.ReachArea(_unit.coords, _unit.data.range))
                {
                    selectCoords.Add(hexNode.Coords);
                }
                break;
            case RangeType.TransitLiner:
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var coords = _unit.coords + hexDirection.Coords() * _unit.data.range;
                    if (GridManager.inst.GetTile(coords)?.CanWalk() == true)
                        selectCoords.Add(coords);
                }
                break;
            case RangeType.TransitAround:
                foreach (HexNode hexNode in HexDirectionExtension.TransitArea(_unit.coords, _unit.data.range))
                {
                    if (GridManager.inst.GetTile(hexNode.Coords)?.CanWalk() == true)
                        selectCoords.Add(hexNode.Coords);
                }
                break;
        }
        if (onSelf)
            selectCoords.Add(_unit.coords);
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
        GridManager.inst.RevertTiles(_unit);
        TurnManager.UseMoveCost(_unit.data.cost);

        if (!GridManager.inst.SetTileUnit(_unit.coords, targetCoords, _unit))
        {
            print("KnockBack");
        }
        else if (GridManager.inst.Tiles.ContainsKey(targetCoords.Pos))
        {
            if (useDotween)
            {
                if (isJump == null ? _unit.data.isJump : (bool)isJump)
                {
                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(transform.DOMove(targetCoords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
                    sequence.AppendCallback(() => _unit.coords = targetCoords);
                }
                else
                {
                    var path = Pathfinding.FindPath(GridManager.inst.GetTile(_unit), GridManager.inst.GetTile(targetCoords));
                    Sequence sequence = DOTween.Sequence();
                    foreach (HexNode tile in path)
                    {
                        sequence.Append(transform.DOMove(tile.Coords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
                        sequence.AppendCallback(() => _unit.coords = tile.Coords);
                    }
                }
            }
            else
                transform.position = targetCoords.Pos - Vector3.forward;
        }
        UnitManager.inst.SetOrderUnits(true);
    }

    public void OnMoved(HexDirection direction, int range, float dotweenTime = 0.05f, Ease ease = Ease.Linear)
    {
        GridManager.inst.RevertTiles(_unit);
        TurnManager.UseMoveCost(_unit.data.cost);

        Sequence sequence = DOTween.Sequence();
        for (int i = 1; i <= range; i++)
        {
            var targetCoords = _unit.coords + direction.Coords() * i;
            if (!GridManager.inst.SetTileUnit(targetCoords - direction, targetCoords, _unit))
            {
                break;
            }
            else
            {
                sequence.Append(transform.DOMove(targetCoords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
                sequence.AppendCallback(() => _unit.coords = targetCoords);
            }
        }
        UnitManager.inst.SetOrderUnits(true);
    }

    public IEnumerator OnMoveInRange(HexCoords targetCoords, int range, bool useDotween = true, float dotweenTime = 0.05f, Ease ease = Ease.Linear)
    {
        GridManager.inst.RevertTiles(_unit);
        TurnManager.UseMoveCost(_unit.data.cost);

        var targetTile = GridManager.inst.GetTile(targetCoords);
        var path = Pathfinding.FindPath(GridManager.inst.GetTile(_unit), targetTile, _unit.coords == targetCoords || !targetTile.OnUnit);

        if(path.Count > 0)
        {
            int i = 0;
            foreach (HexNode tile in path)
            {
                if (i++ >= range)
                    break;

                transform.DOMove(tile.Coords.Pos - Vector3.forward, dotweenTime).SetEase(ease);
                GridManager.inst.SetTileUnit(_unit.coords, tile.Coords, _unit);
                _unit.coords = tile.Coords;

                yield return YieldInstructionCache.WaitForSeconds(dotweenTime);
            }
        }
        UnitManager.inst.SetOrderUnits(true);
    }
}
