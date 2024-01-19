using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Card : MonoBehaviour
{
    Unit unit;
    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public CardSO cardData;
    public void DrawArea(CardSO cardData, bool canSelect = true)
    {
        if (canSelect)
            GridManager.Inst.RevertTiles();

        this.cardData = cardData;

        List<HexCoords> selectCoords = GetArea(cardData);

        SelectOutline outline = SelectOutline.Default;
        switch(cardData.cardType)
        {
            case CardType.Attack:
                outline = canSelect ? SelectOutline.AttackSelect : SelectOutline.DamageAble;
                break;
            case CardType.Buff:
                outline = SelectOutline.BuffAble;
                break;
        }
        GridManager.Inst.SelectNodes(selectCoords, outline);
    }
    public List<HexCoords> GetArea(CardSO cardData)
    {
        List<HexCoords> selectCoords = new();
        switch (cardData.rangeType)
        {
            case RangeType.Liner:
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var floorWide = Mathf.FloorToInt((float)cardData.lineWidth / 2);
                    for (int j = -floorWide; j <= floorWide; j++)
                    {
                        for (int i = 0; i < cardData.range; i++)
                        {
                            var coords = unit.coords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i;
                            if (GridManager.Inst.GetTile(coords)?.CanWalk() == true || cardData.isPenetrate)
                                selectCoords.Add(coords);
                            else if(GridManager.Inst.GetTile(coords)?.onUnit == true)
                            {
                                selectCoords.Add(coords);
                                break;
                            }
                            else
                                break;
                        }
                    }
                }
                break;
            case RangeType.Area:
                if (cardData.canSelectAll)
                    foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, cardData.range, cardData.onSelf))
                    {
                        selectCoords.Add(hexNode.coords);
                    }
                else
                {
                    foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                    {
                        selectCoords.Add(unit.coords + hexDirection.Coords());
                    }
                    foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, cardData.range, cardData.onSelf))
                    {
                        selectCoords.Add(hexNode.coords);
                    }
                }
                break;
            case RangeType.OurArea:
                foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, cardData.range, cardData.onSelf))
                {
                    selectCoords.Add(hexNode.coords);
                }
                List<HexCoords> unitCoords = new();
                foreach (var unit in GridManager.Inst.OnTileUnits.Values.Where(t => t.unitData.type != UnitType.Enemy && selectCoords.Contains(t.coords)))
                {
                    unitCoords.Add(GridManager.Inst.GetTile(unit).coords);
                }
                GridManager.Inst.SelectNodes(unitCoords, SelectOutline.BuffSelect);
                break;
            case RangeType.Self:
                selectCoords.Add(unit.coords);
                break;
        }
        return selectCoords;
    }
    public List<HexNode> SelectArea(HexNode node)
    {
        unit.Anim_SetBool("isReady", true);

        GridManager.Inst.RevertAbles();
        List<HexNode> hexNodes = new List<HexNode>();

        var direction = (node.coords - unit.coords).GetSignDirection();
        //Debug.Log(hexDirection.Coords()._q.ToString() + "/" + hexDirection.Coords()._r.ToString());
        switch (cardData.selectType)
        {
            case SelectType.Single:
                hexNodes.Add(node);
                break;
            case SelectType.Wide:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(unit.coords, direction, cardData.range));
                break;
            case SelectType.Liner:
                for(int i = -cardData.multiShot/2; i <= cardData.multiShot/2; i++)
                    hexNodes.AddRange(HexDirectionExtension.Liner(unit.coords, direction.Rotate(i), cardData.range, cardData.lineWidth, cardData.isPenetrate));
                break;
            case SelectType.Splash:
                hexNodes.AddRange(HexDirectionExtension.Area(node.coords, cardData.splashRange, true));
                break;
            case SelectType.Emission:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(unit.coords + direction, direction, cardData.range - 1, true));
                break;
        }
        return hexNodes;
    }

    public bool UseCard(HexNode node)
    {
        if (!node) return false;
        unit.Anim_SetTrigger("attack");
        unit.Anim_SetBool("isReady", false);

        TurnManager.UseEnergy(cardData.energy);

        var direction = (node.coords - unit.coords).GetSignDirection();
        Attack prefab;
        switch (cardData.selectType)
        {
            case SelectType.Single:
                prefab = Instantiate(cardData.prefab).GetComponent<Attack>();
                prefab.Init(unit, node, cardData);
                break;
            case SelectType.Wide:
                break;
            case SelectType.Liner:
                for (int i = -cardData.multiShot / 2; i <= cardData.multiShot / 2; i++)
                {
                    prefab = Instantiate(cardData.prefab).GetComponent<Attack>();
                    prefab.Init(unit, direction.Rotate(i), cardData);
                }
                break;
            case SelectType.Splash:
                break;
            case SelectType.Emission:
                break;
        }

        GridManager.Inst.RevertTiles();
        return true;
    }
}