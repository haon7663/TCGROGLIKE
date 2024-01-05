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
    public void OnMoveSelect(HexCoords hexCoords)
    {
        if (Tiles.ContainsKey(hexCoords.Pos))
        {
            Tiles[hexCoords.Pos].SetSelectOutline(SelectOutline.MoveSelect);
            Tiles[hexCoords.Pos].moveAble = true;
        }
    }
    public void OnMoveSelect(Vector2 hexPos)
    {
        if (Tiles.ContainsKey(hexPos))
        {
            Tiles[hexPos].SetSelectOutline(SelectOutline.MoveSelect);
            Tiles[hexPos].moveAble = true;
        }
    }
    #endregion
    #region Attack
    public void OnAttackSelect(HexCoords hexCoords)
    {
        if (Tiles.ContainsKey(hexCoords.Pos))
        {
            Tiles[hexCoords.Pos].SetSelectOutline(SelectOutline.AttackSelect);
            Tiles[hexCoords.Pos].attackAble = true;
        }
    }
    public void OnAttackSelect(Vector2 hexPos)
    {
        if (Tiles.ContainsKey(hexPos))
        {
            Tiles[hexPos].SetSelectOutline(SelectOutline.AttackSelect);
            Tiles[hexPos].attackAble = true;
        }
    }
    public void OnAttackRange(HexCoords hexCoords)
    {
        if (Tiles[hexCoords.Pos].attackAble)
            return;

        if (Tiles.ContainsKey(hexCoords.Pos))
        {
            Tiles[hexCoords.Pos].SetSelectOutline(SelectOutline.DamageAble);
        }
    }
    public void OnAttackRange(Vector2 hexPos)
    {
        if (Tiles[hexPos].attackAble)
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
    }
    public void OnTile(HexCoords hexCoords, Unit unit, bool onTile = true)
    {
        if (onTile)
            OnTileUnits.Add(Tiles[hexCoords.Pos], unit);
        else
            OnTileUnits.Remove(Tiles[hexCoords.Pos]);
    }
    public void OnTileMove(HexNode prevNode, HexNode nextNode, Unit unit)
    {
        OnTileUnits.Add(nextNode, unit);
        OnTileUnits.Remove(prevNode);
    }
    public void OnTileMove(HexCoords prevCoords, HexCoords nextCoords, Unit unit)
    {
        OnTileUnits.Add(Tiles[nextCoords.Pos], unit);
        OnTileUnits.Remove(Tiles[prevCoords.Pos]);
    }
    public void OnTileRemove(HexNode hexNode)
    {
        OnTileUnits.Remove(hexNode);
    }
    public void OnTileRemove(HexCoords hexCoords)
    {
        OnTileUnits.Remove(Tiles[hexCoords.Pos]);
    }
    public Unit ContainsTile(HexNode hexNode)
    {
        if(OnTileUnits.ContainsKey(hexNode))
            return OnTileUnits[hexNode];
        return null;
    }
    public Unit ContainsTile(HexCoords hexCoords)
    {
        if (Tiles.ContainsKey(hexCoords.Pos) && OnTileUnits.ContainsKey(Tiles[hexCoords.Pos]))
            return OnTileUnits[Tiles[hexCoords.Pos]];
        return null;
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