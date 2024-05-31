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

    public CardInfo CardInfo { get; private set; }
    public CardSO CardSO { get; private set; }
    
    [HideInInspector] public HexCoords directionCoords; 
    [HideInInspector] public bool canDisplay = false;
    private bool isDisplay = false;
    private int value = -999;

    public void SetUp(CardInfo cardInfo, int value)
    {
        CardInfo = cardInfo;
        CardSO = cardInfo.cardSO;
        this.value = value;
    }

    public void DrawRange(CardSO cardSO = null, bool canSelect = true)
    {
        if (!StatusManager.CanAction(_unit))
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
        var selectCoords = new List<HexCoords>();
        
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
                    foreach (var hexNode in HexDirectionExtension.Area(_unit.coords, cardSO.range, cardSO.onSelf))
                    {
                        selectCoords.Add(hexNode.Coords);
                    }
                else
                {
                    foreach (var hexDirection in HexDirectionExtension.Loop())
                    {
                        selectCoords.Add(_unit.coords + hexDirection.Coords());
                    }
                    foreach (var hexNode in HexDirectionExtension.Area(_unit.coords, cardSO.range, cardSO.onSelf))
                    {
                        selectCoords.Add(hexNode.Coords);
                    }
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

    public List<HexNode> SelectedArea => GetSelectedArea(GridManager.inst.GetNode(_unit.coords + directionCoords));
    public List<HexNode> GetSelectedArea(HexNode node)
    {
        //_unit.Anim_SetTrigger("attackReady");

        var targetCoords = node.Coords - _unit.coords;
        var direction = targetCoords.GetSignDirection();
        if (!targetCoords.ContainsDirection())
        {
            if (_unit.unitSO.type != UnitType.Enemy)
                direction = _unit.coords.GetNearlyMouseDirection();
            else
                direction = _unit.coords.GetNearlyDirection(node.Coords);
        }

        GridManager.inst.RevertSelects();
        List<HexNode> hexNodes = new();

        //Debug.Log(hexDirection.Coords()._q.ToString() + "/" + hexDirection.Coords()._r.ToString());
        switch (CardSO.selectType)
        {
            case SelectType.Single:
                hexNodes.Add(node);
                break;
            case SelectType.Wide:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(_unit.coords, direction, CardSO.range));
                break;
            case SelectType.Liner:
                for (var i = -CardSO.bulletNumber/2; i <= CardSO.bulletNumber/2; i++)
                    hexNodes.AddRange(HexDirectionExtension.Liner(_unit.coords, direction.Rotate(i), CardSO.realRange, CardSO.lineWidth, CardSO.isPenetrate));
                break;
            case SelectType.Splash:
                hexNodes.AddRange(HexDirectionExtension.Area(node.Coords, CardSO.splashRange, true));
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
                    Instantiate(CardSO.prefab).GetComponent<Action>().Init(_unit, node, CardSO, value);
                }
                break;
            case SelectType.Liner:
                var direction = (node.Coords - _unit.coords).GetSignDirection();
                for (var i = 0; i < CardSO.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    for (int j = -CardSO.bulletNumber / 2; j <= this.CardSO.bulletNumber / 2; j++)
                        Instantiate(CardSO.prefab).GetComponent<Action>().Init(_unit, direction.Rotate(j), CardSO, value);
                }
                break;
            default:
                for (var i = 0; i < CardSO.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    Instantiate(CardSO.prefab).GetComponent<Action>().Init(_unit, node, GetSelectedArea(node), CardSO, value);
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

        canDisplay = false;
        GridManager.inst.RevertTiles(_unit);
    }

    public void Cancel()
    {
        _unit.Anim_SetTrigger("cancel");
    }
}