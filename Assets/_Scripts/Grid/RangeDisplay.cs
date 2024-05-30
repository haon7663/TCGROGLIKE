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

    public void Setup(AreaType areaType, Unit unit, HexNode node, bool canSelect, Color color)
    {
        spriteRenderer.color = canSelect ? new Color(color.r, color.g, color.b, 0.2f) : new Color(color.r, color.g, color.b, 0.5f);

        foreach (var direction in HexDirectionExtension.Loop())
        {
            var isContain = !GridManager.inst.ContainNode((node.Coords + direction.Coords()).Pos);
            directionSpriteRenderers[(int)direction].gameObject.SetActive(isContain);
            if (isContain)
                directionSpriteRenderers[(int)direction].color = color;
        }
        animator.enabled = !canSelect;
        
        AreaType = areaType;
        CanSelect = canSelect;
        Unit = unit;
        Active = true;
    }
    public void Release(Unit unit = null)
    {
        if (Unit != unit && unit)
            return;
        
        gameObject.SetActive(false);
        Active = false;
    }
}
