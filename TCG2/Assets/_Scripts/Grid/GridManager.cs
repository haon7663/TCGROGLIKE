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
        OnTileUnits = new Dictionary<HexNode, Unit>();
    }

    [SerializeField] ScriptableGrid _scriptableGrid;
    //[SerializeField] bool _drawConnections;

    public Dictionary<Vector2, HexNode> Tiles { get; private set; }
    public Dictionary<HexNode, Unit> OnTileUnits { get; private set; }
    public HexNode selectedNode;

    HexNode _playerNodeBase, _goalNodeBase;

    public void RevertTiles()
    {
        foreach (var t in Tiles.Values) t.RevertTile();
        RevertAbles();
    }
    public void RevertAbles()
    {
        foreach (var t in Tiles.Values) t.RevertAble();
    }
    public void SelectNodes(List<HexCoords> coordses, SelectOutline outline)
    {
        List<HexNode> tiles = new();
        foreach (HexCoords c in coordses)
        {
            if (Tiles.ContainsKey(c.Pos))
                tiles.Add(Tiles[c.Pos]);
        }
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

    #region OnTileUnit
    public void OnTile(HexNode hexNode, Unit unit, bool onTile = true)
    {
        if (onTile)
            OnTileUnits.Add(hexNode, unit);
        else
            OnTileUnits.Remove(hexNode);
        SetWalkable();
    }
    public void OnTile(HexCoords hexCoords, Unit unit, bool onTile = true)
    {
        if (onTile)
            OnTileUnits.Add(Tiles[hexCoords.Pos], unit);
        else
            OnTileUnits.Remove(Tiles[hexCoords.Pos]);
        SetWalkable();
    }
    public void OnTileMove(HexNode prevNode, HexNode nextNode, Unit unit)
    {
        if (prevNode == nextNode) return;
        OnTileUnits.Add(nextNode, unit);
        OnTileUnits.Remove(prevNode);
        SetWalkable();
    }
    public void OnTileMove(HexCoords prevCoords, HexCoords nextCoords, Unit unit)
    {
        if (prevCoords == nextCoords) return;
        OnTileUnits.Add(Tiles[nextCoords.Pos], unit);
        OnTileUnits.Remove(Tiles[prevCoords.Pos]);
        SetWalkable();
    }
    public void OnTileRemove(HexNode hexNode)
    {
        OnTileUnits.Remove(hexNode);
        SetWalkable();
    }
    public void OnTileRemove(HexCoords hexCoords)
    {
        OnTileUnits.Remove(Tiles[hexCoords.Pos]);
        SetWalkable();
    }
    #endregion    
    #region GetNode
    public HexNode GetTile(Unit unit) => Tiles.TryGetValue(unit.coords.Pos, out var tile) ? tile : null;
    public HexNode GetTile(HexCoords coords) => Tiles.TryGetValue(coords.Pos, out var tile) ? tile : null;
    public HexNode GetTile(Vector2 pos) => Tiles.TryGetValue(pos, out var tile) ? tile : null;
    #endregion
    #region GetUnit
    public Unit GetUnit(HexNode hexNode)
    {
        if(OnTileUnits.ContainsKey(hexNode))
            return OnTileUnits[hexNode];
        return null;
    }
    public Unit GetUnit(HexCoords hexCoords)
    {
        if (Tiles.ContainsKey(hexCoords.Pos) && OnTileUnits.ContainsKey(Tiles[hexCoords.Pos]))
            return OnTileUnits[Tiles[hexCoords.Pos]];
        return null;
    }
    public Unit GetUnit(Vector2 pos)
    {
        if (Tiles.ContainsKey(pos) && OnTileUnits.ContainsKey(Tiles[pos]))
            return OnTileUnits[Tiles[pos]];
        return null;
    }
    #endregion


    public void SetWalkable()
    {
        foreach (KeyValuePair<Vector2, HexNode> tile in Tiles)
            tile.Value.onUnit = OnTileUnits.ContainsKey(tile.Value);
    }

    public HexNode GetRandomNode()
    {
        return Tiles.Where(t => t.Value.CanWalk()).OrderBy(t => Random.value).First().Value;
    }

    /*void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_drawConnections) return;
        Gizmos.color = Color.red;
        foreach (var t in Tiles)
        {
            if (t.Value.Connection == null) continue;
            Gizmos.DrawLine((Vector3)t.Key + new Vector3(0, 0, -1), (Vector3)t.Value.Connection.Coords.Pos + new Vector3(0, 0, -1));
        }
    }*/
}