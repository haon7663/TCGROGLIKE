using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Attack : MonoBehaviour
{
    [Header("범위 표시")]
    public RangeType rangeType;
    public int range;

    [Header("공격 타입")]
    public AttackType attackType;
    [DrawIf("attackType", AttackType.Splash)] public int splashRange = 1;
    [DrawIf("attackType", AttackType.Liner)] public int lineWide = 1;

    Unit unit;
    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    void OnEnable()
    {
        if (rangeType == RangeType.Liner)
        {
            for (int i = 0; i < range; i++)
            {
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var floorWide = Mathf.FloorToInt((float)lineWide / 2);
                    for (int j = -floorWide; j <= floorWide; j++)
                    {
                        var pos = (unit.hexCoords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i).Pos;
                        GridManager.Inst.OnAttackSelect(pos);
                    }
                }
            }
        }
        else if (rangeType == RangeType.Area)
        {
            foreach (HexNode hexNode in HexDirectionExtension.Area(unit.hexCoords, range))
            {
                GridManager.Inst.OnAttackSelect(hexNode.Coords);
            }
        }
    }
    void OnDisable()
    {
        GridManager.Inst.RevertTiles();
    }

    public List<HexNode> GetArea(HexNode hexNode)
    {
        GridManager.Inst.RevertAbles();
        List<HexNode> hexNodes = new List<HexNode>();

        var hexCoords = hexNode.Coords - unit.hexCoords;
        var hexDirection = new HexCoords(SignZero(hexCoords._q), SignZero(hexCoords._r)).ToDirection();
        switch (attackType)
        {
            case AttackType.Single:
                hexNodes.Add(hexNode);
                break;
            case AttackType.Wide:
                hexNodes.AddRange(HexDirectionExtension.GetDiagonal(unit.hexCoords + hexDirection.Coords() * range, hexNodes, unit, range));
                break;
            case AttackType.Liner:
                for (int i = 0; i < range; i++)
                {
                    var floorWide = Mathf.FloorToInt((float)lineWide / 2);
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
