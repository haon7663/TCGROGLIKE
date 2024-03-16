using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OutlineType { Move, Attack, Buff, Arrow, Default}

[Serializable]
public struct OutlineInfo
{
    public OutlineType outlineType;
    [DrawIf("outlineType", OutlineType.Arrow)] public HexDirection direction;
    public bool canSelect;
    public Color color;
}

public class NodeOutline : MonoBehaviour
{
    OutlineInfo info;
    HexNode node;

    public void SetUp(OutlineInfo info, List<HexNode> nodes)
    {
        this.info = info;
        node = transform.GetComponentInParent<HexNode>();

        gameObject.SetActive(true);
        foreach (HexDirection direction in HexDirectionExtension.Loop(HexDirection.EN))
        {
            transform.GetChild((int)direction).gameObject.SetActive(!nodes.Contains(GridManager.Inst.GetTile(node.coords + direction.Coords())));
        }
    }
}
