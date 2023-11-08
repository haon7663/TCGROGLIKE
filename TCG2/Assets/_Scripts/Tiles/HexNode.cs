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
    Color _obstacleColor;

    [SerializeField] Gradient _walkableColor;
    [SerializeField] protected SpriteRenderer _renderer;
    [SerializeField] TMP_Text _coordsText;

    public HexCoords Coords;
    public float GetDistance(HexNode other) => Coords.GetDistance(other.Coords); // Helper to reduce noise in pathfinding
    public bool Walkable { get; private set; }
    bool _selected;
    Color _defaultColor;

    public virtual void Init(bool walkable, HexCoords coords)
    {
        Walkable = walkable;

        _renderer.color = walkable ? _walkableColor.Evaluate(Random.Range(0f, 1f)) : _obstacleColor;
        _defaultColor = _renderer.color;

        OnHoverTile += OnOnHoverTile;

        Coords = coords;
        _coordsText.text = "q: " + coords._q + ", r: " + coords._r;
        transform.position = Coords.Pos;
    }

    public static event Action<HexNode> OnHoverTile;
    void OnEnable() => OnHoverTile += OnOnHoverTile;
    void OnDisable() => OnHoverTile -= OnOnHoverTile;
    void OnOnHoverTile(HexNode selected)
    {
        _selected = selected == this;
    }


    protected virtual void OnMouseDown()
    {
        if (!Walkable) return;
        OnHoverTile?.Invoke(this);
    }

    public void SetColor(Color color) => _renderer.color = color;

    public void RevertTile()
    {
        _renderer.color = _defaultColor;
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
        Pos = _q * new Vector2(Sqrt3, 0) + _r * new Vector2(Sqrt3 / 2, 1.5f);
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

    public float GetDistance(HexCoords other) => (this - (HexCoords)other).AxialLength();

    private static readonly float Sqrt3 = Mathf.Sqrt(3);

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