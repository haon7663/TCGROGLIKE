using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scriptable Hex Grid")]
public class ScriptableHexGrid : ScriptableGrid
{

    [SerializeField, Range(1, 50)] private int _gridWidth = 16;
    [SerializeField, Range(1, 50)] private int _gridDepth = 9;

    public override Dictionary<Vector2, HexNode> GenerateGrid()
    {
        var tiles = new Dictionary<Vector2, HexNode>();
        var grid = new GameObject
        {
            name = "Grid"
        };

        int halfGridDepth = (_gridDepth - 1) / 2;
        int halfGridWidth = (_gridWidth - 1) / 2;
        for (var r = 0; r <= halfGridDepth; r++)
        {
            for (var q = -halfGridWidth; q <= halfGridWidth - r; q++)
            {
                var tile = Instantiate(nodeBasePrefab, grid.transform);
                tile.Init(DecideIfObstacle(), new HexCoords(q, r));
                tiles.Add(tile.Coords.Pos, tile);
            }
            if (r == 0) continue;
            for (var q = r - halfGridWidth; q <= halfGridWidth; q++)
            {
                var tile = Instantiate(nodeBasePrefab, grid.transform);
                tile.Init(DecideIfObstacle(), new HexCoords(q, -r));
                tiles.Add(tile.Coords.Pos, tile);
            }
        }

        return tiles;
    }
}