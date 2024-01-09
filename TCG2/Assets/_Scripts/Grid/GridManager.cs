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

    [SerializeField] Sprite _playerSprite, _goalSprite;
    [SerializeField] Unit _unitPrefab;
    [SerializeField] ScriptableGrid _scriptableGrid;
    [SerializeField] bool _drawConnections;

    public Dictionary<Vector2, HexNode> Tiles { get; private set; }
    public Dictionary<HexNode, Unit> OnTileUnits { get; private set; }
    public List<HexNode> selectedNode;

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
    public void OnSelect(List<HexCoords> coordses, SelectOutline outline)
    {
        List<HexNode> nodes = new();
        foreach (HexCoords coords in coordses)
        {
            if (Tiles.ContainsKey(coords.Pos))
                nodes.Add(Tiles[coords.Pos]);
        }
        foreach (HexNode node in nodes)
        {
            node.OnDisplay(outline, nodes);
        }
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
        OnTileUnits.Add(nextNode, unit);
        OnTileUnits.Remove(prevNode);
        SetWalkable();
    }
    public void OnTileMove(HexCoords prevCoords, HexCoords nextCoords, Unit unit)
    {
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
            tile.Value.walkAble = !OnTileUnits.ContainsKey(tile.Value);
    }

    void SpawnUnits()
    {
        //_playerNodeBase = Tiles.Where(t => t.Value.Walkable).OrderBy(t => Random.value).First().Value;
        //_spawnedPlayer = Instantiate(_unitPrefab, _playerNodeBase.Coords.Pos, Quaternion.identity);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_drawConnections) return;
        Gizmos.color = Color.red;
        foreach (var tile in Tiles)
        {
            if (tile.Value.Connection == null) continue;
            Gizmos.DrawLine((Vector3)tile.Key + new Vector3(0, 0, -1), (Vector3)tile.Value.Connection.Coords.Pos + new Vector3(0, 0, -1));
        }
    }
}