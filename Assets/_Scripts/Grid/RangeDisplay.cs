using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeDisplay : MonoBehaviour
{
    public AreaType AreaType { get; private set; }
    public Unit Unit { get; private set; }
    public bool CanSelect { get; private set; }
    public bool Active { get; private set; }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer[] directionSpriteRenderers;

    private HexNode _node;
    
    private readonly int _select = Animator.StringToHash("canSelect");

    public void Setup(AreaType areaType, Unit unit, HexNode node, List<HexNode> nodes, bool canSelect, Color color)
    {
        spriteRenderer.color = canSelect ? new Color(color.r, color.g, color.b, 0.2f) : new Color(color.r, color.g, color.b, 0.5f);

        foreach (var direction in HexDirectionExtension.Loop())
        {
            var isContain = !nodes.Contains(GridManager.inst.GetNode(node.Coords + direction.Coords())) || !GridManager.inst.ContainNode((node.Coords + direction.Coords()).Pos);
            directionSpriteRenderers[(int)direction].gameObject.SetActive(isContain);
            directionSpriteRenderers[(int)direction].color = color;
        }
        animator.SetBool(_select, canSelect);
        
        AreaType = areaType;
        CanSelect = canSelect;
        Unit = unit;
        Active = true;

        _node = node;
    }
    public void Release(Unit unit = null)
    {
        if (Unit != unit && unit)
            return;
        
        gameObject.SetActive(false);
        Active = false;
    }
}
