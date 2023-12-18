using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Attack : MonoBehaviour
{
    Unit unit;
    public Item item;
    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public void OnDrawArea(bool isDraw, Item item = null)
    {
        GridManager.Inst.RevertTiles();
        this.item = item;
        if (isDraw)
        {
            if (item.rangeType == RangeType.Liner)
            {
                for (int i = 0; i < item.range; i++)
                {
                    foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                    {
                        var floorWide = Mathf.FloorToInt((float)item.lineWide / 2);
                        for (int j = -floorWide; j <= floorWide; j++)
                        {
                            var pos = (unit.hexCoords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i).Pos;
                            GridManager.Inst.OnAttackSelect(pos);
                        }
                    }
                }
            }
            else if (item.rangeType == RangeType.Area)
            {
                foreach (HexNode hexNode in HexDirectionExtension.Area(unit.hexCoords, item.range))
                {
                    GridManager.Inst.OnAttackSelect(hexNode.Coords);
                }
            }
        }
    }

    public List<HexNode> GetArea(HexNode hexNode)
    {
        GridManager.Inst.RevertAbles();
        List<HexNode> hexNodes = new List<HexNode>();

        var hexCoords = hexNode.Coords - unit.hexCoords;
        var hexDirection = new HexCoords(SignZero(hexCoords._q), SignZero(hexCoords._r)).ToDirection();
        switch (item.attackType)
        {
            case AttackType.Single:
                hexNodes.Add(hexNode);
                break;
            case AttackType.Wide:
                hexNodes.AddRange(HexDirectionExtension.GetDiagonal(unit.hexCoords + hexDirection.Coords() * item.range, hexNodes, unit, item.range));
                break;
            case AttackType.Liner:
                for (int i = 0; i < item.range; i++)
                {
                    var floorWide = Mathf.FloorToInt((float)item.lineWide / 2);
                    for (int j = -floorWide; j <= floorWide; j++)
                    {
                        var pos = (unit.hexCoords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i).Pos;
                        if (GridManager.Inst.Tiles.ContainsKey(pos))
                            hexNodes.Add(GridManager.Inst.GetTileAtPosition(pos));
                    }
                }
                break;
        }
        return hexNodes;
    }

    public void OnAttack()
    {
        GridManager.Inst.RevertTiles();
    }

    int SignZero(int value)
    {
        if (value > 0)
            return 1;
        else if (value < 0)
            return -1;
        else
            return 0;
    }
}
