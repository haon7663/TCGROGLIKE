using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class UnitCard : MonoBehaviour
{
    private Unit _unit;
    private void Awake() => _unit = GetComponent<Unit>();

    public CardSO CardSO { get; private set; }

    public HexCoords OffsetCoords;

    public void SetUp(CardSO cardSO)
    {
        CardSO = cardSO;
    }

    private void Update()
    {
        if (!CardSO)
            return;

        switch (CardSO.targetTraceType)
        {
            case TargetTraceType.Direction:
                _unit.targetCoords = _unit.coords + OffsetCoords;
                break;
            case TargetTraceType.Follow:
                _unit.targetCoords = _unit.targetUnit.coords;
                break;
            case TargetTraceType.Anchor:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetOffsetCoords()
    {
        if (!CardSO)
            return;
        
        switch (CardSO.targetTraceType)
        {
            case TargetTraceType.Direction:
                OffsetCoords = _unit.targetUnit - _unit.coords;
                break;
            case TargetTraceType.Follow:
                _unit.targetCoords = _unit.targetUnit.coords;
                break;
            case TargetTraceType.Anchor:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void DrawRange(CardSO cardSO = null, bool canSelect = true)
    {
        if (!_unit.canAction)
            canSelect = false;

        if (cardSO)
            CardSO = cardSO;
        
        var selectCoords = GetArea(CardSO);

        var areaType = AreaType.Attack;
        switch(CardSO.cardType)
        {
            case CardType.Attack:
                areaType = AreaType.Attack;
                break;
            case CardType.Buff:
                areaType = AreaType.Buff;
                break;
        }
        GridManager.inst.AreaDisplay(areaType, canSelect, GridManager.inst.GetTiles(selectCoords), _unit);
    }
    public List<HexCoords> GetArea(CardSO cardSO, Unit otherUnit = null)
    {
        List<HexCoords> selectCoords = new();
        
        switch (cardSO.rangeType)
        {
            case RangeType.Liner:
                foreach (var hexDirection in HexDirectionExtension.Loop())
                {
                    var floorWide = Mathf.FloorToInt((float)cardSO.lineWidth / 2);
                    for (var j = -floorWide; j <= floorWide; j++)
                    {
                        for (var i = 0; i < cardSO.range; i++)
                        {
                            var coords = _unit.coords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i;
                            var node = GridManager.inst.GetNode(coords);
                            if (node)
                            {
                                if (node.CanWalk() || node.Coords == otherUnit?.coords || cardSO.isPenetrate && !node.OnObstacle)
                                    selectCoords.Add(coords);
                                else if (node.OnUnit)
                                {
                                    selectCoords.Add(coords);
                                    break;
                                }
                                else
                                    break;
                            }
                        }
                    }
                }
                break;
            case RangeType.Area:
                if (cardSO.canSelectAll)
                    selectCoords.AddRange(HexDirectionExtension.Area(_unit.coords, cardSO.range, cardSO.onSelf).Select(hexNode => hexNode.Coords));
                else
                {
                    selectCoords.AddRange(HexDirectionExtension.Loop().Select(hexDirection => _unit.coords + hexDirection.Coords()));
                    selectCoords.AddRange(HexDirectionExtension.Area(_unit.coords, cardSO.range, cardSO.onSelf).Select(hexNode => hexNode.Coords));
                }
                break;
            case RangeType.OurArea:
                var tiles = HexDirectionExtension.Area(_unit.coords, cardSO.range, cardSO.onSelf);
                foreach (var unit in UnitManager.inst.allies.FindAll(x => tiles.Contains(GridManager.inst.GetNode(x))))
                {
                    var tile = GridManager.inst.GetNode(unit);
                    selectCoords.Add(tile.Coords);
                    tiles.Remove(tile);
                }
                GridManager.inst.AreaDisplay(AreaType.Buff, false, tiles, _unit);
                break;
            case RangeType.Self:
                selectCoords.Add(_unit.coords);
                break;
        }
        return selectCoords;
    }
    
    public List<HexNode> GetSelectedArea(HexCoords coords)
    {
        //_unit.Anim_SetTrigger("attackReady");

        var targetCoords = coords - _unit.coords;
        var direction = targetCoords.GetSignDirection();
        if (!targetCoords.ContainsDirection())
        {
            if (_unit.unitSO.type != UnitType.Enemy)
                direction = _unit.coords.GetNearlyMouseDirection();
            else
                direction = _unit.coords.GetNearlyDirection(coords);
        }

        GridManager.inst.RevertSelects();
        List<HexNode> hexNodes = new();

        //Debug.Log(hexDirection.Coords()._q.ToString() + "/" + hexDirection.Coords()._r.ToString());
        switch (CardSO.selectType)
        {
            case SelectType.Single:
                hexNodes.Add(GridManager.inst.GetNode(coords));
                break;
            case SelectType.Wide:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(_unit.coords, direction, CardSO.range));
                break;
            case SelectType.Liner:
                for (var i = -CardSO.bulletNumber/2; i <= CardSO.bulletNumber/2; i++)
                    hexNodes.AddRange(HexDirectionExtension.Liner(_unit.coords, direction.Rotate(i), CardSO.realRange, CardSO.lineWidth, CardSO.isPenetrate));
                break;
            case SelectType.Splash:
                hexNodes.AddRange(HexDirectionExtension.Area(coords, CardSO.splashRange, true));
                break;
            case SelectType.Emission:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(_unit.coords + direction, direction, CardSO.range - 1, true));
                break;
            case SelectType.Entire:
                hexNodes.AddRange(GridManager.inst.GetTiles(GetArea(CardSO)));
                break;
        }
        return hexNodes;
    }
    public IEnumerator UseCard(HexNode node, CardSO cardSO = null)
    {
        if (cardSO)
            CardSO = cardSO;
        if (!node && CardSO.rangeType != RangeType.Self)
            yield break;
        if (CardSO.rangeType == RangeType.Self)
            node = GridManager.inst.GetNode(_unit);
        
        _unit.Anim_SetTrigger("attack");

        TurnManager.UseEnergy(CardSO.energy);
        _unit.Repeat(node.Coords.Pos.x);

        if(CardSO.actionTriggerType == ActionTriggerType.Custom)
            yield return YieldInstructionCache.WaitForSeconds(CardSO.actionTriggerTime);
        switch (CardSO.selectType)
        {
            case SelectType.Single:
                for (var i = 0; i < CardSO.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    Instantiate(CardSO.actionPrefab).Init(_unit, node, CardSO);
                }
                break;
            case SelectType.Liner:
                var direction = (node.Coords - _unit.coords).GetSignDirection();
                for (var i = 0; i < CardSO.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    for (int j = -CardSO.bulletNumber / 2; j <= this.CardSO.bulletNumber / 2; j++)
                        Instantiate(CardSO.actionPrefab).Init(_unit, direction.Rotate(j), CardSO);
                }
                break;
            default:
                for (var i = 0; i < CardSO.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    Instantiate(CardSO.actionPrefab).Init(_unit, node, GetSelectedArea(node.Coords), CardSO);
                }
                break;
        }

        if (CardSO.isMove)
        {
            if (CardSO.isJump)
                _unit.move.PassMove(node.Coords);
            else
                _unit.move.Move(node.Coords);
        }
        GridManager.inst.RevertTiles(_unit);
    }

    public void Cancel()
    {
        _unit.Anim_SetTrigger("cancel");
    }
}