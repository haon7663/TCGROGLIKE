using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Unit_Card : MonoBehaviour
{
    Unit unit;
    void Awake() => unit = GetComponent<Unit>();

    [HideInInspector] public CardInfo Info;
    [HideInInspector] public CardSO data;
    [HideInInspector] public HexCoords directionCoords; 
    [HideInInspector] public bool canDisplay = false;
    bool isDisplay = false;
    int value = -999;

    public void SetUp(CardInfo cardInfo, int value)
    {
        Info = cardInfo;
        data = cardInfo.data;
        this.value = value;
    }

    public List<HexCoords> DrawArea(CardSO data = null, bool canSelect = true)
    {
        if (StatusManager.CanAction(unit)) return null;

        if(data)
            this.data = data;

        List<HexCoords> selectCoords = GetArea(this.data);

        AreaType areaType = AreaType.Attack;
        switch(this.data.cardType)
        {
            case CardType.Attack:
                areaType = AreaType.Attack;
                break;
            case CardType.Buff:
                areaType = AreaType.Buff;
                break;
        }
        GridManager.Inst.SelectNodes(areaType, canSelect, selectCoords, unit);
        return selectCoords;
    }
    public List<HexCoords> GetArea(CardSO data, Unit otherUnit = null)
    {
        List<HexCoords> selectCoords = new();
        switch (data.rangeType)
        {
            case RangeType.Liner:
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var floorWide = Mathf.FloorToInt((float)data.lineWidth / 2);
                    for (int j = -floorWide; j <= floorWide; j++)
                    {
                        for (int i = 0; i < data.range; i++)
                        {
                            var coords = unit.coords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i;
                            var node = GridManager.Inst.GetTile(coords);
                            if (node)
                            {
                                if (node.CanWalk() || node.coords == otherUnit?.coords || data.isPenetrate && !node.onObstacle)
                                    selectCoords.Add(coords);
                                else if (node.onUnit)
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
                    foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, data.range, data.onSelf))
                    {
                        selectCoords.Add(hexNode.coords);
                    }
                else
                {
                    foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                    {
                        selectCoords.Add(unit.coords + hexDirection.Coords());
                    }
                    foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, data.range, data.onSelf))
                    {
                        selectCoords.Add(hexNode.coords);
                    }
                }
                break;
            case RangeType.OurArea:
                List<HexNode> AreaNodes = HexDirectionExtension.Area(unit.coords, data.range, data.onSelf);
                foreach (var unit in GridManager.Inst.OnTileUnits.Values.Where(t => t.data.type != UnitType.Enemy && AreaNodes.Contains(GridManager.Inst.GetTile(t))))
                {
                    var tile = GridManager.Inst.GetTile(unit);
                    selectCoords.Add(tile.coords);
                    AreaNodes.Remove(tile);
                }
                GridManager.Inst.SelectNodes(AreaType.Buff, false, AreaNodes, unit);
                break;
            case RangeType.Self:
                selectCoords.Add(unit.coords);
                break;
        }
        return selectCoords;
    }
    public List<HexNode> GetSelectedArea(HexNode node)
    {
        unit.Anim_SetTrigger("attackReady");

        var targetCoords = node.coords - unit.coords;
        var direction = targetCoords.GetSignDirection();
        if (!targetCoords.ContainsDirection())
        {
            if (unit.data.type != UnitType.Enemy)
                direction = unit.coords.GetNearlyMouseDirection();
            else
                direction = unit.coords.GetNearlyDirection(node.coords);
        }

        GridManager.Inst.RevertAbles();
        List<HexNode> hexNodes = new();

        //Debug.Log(hexDirection.Coords()._q.ToString() + "/" + hexDirection.Coords()._r.ToString());
        switch (data.selectType)
        {
            case SelectType.Single:
                hexNodes.Add(node);
                break;
            case SelectType.Wide:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(unit.coords, direction, data.range));
                break;
            case SelectType.Liner:
                for (int i = -data.bulletNumber/2; i <= data.bulletNumber/2; i++)
                    hexNodes.AddRange(HexDirectionExtension.Liner(unit.coords, direction.Rotate(i), data.realRange, data.lineWidth, data.isPenetrate));
                break;
            case SelectType.Splash:
                hexNodes.AddRange(HexDirectionExtension.Area(node.coords, data.splashRange, true));
                break;
            case SelectType.Emission:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(unit.coords + direction, direction, data.range - 1, true));
                break;
            case SelectType.Entire:
                hexNodes.AddRange(GridManager.Inst.CoordsToNodes(GetArea(data)));
                break;
        }
        return hexNodes;
    }

    public IEnumerator UseCard(HexNode node, CardSO data = null)
    {
        node.coords.DebugQRS();
        if (data)
            this.data = data;
        if (!node && this.data.rangeType != RangeType.Self)
            yield break;
        else if (this.data.rangeType == RangeType.Self)
            node = GridManager.Inst.GetTile(unit);


        unit.Anim_SetTrigger("attack");

        TurnManager.UseEnergy(this.data.energy);
        unit.SetFlipX(unit.transform.position.x < node.coords.Pos.x);

        if(this.data.actionTriggerType == ActionTriggerType.Custom)
            yield return YieldInstructionCache.WaitForSeconds(this.data.actionTriggerTime);
        switch (this.data.selectType)
        {
            case SelectType.Single:
                for (int i = 0; i < this.data.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    Instantiate(this.data.prefab).GetComponent<Action>().Init(unit, node, this.data, value);
                }
                break;
            case SelectType.Liner:
                var direction = (node.coords - unit.coords).GetSignDirection();
                for (int i = 0; i < this.data.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    for (int j = -this.data.bulletNumber / 2; j <= this.data.bulletNumber / 2; j++)
                        Instantiate(this.data.prefab).GetComponent<Action>().Init(unit, direction.Rotate(j), this.data, value);
                }
                break;
            default:
                for (int i = 0; i < this.data.multiShot; i++)
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.05f);
                    Instantiate(this.data.prefab).GetComponent<Action>().Init(unit, node, GetSelectedArea(node), this.data, value);
                }
                break;
        }
        if (this.data.isMove)
            unit.move.OnMove(node.coords, this.data.isJump);

        GridManager.Inst.RevertTiles(unit);
    }

    public void Cancel()
    {
        unit.Anim_SetTrigger("cancel");
    }

    public GameObject lineDot;
    public void DisplayObjects(bool isActive)
    {
        if (isActive && !isDisplay)
        {
            var selectedTiles = GetSelectedArea(GridManager.Inst.GetTile(unit.coords + directionCoords));

            if (data.selectType == SelectType.Liner)
            {
                int count = 1;
                bool isHorizontal;
                switch (directionCoords.GetSignDirection())
                {
                    case HexDirection.W:
                        isHorizontal = true;
                        break;
                    case HexDirection.E:
                        isHorizontal = true;
                        break;
                    default:
                        isHorizontal = false;
                        break;
                }
                for (int i = 0; i < selectedTiles.Count; i++)
                {
                    for (int j = 0; j <= (isHorizontal ? 1 : selectedTiles[i].coords.GetDistance(unit.coords) % 2); j++)
                    {
                        Instantiate(lineDot, unit.coords.Pos + (directionCoords.GetSignDirection().Coords() * count).Pos * (isHorizontal ? 0.5f : 0.6667f), Quaternion.identity);
                        count++;
                    }
                }
            }
            else
                GridManager.Inst.SelectNodes(data.cardType == CardType.Attack ? AreaType.Attack : AreaType.Buff, false, selectedTiles, unit);
            isDisplay = true;
        }
        else if (!isActive)
        {
            GridManager.Inst.RevertTiles(unit);
            isDisplay = false;
        }
    }
}