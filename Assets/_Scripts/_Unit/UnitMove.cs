using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class UnitMove : MonoBehaviour
{
    private Unit _unit;
    private void Awake() => _unit = GetComponent<Unit>();

    public void DrawArea(bool canSelect = true)
    {
        if (!_unit.canMove)
            canSelect = false;

        var selectCoords = GetArea();
        GridManager.inst.AreaDisplay(AreaType.Move, canSelect, GridManager.inst.GetTiles(selectCoords), _unit);
    }

    public List<HexCoords> GetArea(bool onSelf = false)
    {
        List<HexCoords> selectCoords = new();

        var maxRange = _unit.unitSO.type == UnitType.Enemy ? _unit.unitSO.enemyMoveRange : TurnManager.Inst.MoveCost / (_unit.unitSO.cost == 0 ? 1 : _unit.unitSO.cost);
        selectCoords.AddRange(HexDirectionExtension.ReachArea(_unit.coords, maxRange).Select(hexNode => hexNode.Coords));
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

    public void Move(HexCoords targetCoords, bool useDotween = true, float dotweenTime = 0.05f, Ease ease = Ease.Linear)
    {
        GridManager.inst.RevertTiles(_unit);
        GridManager.inst.SetTileUnit(_unit.coords, targetCoords, _unit);
        
        if (useDotween)
        {
            var path = Pathfinding.FindPath(GridManager.inst.GetNode(_unit), GridManager.inst.GetNode(targetCoords));
            var sequence = DOTween.Sequence();
            foreach (var node in path)
            {
                sequence.Append(transform.DOMove(node.Coords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
                sequence.AppendCallback(() =>
                {
                    _unit.Repeat(node.Coords.Pos.x);
                    _unit.coords = node.Coords;
                });
            }
        }
        else
            transform.position = targetCoords.Pos - Vector3.forward;
        
        UnitManager.inst.SetOrderUnits(true);
    }

    public void PassMove(HexCoords targetCoords, bool useDotween = true, float dotweenTime = 0.1f, Ease ease = Ease.Linear)
    {
        GridManager.inst.RevertTiles(_unit);
        GridManager.inst.SetTileUnit(_unit.coords, targetCoords, _unit);
        
        if (useDotween)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(targetCoords.Pos - Vector3.forward, dotweenTime).SetEase(ease));
            sequence.AppendCallback(() =>
            {
                _unit.Repeat(targetCoords.Pos.x);
                _unit.coords = targetCoords;
            });
        }
        
        transform.position = targetCoords.Pos - Vector3.forward;
        UnitManager.inst.SetOrderUnits(true);
    }

    public void OnMoved(HexDirection direction, int range, float dotweenTime = 0.05f, Ease ease = Ease.Linear)
    {
        GridManager.inst.RevertTiles(_unit);
        TurnManager.UseMoveCost(_unit.unitSO.cost);

        var sequence = DOTween.Sequence();
        for (var i = 1; i <= range; i++)
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

        var targetTile = GridManager.inst.GetNode(targetCoords);
        var path = Pathfinding.FindPath(GridManager.inst.GetNode(_unit), targetTile, _unit.coords == targetCoords || !targetTile.OnUnit);

        if(path.Count > 0)
        {
            var i = 0;
            foreach (var node in path.TakeWhile(node => i++ < range))
            {
                transform.DOMove(node.Coords.Pos - Vector3.forward, dotweenTime).SetEase(ease);
                GridManager.inst.SetTileUnit(_unit.coords, node.Coords, _unit);
                _unit.coords = node.Coords;

                yield return YieldInstructionCache.WaitForSeconds(dotweenTime);
            }
        }
        UnitManager.inst.SetOrderUnits(true);
    }
}
