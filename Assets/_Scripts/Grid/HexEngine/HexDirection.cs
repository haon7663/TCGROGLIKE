using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public enum HexDirection
{
    EN,
    E,
    ES,
    WS,
    W,
    WN,
    Default,
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
            new HexCoords(0, 0),
        };

    private static readonly float[] _angles = new[] {
            330f, 270f, 210f, 150f, 90f, 30f, 0
        };

    private static readonly Vector3[] _directions = new[] {
            Quaternion.Euler(0f, 0f, _angles[0]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[1]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[2]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[3]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[4]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[5]) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[6]) * Vector3.up,
        };

    private static readonly Vector3[] _vertexDirections = new[] {
            Quaternion.Euler(0f, 0f, _angles[0] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[1] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[2] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[3] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[4] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[5] - 30) * Vector3.up,
            Quaternion.Euler(0f, 0f, _angles[6] - 30) * Vector3.up,
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
    public static IEnumerable<HexDirection> Loop()
    {
        for (var i = 0; i < 6; i++)
        {
            yield return (HexDirection)i;
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

    public static List<HexNode> Area(HexCoords coords, int range, bool onSelf = false)
    {
        List<HexNode> hexNodes = new List<HexNode>();
        foreach(KeyValuePair<Vector2, HexNode> tile in GridManager.inst.Tiles)
        {
            if (!onSelf && tile.Value.Coords == coords) continue;
            if (Cube_distance(coords, tile.Value.Coords) <= range)
                hexNodes.Add(tile.Value);
        }
        return hexNodes;
    }

    public static List<HexNode> ReachArea(HexCoords coords, int range, bool onSelf = false)
    {
        var startNode = GridManager.inst.GetNode(coords);
        var visited = new List<HexNode>() { startNode };
        var fringes = new List<HexNode>() { startNode };

        for (var i = 0; i < range; i++)
        {
            var count = fringes.Count;
            for (var j = 0; j < count; j++)
            {
                var neighbors = fringes[j].Neighbors;
                foreach (var neighbor in neighbors.Where(neighbor => !visited.Contains(neighbor) && neighbor.CanWalk()))
                {
                    visited.Add(neighbor);
                    fringes.Add(neighbor);
                }
            }
        }
        if (!onSelf)
            visited.Remove(startNode);
        return visited;
    }
    public static List<HexNode> TransitArea(HexCoords coords, int range)
    {
        return (from tile in GridManager.inst.Tiles
            where tile.Value.Coords != coords
            where Cube_distance(coords, tile.Value.Coords) == range select tile.Value).ToList();
    }

    public static List<HexNode> Liner(HexCoords coords, HexDirection hexDirection, int range, int width = 1, bool isPenetrate = false)
    {
        var linerNode = new List<HexNode>();
        var isBlocked = false;
        for (var i = 0; i < range; i++)
        {
            var floorWide = Mathf.FloorToInt((float)width / 2);
            for (var j = -floorWide; j <= floorWide; j++)
            {
                var selectedCoords = coords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i;
                if (GridManager.inst.Tiles.ContainsKey(selectedCoords.Pos) && (isPenetrate || GridManager.inst.GetNode(selectedCoords).CanWalk()) && GridManager.inst.GetNode(selectedCoords)?.OnObstacle == false)
                    linerNode.Add(GridManager.inst.GetNode(selectedCoords.Pos));
                else if (GridManager.inst.Tiles.ContainsKey(selectedCoords.Pos) && GridManager.inst.GetNode(selectedCoords).OnUnit)
                {
                    linerNode.Add(GridManager.inst.GetNode(selectedCoords.Pos));
                    isBlocked = true;
                    break;
                }
                else
                {
                    isBlocked = true;
                    break;
                }
            }
            if (isBlocked)
                break;
        }
        return linerNode;
    }

    public static List<HexNode> Diagonal(HexCoords unitCoords, HexDirection hexDirection, int range, bool onSelf = false, bool isSubtract = false)
    {
        var directionCoords = unitCoords + hexDirection.Coords() * range;

        var resultNode = Area(unitCoords, range)
            .Where(hexNode => !isSubtract || !Liner(unitCoords, hexDirection, range).Contains(hexNode)).Where(hexNode =>
                DiagonalRange(directionCoords._q, hexNode.Coords._q, unitCoords._q, range) &&
                DiagonalRange(directionCoords._r, hexNode.Coords._r, unitCoords._r, range) &&
                DiagonalRange(directionCoords._s, hexNode.Coords._s, unitCoords._s, range)).ToList();

        if(onSelf)
            resultNode.Add(GridManager.inst.Tiles[unitCoords.Pos]);

        return resultNode;
    }

    private static bool DiagonalRange(int value, int nodeValue, int unitValue, int range)
    {
        if (value == unitValue)
            return true;
        if (value == range + unitValue && nodeValue >= unitValue && nodeValue <= range + unitValue)
            return true;
        else if (value == -range + unitValue && nodeValue <= unitValue && nodeValue >= -range + unitValue)
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
    public static bool ContainsDirection(this HexCoords coords)
    {
        return _coords.Contains(coords);
    }
    public static HexDirection ToDirection(this HexCoords coords)
    {
        return (HexDirection)Array.IndexOf(_coords, coords);
    }
    public static HexDirection GetSignDirection(this HexCoords coords)
    {
        if (coords._q == 0 || coords._r == 0 || coords._s == 0)
            return new HexCoords(SignZero(coords._q), SignZero(coords._r)).ToDirection();
        return (HexDirection)(-1);
    }
    public static HexDirection GetNearlyDirection(this HexCoords startCoords, HexCoords targetCoords)
    {
        var distance = 9999999f;
        var minCoords = new HexCoords(0, 1);
        foreach (var coords in _coords)
        {
            var magnitude = (targetCoords.Pos - (startCoords + coords).Pos).sqrMagnitude;
            if (magnitude >= distance) continue;
            distance = magnitude;
            minCoords = coords;
        }
        return minCoords.ToDirection();
    }
    public static HexDirection GetNearlyMouseDirection(this HexCoords startCoords)
    {
        var distance = 9999999f;
        var minCoords = new HexCoords(0, 1);
        foreach(var coords in _coords)
        {
            var magnitude = (Utils.MousePos - (startCoords + coords).Pos).sqrMagnitude;
            if (magnitude >= distance) continue;
            distance = magnitude;
            minCoords = coords;
        }
        return minCoords.ToDirection();
    }
    private static int SignZero(int value)
    {
        return value switch
        {
            > 0 => 1,
            < 0 => -1,
            _ => 0
        };
    }
}