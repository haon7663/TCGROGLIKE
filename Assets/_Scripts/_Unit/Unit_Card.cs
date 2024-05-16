using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class Unit_Card : MonoBehaviour
{
    private Unit _unit;
    private void Awake() => _unit = GetComponent<Unit>();

    public CardInfo CardInfo { get; private set; }
    public CardData CardData { get; private set; }
    
    [HideInInspector] public HexCoords directionCoords; 
    [HideInInspector] public bool canDisplay = false;
    bool isDisplay = false;
    int value = -999;

    public void SetUp(CardInfo cardInfo, int value)
    {
        CardInfo = cardInfo;
        CardData = cardInfo.data;
        this.value = value;
    }

    public void DrawRange(CardData data = null, bool canSelect = true)
    {
        if (StatusManager.CanAction(_unit))
            return;
        
        if(data)
            CardData = data;
        
        var selectCoords = GetArea(CardData);

        var areaType = AreaType.Attack;
        switch(CardData.cardType)
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
    public List<HexCoords> GetArea(CardData data, Unit otherUnit = null)
    {
        var selectCoords = new List<HexCoords>();
        
        switch (data.rangeType)
        {
            case RangeType.Liner:
                foreach (var hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var floorWide = Mathf.FloorToInt((float)data.lineWidth / 2);
                    for (var j = -floorWide; j <= floorWide; j++)
                    {
                        for (var i = 0; i < data.range; i++)
                        {
                            var coords = _unit.coords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i;
                            var node = GridManager.inst.GetTile(coords);
                            if (node)
                            {
                                if (node.CanWalk() || node.Coords == otherUnit?.coords || data.isPenetrate && !node.OnObstacle)
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
                if (data.canSelectAll)
                    foreach (var hexNode in HexDirectionExtension.Area(_unit.coords, data.range, data.onSelf))
                    {
                        selectCoords.Add(hexNode.Coords);
                    }
                else
                {
                    foreach (var hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                    {
                        selectCoords.Add(_unit.coords + hexDirection.Coords());
                    }
                    foreach (var hexNode in HexDirectionExtension.Area(_unit.coords, data.range, data.onSelf))
                    {
                        selectCoords.Add(hexNode.Coords);
                    }
                }
                break;
            case RangeType.OurArea:
                var tiles = HexDirectionExtension.Area(_unit.coords, data.range, data.onSelf);
                foreach (var unit in UnitManager.inst.allies.FindAll(x => tiles.Contains(GridManager.inst.GetTile(x))))
                {
                    var tile = GridManager.inst.GetTile(unit);
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

    public List<HexNode> SelectedArea => GetSelectedArea(GridManager.inst.GetTile(_unit.coords + directionCoords));
    public List<HexNode> GetSelectedArea(HexNode node)
    {
        _unit.Anim_SetTrigger("attackReady");

        var targetCoords = node.Coords - _unit.coords;
        var direction = targetCoords.GetSignDirection();
        if (!targetCoords.ContainsDirection())
        {
            if (_unit.data.type != UnitType.Enemy)
                direction = _unit.coords.GetNearlyMouseDirection();
            else
                direction = _unit.coords.GetNearlyDirection(node.Coords);
        }

        GridManager.inst.RevertAbles();
        List<HexNode> hexNodes = new();

        //Debug.Log(hexDirection.Coords()._q.ToString() + "/" + hexDirection.Coords()._r.ToString());
        switch (CardData.selectType)
        {
            case SelectType.Single:
                hexNodes.Add(node);
                break;
            case SelectType.Wide:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(_unit.coords, direction, CardData.range));
                break;
            case SelectType.Liner:
                for (int i = -CardData.bulletNumber/2; i <= CardData.bulletNumber/2; i++)
                    hexNodes.AddRange(HexDirectionExtension.Liner(_unit.coords, direction.Rotate(i), CardData.realRange, CardData.lineWidth, CardData.isPenetrate));
                break;
            case SelectType.Splash:
                hexNodes.AddRange(HexDirectionExtension.Area(node.Coords, CardData.splashRange, true));
                break;
            case SelectType.Emission:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(_unit.coords + direction, direction, CardData.range - 1, true));
                break;
            case SelectType.Entire:
                hexNodes.AddRange(GridManager.inst.GetTiles(GetArea(CardData)));
                break;
        }
        return hexNodes;
    }
    public IEnumerator UseCard(HexNode node, CardData data = null)
    {
        if (data)
            CardData = data;
        if (!node && CardData.rangeType != RangeType.Self)
            yield break;
        else if (CardData.rangeType == RangeType.Self)
            node = GridManager.inst.GetTile(_unit);


        _unit.Anim_SetTrigger("attack");

        TurnManager.UseEnergy(this.CardData.energy);
        _unit.SetFlipX(_unit.transform.position.x < node.Coords.Pos.x);

        if(this.CardData.actionTriggerType == ActionTriggerType.Custom)
            yield return YieldInstructionCache.WaitForSeconds(this.CardData.actionTriggerTime);
        switch (this.CardData.selectType)
        {
            case SelectType.Single:
                for (int i = 0; i < this.CardData.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    Instantiate(this.CardData.prefab).GetComponent<Action>().Init(_unit, node, this.CardData, value);
                }
                break;
            case SelectType.Liner:
                var direction = (node.Coords - _unit.coords).GetSignDirection();
                for (int i = 0; i < this.CardData.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    for (int j = -this.CardData.bulletNumber / 2; j <= this.CardData.bulletNumber / 2; j++)
                        Instantiate(this.CardData.prefab).GetComponent<Action>().Init(_unit, direction.Rotate(j), this.CardData, value);
                }
                break;
            default:
                for (int i = 0; i < this.CardData.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    Instantiate(this.CardData.prefab).GetComponent<Action>().Init(_unit, node, GetSelectedArea(node), this.CardData, value);
                }
                break;
        }
        if (this.CardData.isMove)
            _unit.move.OnMove(node.Coords, this.CardData.isJump);

        canDisplay = false;
        GridManager.inst.RevertTiles(_unit);
    }

    public void Cancel()
    {
        _unit.Anim_SetTrigger("cancel");
    }
}