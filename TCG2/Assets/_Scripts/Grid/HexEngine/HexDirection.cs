using System;
using System.Linq;
using System.Collections.Generic;
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

    public static List<HexNode> Area(HexCoords coords, int range, bool onSelf = false)
    {
        List<HexNode> hexNodes = new List<HexNode>();
        foreach(KeyValuePair<Vector2, HexNode> tile in GridManager.Inst.Tiles)
        {
            if (!onSelf && tile.Value.coords == coords) continue;
            if (Cube_distance(coords, tile.Value.coords) <= range)
                hexNodes.Add(tile.Value);
        }
        return hexNodes;
    }

    public static List<HexNode> ReachArea(HexCoords coords, int range, bool onSelf = false)
    {
        var startNode = GridManager.Inst.GetTile(coords.Pos);
        List<HexNode> visited = new List<HexNode>() { startNode };
        List<HexNode> fringes = new List<HexNode>() { startNode };

        for (int i = 0; i < range; i++)
        {
            var count = fringes.Count;
            for (int j = 0; j < count; j++)
            {
                List<HexNode> neighbors = fringes[j].Neighbors;
                foreach (HexNode neighbor in neighbors)
                {
                    if(!visited.Contains(neighbor) && neighbor.CanWalk())
                    {
                        visited.Add(neighbor);
                        fringes.Add(neighbor);
                    }
                }
            }
        }
        if (!onSelf)
            visited.Remove(startNode);
        return visited;
    }
    public static List<HexNode> TransitArea(HexCoords coords, int range)
    {
        List<HexNode> hexNodes = new List<HexNode>();
        foreach (KeyValuePair<Vector2, HexNode> tile in GridManager.Inst.Tiles)
        {
            if (tile.Value.coords == coords) continue;
            if (Cube_distance(coords, tile.Value.coords) == range)
                hexNodes.Add(tile.Value);
        }
        return hexNodes;
    }

    public static List<HexNode> Liner(HexCoords coords, HexDirection hexDirection, int range, int width = 1, bool isPenetrate = false)
    {
        List<HexNode> linerNode = new List<HexNode>();
        var isBlocked = false;
        for (int i = 0; i < range; i++)
        {
            var floorWide = Mathf.FloorToInt((float)width / 2);
            for (int j = -floorWide; j <= floorWide; j++)
            {
                var SelectedCoords = coords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i;
                if (GridManager.Inst.Tiles.ContainsKey(SelectedCoords.Pos) && (isPenetrate || GridManager.Inst.GetTile(SelectedCoords).CanWalk()) && GridManager.Inst.GetTile(SelectedCoords)?.onObstacle == false)
                    linerNode.Add(GridManager.Inst.GetTile(SelectedCoords.Pos));
                else if (GridManager.Inst.Tiles.ContainsKey(SelectedCoords.Pos) && GridManager.Inst.GetTile(SelectedCoords).onUnit)
                {
                    linerNode.Add(GridManager.Inst.GetTile(SelectedCoords.Pos));
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
        List<HexNode> resultNode = new List<HexNode>();
        var directionCoords = unitCoords + hexDirection.Coords() * range;

        foreach (HexNode hexNode in Area(unitCoords, range))
            if (!isSubtract || !Liner(unitCoords, hexDirection, range).Contains(hexNode))
                if (DiagonalRange(directionCoords._q, hexNode.coords._q, unitCoords._q, range) && DiagonalRange(directionCoords._r, hexNode.coords._r, unitCoords._r, range) && DiagonalRange(directionCoords._s, hexNode.coords._s, unitCoords._s, range))
                    resultNode.Add(hexNode);

        if(onSelf)
            resultNode.Add(GridManager.Inst.Tiles[unitCoords.Pos]);

        return resultNode;
    }

    public static bool DiagonalRange(int value, int nodeValue, int unitValue, int range)
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
        float distance = 9999999f;
        HexCoords minCoords = new HexCoords(0, 1);
        foreach (HexCoords coords in _coords)
        {
            var magnitude = (targetCoords.Pos - (startCoords + coords).Pos).sqrMagnitude;
            if (magnitude < distance)
            {
                distance = magnitude;
                minCoords = coords;
            }
        }
        return minCoords.ToDirection();
    }
    public static HexDirection GetNearlyMouseDirection(this HexCoords startCoords)
    {
        float distance = 9999999f;
        HexCoords minCoords = new HexCoords(0, 1);
        foreach(HexCoords coords in _coords)
        {
            var magnitude = (Utils.MousePos - (startCoords + coords).Pos).sqrMagnitude;
            if (magnitude < distance)
            {
                distance = magnitude;
                minCoords = coords;
            }
        }
        return minCoords.ToDirection();
    }
    static int SignZero(int value)
    {
        if (value > 0)
            return 1;
        else if (value < 0)
            return -1;
        else
            return 0;
    }
}