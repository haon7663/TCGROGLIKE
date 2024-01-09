using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public enum SelectOutline
{
    MoveSelect, MoveAble, AttackSelect, DamageAble,
}

public class HexNode : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Color obstacleColor;

    [SerializeField] Gradient walkableColor;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TMP_Text coordsText;
    [SerializeField] GameObject moveSelectObject;
    [SerializeField] GameObject moveAbleObject;
    [SerializeField] GameObject attackSelectObject;
    [SerializeField] GameObject damageAbleObject;
    [SerializeField] GameObject displayMoveObject;
    [SerializeField] GameObject displayAttackObject;

    public HexCoords Coords;
    public float GetDistance(HexNode other) => Coords.GetDistance(other.Coords); // Helper to reduce noise in pathfinding
    public bool walkAble;
    public bool canMove, canAttack, canDamaged;

    public virtual void Init(bool walkable, HexCoords coords)
    {
        walkAble = walkable;

        spriteRenderer.color = walkable ? walkableColor.Evaluate(Random.Range(0f, 1f)) : obstacleColor;
        spriteRenderer.sortingOrder = -coords._r;

        Coords = coords;
        coordsText.text = "q: " + coords._q + ", r: " + coords._r + "  s: " + coords._s;
        transform.position = Coords.Pos;
    }

    void OnMouseDown()
    {
        if (!walkAble) return;

        if (canMove)
        {
            UnitManager.sUnit_Move.OnMove(Coords);
        }
        else if (canAttack)
        {
            UnitManager.sUnit_Attack.OnAttack();
        }
    }
    void OnMouseOver()
    {
        if (!walkAble || (!canMove && !canAttack)) return;
        UnitManager.sUnit.Repeat(this);

        if (canMove)
        {
            UnitManager.sUnit_Move.TouchArea(this).displayMoveObject.SetActive(true);
        }
        else if (canAttack)
        {
            foreach (HexNode hexNode in UnitManager.sUnit_Attack.GetArea(this))
            {
                hexNode.displayAttackObject.SetActive(true);
            }
        }
    }
    void OnMouseExit()
    {
        if (!canMove && !canAttack) return;

        GridManager.Inst.RevertAbles();
    }

    public void SetSelectOutline(SelectOutline selectLine)
    {
        GridManager.Inst.selectedNode.Add(this);
    }
    public void OnDisplay(SelectOutline selectLine, List<HexNode> nodes)
    {
        switch (selectLine)
        {
            case SelectOutline.MoveSelect:
                canMove = true;
                moveSelectObject.SetActive(true);
                foreach (HexDirection direction in HexDirectionExtension.Loop(HexDirection.EN))
                {
                    moveSelectObject.transform.GetChild((int)direction).gameObject.SetActive(!nodes.Contains(GridManager.Inst.GetTile(Coords + direction.Coords())));
                }
                break;
            case SelectOutline.MoveAble:
                moveAbleObject.SetActive(true);
                foreach (HexDirection direction in HexDirectionExtension.Loop(HexDirection.EN))
                {
                    moveAbleObject.transform.GetChild((int)direction).gameObject.SetActive(!nodes.Contains(GridManager.Inst.GetTile(Coords + direction.Coords())));
                }
                break;
            case SelectOutline.AttackSelect:
                canAttack = true;
                attackSelectObject.SetActive(true);
                foreach (HexDirection direction in HexDirectionExtension.Loop(HexDirection.EN))
                {
                    attackSelectObject.transform.GetChild((int)direction).gameObject.SetActive(!nodes.Contains(GridManager.Inst.GetTile(Coords + direction.Coords())));
                }
                break;
            case SelectOutline.DamageAble:
                canDamaged = true;
                damageAbleObject.SetActive(true);
                foreach (HexDirection direction in HexDirectionExtension.Loop(HexDirection.EN))
                {
                    damageAbleObject.transform.GetChild((int)direction).gameObject.SetActive(!nodes.Contains(GridManager.Inst.GetTile(Coords + direction.Coords())));
                }
                break;
        }
    }

    public void RevertTile()
    {
        moveSelectObject.SetActive(false);
        moveAbleObject.SetActive(false);
        attackSelectObject.SetActive(false);
        damageAbleObject.SetActive(false);

        GridManager.Inst.selectedNode.Remove(this);
        canMove = false;
        canAttack = false;
        canDamaged = false;
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

    public void CacheNeighbors()
    {
        Neighbors = GridManager.Inst.Tiles.Where(t => Coords.GetDistance(t.Value.Coords) == 1).Select(t => t.Value).ToList();
    }

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
        Pos = _q * new Vector2(HexSize * 2, 0) + _r * new Vector2(HexSize, Sqrt3 / 2);
    }
    public HexCoords(float q, float r)
    {
        int sq = (int)q;
        int sr = (int)r;
        _q = sq;
        _r = sr;
        _s = -sq - sr;
        Pos = sq * new Vector2(HexSize * 2, 0) + sr * new Vector2(HexSize, Sqrt3 / 2);
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

    private static readonly float HexSize = 1;

    public Vector2 Pos { get; set; }

    private int AxialLength()
    {
        if (_q == 0 && _r == 0) return 0;
        if (_q > 0 && _r >= 0) return _q + _r;
        if (_q <= 0 && _r > 0) return -_q < _r ? _r : -_q;
        if (_q < 0) return -_q - _r;
        return -_r > _q ? -_r : _q;
    }

    public override bool Equals(object obj)
    {
        return obj is HexCoords coords &&
               _q == coords._q &&
               _r == coords._r &&
               _s == coords._s;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_q, _r, _s);
    }
}