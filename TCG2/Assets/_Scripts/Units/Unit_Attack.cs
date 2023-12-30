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
                        var floorWide = Mathf.FloorToInt((float)item.lineWidth / 2);
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

        var hexDirection = (hexNode.Coords - unit.hexCoords).GetSignDirection();
        //Debug.Log(hexDirection.Coords()._q.ToString() + "/" + hexDirection.Coords()._r.ToString());
        switch (item.attackType)
        {
            case AttackType.Single:
                hexNodes.Add(hexNode);
                break;
            case AttackType.Wide:
                hexNodes.AddRange(HexDirectionExtension.GetDiagonal(unit.hexCoords, hexDirection, item.range));
                break;
            case AttackType.Liner:
                hexNodes.AddRange(HexDirectionExtension.GetLiner(unit.hexCoords, hexDirection, item.range, item.lineWidth));
                break;
        }
        return hexNodes;
    }

    public void OnAttack()
    {
        GridManager.Inst.RevertTiles();
    }
}
