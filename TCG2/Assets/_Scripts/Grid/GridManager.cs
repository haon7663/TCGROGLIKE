using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public static GridManager Inst;
    void Awake()
    {
        Inst = this;

        Tiles = _scriptableGrid.GenerateGrid();
        foreach (var tile in Tiles.Values) tile.CacheNeighbors();
        OnTileUnits = new Dictionary<Vector2, Unit>();
    }

    [SerializeField] ScriptableGrid _scriptableGrid;
    //[SerializeField] bool _drawConnections;

    public Dictionary<Vector2, HexNode> Tiles { get; private set; }
    public Dictionary<Vector2, Unit> OnTileUnits { get; private set; }
    public HexNode selectedNode;

    public void RevertTiles()
    {
        foreach (var t in Tiles.Values) t.RevertTile();
        RevertAbles();
    }
    public void RevertAbles()
    {
        foreach (var t in Tiles.Values) t.RevertAble();
    }

    public List<HexNode> CoordsToNodes(List<HexCoords> coordses)
    {
        List<HexNode> tiles = new();
        foreach (HexCoords c in coordses)
        {
            if (Tiles.ContainsKey(c.Pos))
                tiles.Add(Tiles[c.Pos]);
        }
        return tiles;
    }
    public void SelectNodes(List<HexCoords> coordses, SelectOutline outline)
    {
        List<HexNode> tiles = CoordsToNodes(coordses);
        foreach (HexNode t in tiles)
        {
            t.OnDisplay(outline, tiles);
        }
    }
    public void SelectNodes(List<HexNode> tiles, SelectOutline outline)
    {
        foreach (HexNode t in tiles)
        {
            t.OnDisplay(outline, tiles);
        }
    }
    public void SelectNode(HexNode node, bool isSelect = true)
    {
        if (!isSelect)
            selectedNode = null;
        else if (node != selectedNode)
            selectedNode = node;
    }

    #region SetTileUnit
    public void SetTileUnit(HexCoords hexCoords, Unit unit)
    {
        GetTile(hexCoords).OnUnit(unit);
        OnTileUnits.Add(hexCoords.Pos, unit);
        SetWalkable();
    }
    public bool SetTileUnit(HexCoords prevCoords, HexCoords nextCoords, Unit unit)
    {
        if (prevCoords == nextCoords)
            return false;
        if (GetTile(nextCoords)?.CanWalk() != true)
            return false;

        GetTile(nextCoords).OnUnit(unit);
        OnTileUnits.Add(nextCoords.Pos, unit);
        GetTile(prevCoords).OnUnit(unit, true);
        OnTileUnits.Remove(prevCoords.Pos);
        SetWalkable();
        return true;
    }
    public void SetTileUnitRemove(Unit unit)
    {
        GetTile(unit).OnUnit(unit, true);
        OnTileUnits.Remove(unit.coords.Pos);
        SetWalkable();
    }
    #endregion    
    #region GetTile
    public HexNode GetTile(Unit unit) => Tiles.ContainsKey(unit.coords.Pos) ? Tiles.TryGetValue(unit.coords.Pos, out var tile) ? tile : null : null;
    public HexNode GetTile(HexCoords coords) => Tiles.ContainsKey(coords.Pos) ? Tiles.TryGetValue(coords.Pos, out var tile) ? tile : null : null;
    public HexNode GetTile(Vector2 pos) => Tiles.ContainsKey(pos) ? Tiles.TryGetValue(pos, out var tile) ? tile : null : null;
    #endregion
    #region GetUnit
    public Unit GetUnit(HexNode hexNode) => OnTileUnits.ContainsKey(hexNode.coords.Pos) ? OnTileUnits[hexNode.coords.Pos] : null;
    public Unit GetUnit(HexCoords hexCoords) => OnTileUnits.ContainsKey(hexCoords.Pos) ? OnTileUnits[hexCoords.Pos] : null;
    public Unit GetUnit(Vector2 pos) => OnTileUnits.ContainsKey(pos) ? OnTileUnits[pos] : null;
    #endregion

    public void SetWalkable()
    {
        foreach (KeyValuePair<Vector2, HexNode> tile in Tiles)
            tile.Value.onUnit = OnTileUnits.ContainsKey(tile.Value.coords.Pos);
    }
    public HexNode GetRandomNode() => Tiles.Where(t => t.Value.CanWalk()).OrderBy(t => Random.value).First().Value;
    public void StatusNode()
    {
        foreach (var tile in Tiles.Where(t => t.Value.onUnit && t.Value.statuses.Count != 0))
        {
            print(tile.Value.statuses[0].data.name);
            StatusManager.Inst.AddUnitStatus(tile.Value.statuses, GetUnit(tile.Value));
        }
    }


    /*void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        foreach (var t in Tiles)
        {
            if (t.Value.Connection == null) continue;
            Gizmos.DrawLine((Vector3)t.Key + new Vector3(0, 0, -1), (Vector3)t.Value.Connection.coords.Pos + new Vector3(0, 0, -1));
        }
    }*/
}