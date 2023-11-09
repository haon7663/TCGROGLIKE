using System;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection
{
    EN,
    E,
    ES,
    WS,
    W,
    WN
}

public static class HexDirectionExtension
{

    #region Readonly Arrays
    private static readonly HexCoords[] _coords = new[] {
            new HexCoords(0, 1),
            new HexCoords(1, 0),
            new HexCoords(1, -1),
            new HexCoords(0, -1),
            new HexCoords(-1, 0),
            new HexCoords(-1, 1),
        };

    private static readonly float[] _angles = new[] {
            330f, 270f, 210f, 150f, 90f, 30f
        };

    private static readonly Vector3[] _directions = new[] {
            Quaternion.Euler(0f, 0f, _angles[0]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[1]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[2]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[3]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[4]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[5]) * Vector3.up,
        };

    private static readonly Vector3[] _vertexDirections = new[] {
            Quaternion.Euler(0f, 0f, _angles[0] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[1] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[2] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[3] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[4] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[5] - 30) * Vector3.up,
        };
    #endregion

    /// <summary>
    /// HexCoords of the direction.
    /// </summary>
    public static HexCoords Coords(this HexDirection direction) => _coords[(int)direction];

    /// <summary>
    /// Angle in degrees from 0 for HexDirection.N, increasing clockwise.
    /// </summary>
    public static float Angle(this HexDirection direction) => _angles[(int)direction];

    /// <summary>
    /// Normalized vector of direction in 3D space for HexDirection, where HexDirection.N is Vector3.forward.
    /// </summary>
    public static Vector3 Direction(this HexDirection direction) => _directions[(int)direction] * (Mathf.Sqrt(3) / 2);

    /// <summary>
    /// Iterates through all hex directions starting from this direction and going clockwise.
    /// </summary>
    public static IEnumerable<HexDirection> Loop(this HexDirection direction)
    {
        var startIndex = (int)direction;
        for (int i = 0; i < 6; i++)
        {
            yield return (HexDirection)((startIndex + i) % 6);
        }
    }

    public static HexCoords Cube_subtract(HexCoords hexPlayer, HexCoords hexTarget)
    {
        return new HexCoords(hexPlayer._q - hexTarget._q, hexPlayer._r - hexTarget._r);
    }

    public static int Cube_distance(HexCoords hexPlayer, HexCoords hexTarget)
    {
        var vec = Cube_subtract(hexPlayer, hexTarget);
        return Mathf.Max(Mathf.Abs(vec._q), Mathf.Abs(vec._r), Mathf.Abs(vec._s));
    }

    public static List<HexNode> Area(HexCoords unitCoords, int range, bool onSelf = false)
    {
        List<HexNode> hexNodes = new List<HexNode>();
        foreach(KeyValuePair<Vector2, HexNode> tile in GridManager.Inst.Tiles)
        {
            if (!onSelf && tile.Value.Coords == unitCoords) continue;
            if (Cube_distance(unitCoords, tile.Value.Coords) <= range)
                hexNodes.Add(tile.Value);
        }

        return hexNodes;
    }

    public static List<HexNode> GetDiagonal(HexCoords hexCoords, List<HexNode> hexNodes, Unit unit, int range)
    {
        List<HexNode> resultNode = new List<HexNode>();
        foreach (HexNode hexNode in Area(unit.hexCoords, range))
            if (!hexNodes.Contains(hexNode))
                if (DiagonalRange(hexCoords._q, hexNode.Coords._q, range) && DiagonalRange(hexCoords._r, hexNode.Coords._r, range) && DiagonalRange(hexCoords._s, hexNode.Coords._s, range))
                    resultNode.Add(hexNode);

        return resultNode;
    }

    public static bool DiagonalRange(int value, int nodeValue, int range)
    {
        if (value == 0)
            return true;
        if (value == range && nodeValue >= 0 && nodeValue <= range)
            return true;
        else if (value == -range && nodeValue <= 0 && nodeValue >= -range)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Returns HexDirection rotated clockwise.
    /// </summary>
    public static HexDirection Rotate(this HexDirection direction, int rotation)
    {
        var value = (int)direction + rotation;
        value = (value % 6) + 6;
        return (HexDirection)(value % 6);
    }

    /// <summary>
    /// Returns HexDirection rotated clockwise once.
    /// </summary>
    public static HexDirection Right(this HexDirection direction)
    {
        return (HexDirection)(((int)direction + 1) % 6);
    }

    /// <summary>
    /// Returns HexDirection rotated counter-clockwise once.
    /// </summary>
    public static HexDirection Left(this HexDirection direction)
    {
        return (HexDirection)(((int)direction + 5) % 6);
    }

    /// <summary>
    /// Returns the opposite HexDirection.
    /// </summary>
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (HexDirection)(((int)direction + 3) % 6);
    }

    /// <summary>
    /// If given HexCoords are coords of any HexDirection, it returns given HexDirection.
    /// Otherwise it throws an error.
    /// </summary>
    public static HexDirection ToDirection(this HexCoords coords)
    {
        return (HexDirection)Array.IndexOf(_coords, coords);
    }
}