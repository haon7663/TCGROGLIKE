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
    [DrawIf("attackType", AttackType.Wide)] public int wideRange;
    [DrawIf("attackType", AttackType.Splash)] public int splashRange;

    Unit unit;
    void Awake()
    {
        unit = GetComponent<Unit>();
    }

    void OnEnable()
    {
        if (rangeType == RangeType.Liner)
        {
            for (int i = 1; i <= range; i++)
            {
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    GridManager.Inst.OnAttackSelect(unit.hexCoords + hexDirection.Coords() * i);
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

    public void OnAttack()
    {

    }

    public List<HexNode> GetArea(HexNode hexNode)
    {
        GridManager.Inst.RevertAbles();
        List<HexNode> hexNodes = new List<HexNode>();
        switch (attackType)
        {
            case AttackType.Single:
                hexNodes.Add(hexNode);
                break;
            case AttackType.Wide:
                var hexCoords = hexNode.Coords - unit.hexCoords;
                var hexDirection = new HexCoords(SignZero(hexCoords._q), SignZero(hexCoords._r)).ToDirection();
                for(int i = 1; i <= range; i++)
                {
                    for (int j = -Mathf.FloorToInt((float)wideRange / 2); j <= Mathf.FloorToInt((float)wideRange / 2); j++)
                    {
                        hexNodes.Add(GridManager.Inst.GetTileAtPosition((unit.hexCoords + hexDirection.Rotate(j).Coords() * i).Pos));
                    }
                }
                break;
        }
        return hexNodes;
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
