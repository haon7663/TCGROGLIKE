using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableGrid : ScriptableObject
{
    [SerializeField] protected HexNode nodeBasePrefab;
    [SerializeField, Range(0, 6)] int _obstacleWeight = 3;
    public abstract Dictionary<Vector2, HexNode> GenerateGrid();

    protected bool DecideIfObstacle() => Random.Range(1, 20) <= _obstacleWeight;
}