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
    }

    private static readonly Color MoveColor = new Color(1, 1, .4f);
    private static readonly Color AttackColor = new Color(1, .4f, .4f);

    [SerializeField] Sprite _playerSprite, _goalSprite;
    [SerializeField] Unit _unitPrefab;
    [SerializeField] ScriptableGrid _scriptableGrid;
    [SerializeField] bool _drawConnections;

    public Dictionary<Vector2, HexNode> Tiles { get; private set; }

    HexNode _playerNodeBase, _goalNodeBase;
    Unit _spawnedPlayer, _spawnedGoal;

    public void RevertTiles()
    {
        foreach (var t in Tiles.Values) t.RevertTile();
    }
    public void RevertAbles()
    {
        foreach (var t in Tiles.Values) t.RevertAble();
    }


    public void OnMoveSelect(HexCoords hexCoords)
    {
        if (Tiles.ContainsKey(hexCoords.Pos))
        {
            Tiles[hexCoords.Pos].SetColor(MoveColor);
            Tiles[hexCoords.Pos].moveable = true;
        }
    }
    public void OnMoveSelect(Vector2 hexPos)
    {
        if (Tiles.ContainsKey(hexPos))
        {
            Tiles[hexPos].SetColor(MoveColor);
            Tiles[hexPos].moveable = true;
        }
    }

    public void OnAttackSelect(HexCoords hexCoords)
    {
        if (Tiles.ContainsKey(hexCoords.Pos))
        {
            Tiles[hexCoords.Pos].SetColor(AttackColor);
            Tiles[hexCoords.Pos].attackable = true;
        }
    }
    public void OnAttackSelect(Vector2 hexPos)
    {
        if (Tiles.ContainsKey(hexPos))
        {
            Tiles[hexPos].SetColor(AttackColor);
            Tiles[hexPos].attackable = true;
        }
    }

    void SpawnUnits()
    {
        _playerNodeBase = Tiles.Where(t => t.Value.Walkable).OrderBy(t => Random.value).First().Value;
        _spawnedPlayer = Instantiate(_unitPrefab, _playerNodeBase.Coords.Pos, Quaternion.identity);
        _spawnedPlayer.Init(_playerSprite);

        _spawnedGoal = Instantiate(_unitPrefab, new Vector3(50, 50, 50), Quaternion.identity);
        _spawnedGoal.Init(_goalSprite);
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