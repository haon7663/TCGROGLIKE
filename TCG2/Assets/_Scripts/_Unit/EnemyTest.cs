using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    public Unit startUnit, targetUnit;

    public List<HexNode> a = new List<HexNode>();
    void OnEnable()
    {
        a = Pathfinding.FindPath(GridManager.Inst.GetTileAtPosition(startUnit.coords.Pos), GridManager.Inst.GetTileAtPosition(targetUnit.coords.Pos));
        foreach (HexNode hex in a)
        {
            hex.SetSelectOutline(SelectOutline.DamageAble);
        }
    }
}
