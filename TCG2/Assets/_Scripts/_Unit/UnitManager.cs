using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum UnitType { Ally, Enemy, Commander }

public class UnitManager : MonoBehaviour
{
    public static UnitManager Inst;
    void Awake() => Inst = this;

    [SerializeField] CinemachineVirtualCamera cinevirtual;

    [HideInInspector]
    public List<Unit> Units;

    public Unit Commander;
    public List<Unit> Allies;
    public List<Unit> Enemies;

    public static Unit sUnit;
    public static Unit_Move sUnit_Move;
    public static Unit_Card sUnit_Card;

    void Start()
    {
        FindUnits();
        TurnManager.OnTurnStarted += OnTurnStarted;
        SelectUnit(Commander);

        CardManager.Inst.StartSet();
    }

    void FindUnits()
    {
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            switch (unit.unitData.type)
            {
                case UnitType.Commander:
                    Commander = unit;
                    Allies.Add(unit);
                    break;
                case UnitType.Ally:
                    Allies.Add(unit);
                    break;
                case UnitType.Enemy:
                    Enemies.Add(unit);
                    break;
            }
            Units.Add(unit);
        }
    }

    public void Death(Unit unit)
    {
        switch (unit.unitData.type)
        {
            case UnitType.Commander:
                Allies.Remove(unit);
                break;
            case UnitType.Ally:
                Allies.Remove(unit);
                break;
            case UnitType.Enemy:
                Enemies.Remove(unit);
                break;
        }
        Units.Remove(unit);
        GridManager.Inst.OnTileRemove(unit.coords);
        Destroy(unit.gameObject);
    }

    void OnTurnStarted(bool myTurn)
    {

    }

    public void SetOrder(bool isFront)
    {
        foreach (Unit unit in Units)
        {
            unit.transform.position = new Vector3(unit.coords.Pos.x, unit.coords.Pos.y, isFront ? -1 : 1);
        }
    }

    public void SelectUnit(Unit unit)
    {
        if (Enemies.Contains(unit))
        {
            unit.move.DrawArea(false);
            unit.card.DrawArea(unit.card.cardData, false);
        }
        else
        {
            sUnit = unit;
            sUnit_Move = sUnit.move;
            sUnit_Card = sUnit.card;
            cinevirtual.Follow = sUnit.transform;
            switch (TurnManager.Inst.paze)
            {
                case Paze.Draw | Paze.End | Paze.Enemy:
                    break;
                case Paze.Move:
                    sUnit_Move.DrawArea();
                    break;
                case Paze.Card:
                    break;
            }
        }
    }

    public void AutoAction(Unit unit, bool isEnemy = true)
    {
        Unit targetUnit = GetNearestUnit2(unit, isEnemy);
        if (targetUnit)
        {
            var targetCoords = FollowRange(unit, targetUnit);

            if (StatusManager.CanMove(unit)) return;
            if (targetCoords != null)
                StartCoroutine(unit.move.OnMove((HexCoords)targetCoords));
        }
    }

    #region UnitAlgorithm
    public void FollowUnit(Unit startUnit, Unit targetUnit)
    {
        var coordses = GetMinCoordses(startUnit, targetUnit.coords);

        StartCoroutine(startUnit.move.OnMove(coordses[Random.Range(0, coordses.Count)]));
    }
    public HexCoords? FollowRange(Unit startUnit, Unit targetUnit)
    {
        Dictionary<HexCoords, float> coordses = new();
        var minDistance = 10000f;
        foreach (var coords in targetUnit.card.GetArea(startUnit.card.cardData))
        {
            if (coords == startUnit.coords) return null;

            var distance = coords.GetPathDistance(startUnit.coords);
            if (distance <= minDistance)
            {
                minDistance = distance;
                coordses.Add(coords, distance);
            }
        }
        var minCoordses = new List<HexCoords>();
        foreach (var coords in coordses)
        {
            if (coords.Value == minDistance)
            {
                minCoordses.Add(coords.Key);
            }
        }

        HexCoords targetCoords = new();
        if (startUnit.card.cardData.shouldClose)
        {
            var min = 10000f;
            foreach (var coords in minCoordses)
            {
                var distance = coords.GetPathDistance(targetUnit.coords);
                if (distance <= min)
                {
                    min = distance;
                    targetCoords = coords;
                }
            }
        }
        else
        {
            var max = 0f;
            foreach (var coords in minCoordses)
            {
                var distance = coords.GetPathDistance(targetUnit.coords);
                if (distance >= max)
                {
                    max = distance;
                    targetCoords = coords;
                }
            }
        }

        var result = GetMinCoordses(startUnit, targetCoords);
        return result[Random.Range(0, result.Count)];
    }
    public Unit GetNearestUnit(Unit startUnit, bool isEnemy = true) //단순 가까운 유닛 탐색, 거리가 같으면 유닛이 바뀔수도 있음
    {
        Unit targetUnit = null;
        var minDistance = 10000f;
        foreach (Unit unit in isEnemy ? Allies : Enemies)
        {
            var distance = startUnit.coords.GetPathDistance(unit.coords);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetUnit = unit;
            }
        }
        startUnit.target = targetUnit;
        return targetUnit;
    }
    public Unit GetNearestUnit2(Unit startUnit, bool isEnemy = true) //가까운 유닛 탐색, 거리가 같으면 원래 유닛 타겟 고정
    {
        Unit targetUnit = null;
        var minDistance = 10000f;
        foreach (Unit unit in isEnemy ? Allies : Enemies)
        {
            var distance = startUnit.coords.GetPathDistance(unit.coords);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetUnit = unit;
            }
        }
        targetUnit = startUnit.target?.coords.GetPathDistance(startUnit.coords) == minDistance ? startUnit.target : targetUnit;
        startUnit.target = targetUnit;
        return targetUnit;
    }

    List<HexCoords> GetMinCoordses(Unit startUnit, HexCoords targetCoords)
    {
        if (startUnit.coords == targetCoords) return new List<HexCoords> { startUnit.coords };

        Dictionary<HexCoords, float> coordses = new();
        var minDistance = 10000f;
        foreach (var coords in startUnit.move.GetArea())
        {
            var distance = coords.GetPathDistance(targetCoords);
            if (distance <= minDistance)
            {
                minDistance = distance;
                coordses.Add(coords, distance);
            }
        }

        var minCoordses = new List<HexCoords>();
        foreach (var coords in coordses)
        {
            if (coords.Value == minDistance)
                minCoordses.Add(coords.Key);
        }

        return minCoordses;
    }
    #endregion

    void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
}
