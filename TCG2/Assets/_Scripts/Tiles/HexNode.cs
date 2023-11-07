using System.Linq;
using UnityEngine;
using System;

public class HexNode : NodeBase
{
    public override void CacheNeighbors()
    {
        Neighbors = GridManager.Inst.Tiles.Where(t => Coords.GetDistance(t.Value.Coords) == 1).Select(t => t.Value).ToList();
    }
}
public struct HexCoords : ICoords 
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

    public float GetDistance(ICoords other) => (this - (HexCoords)other).AxialLength();

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