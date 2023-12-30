using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class HexNode : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Color obstacleColor;

    [SerializeField] Gradient walkableColor;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TMP_Text coordsText;
    [SerializeField] GameObject selectAbleObject;
    [SerializeField] GameObject attackAbleObject;
    [SerializeField] GameObject displayMoveObject;
    [SerializeField] GameObject displayAttackObject;

    public HexCoords Coords;
    public float GetDistance(HexNode other) => Coords.GetDistance(other.Coords); // Helper to reduce noise in pathfinding
    public bool Walkable { get; private set; }
    public bool moveable, attackable;
    Color defaultColor;

    public virtual void Init(bool walkable, HexCoords coords)
    {
        Walkable = walkable;

        spriteRenderer.color = walkable ? walkableColor.Evaluate(Random.Range(0f, 1f)) : obstacleColor;
        defaultColor = spriteRenderer.color;

        Coords = coords;
        coordsText.text = "q: " + coords._q + ", r: " + coords._r + "  s: " + coords._s;
        transform.position = Coords.Pos;
    }

    void OnClickTile(HexNode selected)
    {
        if(moveable)
        {
            UnitManager.sUnit_Move.OnMove(selected.Coords);
        }
        else if(attackable)
        {
            UnitManager.sUnit_Attack.OnAttack();
        }
    }
    void OnHoverTile(HexNode selected)
    {
        if (moveable)
        {
            UnitManager.sUnit_Move.GetArea(selected).displayMoveObject.SetActive(true);
        }
        else if (attackable)
        {
            foreach (HexNode hexNode in UnitManager.sUnit_Attack.GetArea(selected))
            {
                hexNode.displayAttackObject.SetActive(true);
            }
        }
    }
    protected virtual void OnMouseDown()
    {
        if (!Walkable) return;
        OnClickTile(HexDirectionExtension.GetThisHexNode(this));
    }
    protected virtual void OnMouseOver()
    {
        if (!Walkable) return;
        OnHoverTile(HexDirectionExtension.GetThisHexNode(this));
    }

    public void SetColor(Color color)
    {
        GridManager.Inst.selectedNode.Add(this);
        selectAbleObject.SetActive(true);
    }

    public void RevertTile()
    {
        selectAbleObject.SetActive(false);
        GridManager.Inst.selectedNode.Remove(this);
        moveable = false;
        attackable = false;
    }

    public void RevertAble()
    {
        displayMoveObject.SetActive(false);
        displayAttackObject.SetActive(false);
    }

    #region Pathfinding
    public List<HexNode> Neighbors { get; protected set; }
    public HexNode Connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    public void SetConnection(HexNode nodeBase)
    {
        Connection = nodeBase;
    }

    public void SetG(float g)
    {
        G = g;
    }

    public void SetH(float h)
    {
        H = h;
    }

    #endregion
}

public struct HexCoords
{
    public int _q { get; set; }
    public int _r { get; set; }
    public int _s { get; set; }

    public HexCoords(int q, int r)
    {
        _q = q;
        _r = r;
        _s = -q - r;
        Pos = _q * new Vector2(HexSize * 2, 0) + _r * new Vector2(HexSize, HexSize);
    }
    public HexCoords(float q, float r)
    {
        int sq = (int)q;
        int sr = (int)r;
        _q = sq;
        _r = sr;
        _s = -sq - sr;
        Pos = sq * new Vector2(HexSize * 2, 0) + sr * new Vector2(HexSize, HexSize);
    }

    public static HexCoords operator +(HexCoords a, HexCoords b)
        => new HexCoords(a._q + b._q, a._r + b._r);

    public static HexCoords operator +(HexCoords a, HexDirection b)
        => a + b.Coords();

    public static HexCoords operator +(HexDirection a, HexCoords b)
        => a.Coords() + b;

    public static HexCoords operator -(HexCoords a, HexCoords b)
        => new HexCoords(a._q - b._q, a._r - b._r);

    public static HexCoords operator -(HexCoords a, HexDirection b)
        => a - b.Coords();

    public static HexCoords operator -(HexDirection a, HexCoords b)
        => a.Coords() - b;

    public static HexCoords operator *(HexCoords a, int b)
        => new HexCoords(a._q * b, a._r * b);

    public static HexCoords operator *(int a, HexCoords b)
        => b * a;

    public static bool operator ==(HexCoords a, HexCoords b)
        => a._q == b._q && a._r == b._r;

    public static bool operator !=(HexCoords a, HexCoords b)
        => a._q != b._q || a._r != b._r;

    public float GetDistance(HexCoords other) => (this - other).AxialLength();

    private static readonly float Sqrt3 = Mathf.Sqrt(3);

    private static readonly float HexSize = 1.0625f;

    public Vector2 Pos { get; set; }

    private int AxialLength()
    {
        if (_q == 0 && _r == 0) return 0;
        if (_q > 0 && _r >= 0) return _q + _r;
        if (_q <= 0 && _r > 0) return -_q < _r ? _r : -_q;
        if (_q < 0) return -_q - _r;
        return -_r > _q ? -_r : _q;
    }
}