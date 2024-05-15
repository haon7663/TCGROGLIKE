using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine.Serialization;

public class HexNode : MonoBehaviour
{
    public HexCoords Coords { get; private set; }
    public Unit Unit { get; private set; }
    
    public bool OnUnit { get; private set; }
    public bool OnObstacle { get; private set; }

    [Header("트랜스폼")]
    [SerializeField] private Transform rangeDisplayBundle;
    
    [Header("텍스트")]
    [SerializeField] private TMP_Text damageText;
    
    [Header("장판효과")]
    public List<StatusInfo> statuses;
    
    [Header("디버그")]
    [SerializeField] private TMP_Text coordsText;

    public float GetDistance(HexNode other) => Coords.GetDistance(other.Coords); // Helper to reduce noise in pathfinding

    public virtual void Init(bool onObstacle, HexCoords coords)
    {
        OnObstacle = onObstacle;
        
        this.Coords = coords;
        coordsText.text = "q: " + coords._q + ", r: " + coords._r + "  s: " + coords._s;
        transform.position = this.Coords.Pos;
    }
    
    private void OnMouseUp()
    {
        if (!CanWalk()) return;

        var canMove = CanMove();
        var canCard = CanCard();
        var canArrange = CanArrange();

        if (canMove.Item1)
        {
            canMove.Item2.move.OnMove(Coords);
        }
        else if (canCard.Item1)
        {
            canCard.Item2.card.UseCard(this);
        }
    } 
    private void OnMouseDown()
    {
        if (!CanWalk()) return;

        var canMove = CanMove();
        var canCard = CanCard();
        var canArrange = CanArrange();

        if (canMove.Item1)
        {
            canMove.Item2.move.OnMove(Coords);
        }
        else if (canCard.Item1)
        {
            canCard.Item2.card.UseCard(this);
        }
    }
    private void OnMouseOver()
    {
        if (OnSelect()) return;

        var canMove = CanMove();
        var canCard = CanCard();
        var canArrange = CanArrange();

        GridManager.inst.SelectNode(this, canMove.Item1 || canCard.Item1 || canArrange.Item1);

        if ((canMove.Item1 || canCard.Item1 || canArrange.Item1) && UnitManager.sUnit)
            UnitManager.sUnit.Repeat(this);

        if (canMove.Item1)
        {
            GridManager.inst.AreaDisplay(AreaType.Select, true, new List<HexNode>() { this }, null);
        }
        else if (canCard.Item1)
        {
            GridManager.inst.AreaDisplay(AreaType.Select, true, canCard.Item2.card.GetSelectedArea(this), null);
        }
        else if (canArrange.Item1)
        {
            GridManager.inst.AreaDisplay(AreaType.Select, true, new List<HexNode>() { this }, null);
        }
    }
    private void OnMouseExit()
    {
        if (!CanMove().Item1 && !CanCard().Item1 && !CanArrange().Item1) return;

        GridManager.inst.RevertAbles();
    }

    public void PutUnit(Unit unit)
    {
        OnUnit = true;
        Unit = unit;
    }
    public void RemoveUnit(Unit unit)
    {
        OnUnit = false;
        Unit = null;
    }
    
    private (bool, Unit) CanMove()
    {
        foreach (var displayNode in transform.GetChild(0).GetComponentsInChildren<RangeDisplayer>())
        {
            if (displayNode.gameObject.activeSelf && displayNode.areaType == AreaType.Move && displayNode.canSelect)
                return (true, displayNode.unit);
        }
        return (false, null);
    }
    private (bool, Unit) CanCard()
    {
        foreach (var displayNode in transform.GetChild(0).GetComponentsInChildren<RangeDisplayer>())
        {
            if (displayNode.gameObject.activeSelf && (displayNode.areaType == AreaType.Attack || displayNode.areaType == AreaType.Buff) && displayNode.canSelect)
                return (true, displayNode.unit);
        }
        return (false, null);
    }
    private (bool, Unit) CanArrange()
    {
        foreach (var displayNode in transform.GetChild(0).GetComponentsInChildren<RangeDisplayer>())
        {
            if (displayNode.gameObject.activeSelf && displayNode.areaType == AreaType.Arrange && displayNode.canSelect)
                return (true, displayNode.unit);
        }
        return (false, null);
    }

    public bool OnSelect()
    {
        return transform.GetChild(0).GetComponentsInChildren<RangeDisplayer>().ToList().Exists(x => x.gameObject.activeSelf && x.areaType == AreaType.Select);
    }

    public void RevertTile(Unit unit = null)
    {
        foreach(var displayNode in transform.GetChild(0).GetComponentsInChildren<RangeDisplayer>())
        {
            displayNode.Release(unit);
        }
    }

    public void RevertAble(Unit unit = null)
    {
        foreach (var displayNode in transform.GetChild(0).GetComponentsInChildren<RangeDisplayer>())
        {
            if(displayNode.areaType == AreaType.Select)
                displayNode.Release(unit);
        }
        damageText.gameObject.SetActive(false);
    }

    public bool CanWalk() => !OnObstacle && !OnUnit;
    public bool CanPass() => !OnObstacle && !OnUnit;


    #region Pathfinding
    public List<HexNode> Neighbors { get; protected set; }
    public HexNode Connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    public void CacheNeighbors()
    {
        Neighbors = GridManager.inst.Tiles.Where(t => Coords.GetDistance(t.Value.Coords) == 1).Select(t => t.Value).ToList();
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

    public int GetPathDistance(HexCoords other) => Pathfinding.FindPathDistance(GridManager.inst.GetTile(this), GridManager.inst.GetTile(other));

    private static readonly float Sqrt3 = Mathf.Sqrt(3);

    private static readonly float HexSize = 15f / 16;

    public Vector3 Pos { get; set; }

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

    public void DebugQRS()
    {
        Debug.Log("Q: " + _q + " / R: " + _r + " / S: " + _s);
    }
}