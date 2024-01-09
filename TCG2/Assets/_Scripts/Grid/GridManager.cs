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

    #region Move
    public void OnMove(List<HexCoords> coordses, bool canMove = true)
    {
        List<HexNode> nodes = new();
        foreach(HexCoords coords in coordses)
        {
            if (Tiles.ContainsKey(coords.Pos))
                nodes.Add(Tiles[coords.Pos]);
        }
        foreach (HexNode node in nodes)
        {
            node.OnDisplay(SelectOutline.MoveSelect, nodes);
        }
        /*if (Tiles.ContainsKey(coordses.Pos))
        {
            Tiles[coordses.Pos].SetSelectOutline(canMove ? SelectOutline.MoveSelect : SelectOutline.MoveAble);
            Tiles[coordses.Pos].canMove = canMove;
        }*/
    }
    public void OnMove(Vector2 hexPos, bool canMove = true)
    {
        if (Tiles.ContainsKey(hexPos))
        {
            Tiles[hexPos].SetSelectOutline(canMove ? SelectOutline.MoveSelect : SelectOutline.MoveAble);
            Tiles[hexPos].canMove = canMove;
        }
    }
    #endregion
    #region Attack
    public void OnAttackSelect(HexCoords hexCoords)
    {
        if (Tiles.ContainsKey(hexCoords.Pos))
        {
            Tiles[hexCoords.Pos].SetSelectOutline(SelectOutline.AttackSelect);
            Tiles[hexCoords.Pos].canAttack = true;
        }
    }
    public void OnAttackSelect(Vector2 hexPos)
    {
        if (Tiles.ContainsKey(hexPos))
        {
            Tiles[hexPos].SetSelectOutline(SelectOutline.AttackSelect);
            Tiles[hexPos].canAttack = true;
        }
    }
    public void OnAttackRange(HexCoords hexCoords)
    {
        if (Tiles[hexCoords.Pos].canAttack)
            return;

        if (Tiles.ContainsKey(hexCoords.Pos))
        {
            Tiles[hexCoords.Pos].SetSelectOutline(SelectOutline.DamageAble);
        }
    }
    public void OnAttackRange(Vector2 hexPos)
    {
        if (Tiles[hexPos].canAttack)
            return;

        if (Tiles.ContainsKey(hexPos))
        {
            Tiles[hexPos].SetSelectOutline(SelectOutline.DamageAble);
        }
    }
    #endregion
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
    #region ContainsOnTileUnits
    public Unit ContainsOnTileUnits(HexNode hexNode)
    {
        if(OnTileUnits.ContainsKey(hexNode))
            return OnTileUnits[hexNode];
        return null;
    }
    public Unit ContainsOnTileUnits(HexCoords hexCoords)
    {
        if (Tiles.ContainsKey(hexCoords.Pos) && OnTileUnits.ContainsKey(Tiles[hexCoords.Pos]))
            return OnTileUnits[Tiles[hexCoords.Pos]];
        return null;
    }
    #endregion
    public HexNode ContainsUnit(Unit unit)
    {
        if (OnTileUnits.ContainsValue(unit))
            return Tiles[unit.coords.Pos];
        return null;
    }
    public HexNode GetNode(HexCoords coords)
    {
        if (Tiles.ContainsKey(coords.Pos))
            return Tiles[coords.Pos];
        return null;
    }

    public void SetWalkable()
    {
        foreach (KeyValuePair<Vector2, HexNode> tile in Tiles)
            tile.Value.walkAble = !OnTileUnits.ContainsKey(tile.Value);
    }
    #endregion

    void SpawnUnits()
    {
        //_playerNodeBase = Tiles.Where(t => t.Value.Walkable).OrderBy(t => Random.value).First().Value;
        //_spawnedPlayer = Instantiate(_unitPrefab, _playerNodeBase.Coords.Pos, Quaternion.identity);
    }

    public HexNode GetTileAtPosition(Vector2 pos) => Tiles.TryGetValue(pos, out var tile) ? tile : null;

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