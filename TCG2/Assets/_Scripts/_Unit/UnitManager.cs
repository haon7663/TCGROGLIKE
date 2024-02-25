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

    [Header("Graphic")]
    [SerializeField] Material outlineMaterial;
    [SerializeField] Material defaultMaterial;

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
            switch (unit.data.type)
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
        switch (unit.data.type)
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
        GridManager.Inst.SetTileUnitRemove(unit);
        DestroyImmediate(unit.gameObject);
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

    public void UnitMouseOver(Unit unit)
    {
        unit.SetMaterial(outlineMaterial);
    }
    public void UnitMouseExit(Unit unit)
    {
        if (unit != sUnit)
            unit.SetMaterial(defaultMaterial);
    }
    public void UnitMouseDown(Unit unit)
    {
        SelectUnit(unit);
    }

    public void SelectUnit(Unit unit, bool isCard = false)
    {
        sUnit = unit;
        sUnit_Move = sUnit.move;
        sUnit_Card = sUnit.card;

        foreach (Unit other in Units)
        {
            other.SetMaterial(defaultMaterial);
        }
        unit.SetMaterial(outlineMaterial);

        if (Enemies.Contains(unit))
        {
            GridManager.Inst.RevertTiles();
            unit.move.DrawArea(false);
            unit.card.DrawArea(unit.card.data, false);
        }
        else
        {
            cinevirtual.Follow = sUnit.transform;
            GridManager.Inst.RevertTiles();
            if (isCard) return;
            switch (TurnManager.Inst.paze)
            {
                case Paze.Draw | Paze.End | Paze.Enemy:
                    sUnit_Move.DrawArea(false);
                    break;
                case Paze.Commander:
                    sUnit_Move.DrawArea();
                    break;
                case Paze.Card:
                    sUnit_Move.DrawArea(false);
                    break;
            }
        }
    }
    public void DeSelectUnit(Unit unit)
    {
        unit.SetMaterial(defaultMaterial);
        unit.card.Cancel();
    }

    public void AutoSelectCard(Unit unit)
    {
        List<CardInfo> cardInfos = new();
        foreach (CardInfo cardInfo in unit.data._CardInfo)
        {
            if (cardInfo.data.conditions.Count == 0)
            {
                for (int i = 0; i < cardInfo.count; i++)
                    cardInfos.Add(new CardInfo(cardInfo));
            }
            else
            {
                foreach (Condition condition in cardInfo.data.conditions)
                {
                    switch (condition.activatedType)
                    {
                        case ActivatedType.Health:
                            float leftValue = unit.hp;
                            float rightValue = unit.data.hp * condition.value * 0.01f;
                            switch (condition.conditionType)
                            {
                                case ConditionType.Less:
                                    if (leftValue <= rightValue)
                                        for (int i = 0; i < cardInfo.count; i++)
                                            cardInfos.Add(new CardInfo(cardInfo));
                                    break;
                                case ConditionType.Greater:
                                    if (leftValue >= rightValue)
                                        for (int i = 0; i < cardInfo.count; i++)
                                            cardInfos.Add(new CardInfo(cardInfo));
                                    break;
                                case ConditionType.Equal:
                                    if (leftValue == rightValue)
                                        for (int i = 0; i < cardInfo.count; i++)
                                            cardInfos.Add(new CardInfo(cardInfo));
                                    break;
                            }
                            break;
                        case ActivatedType.Range:
                            var contains = false;
                            foreach (Unit ally in Allies)
                            {
                                if (unit.card.GetArea(cardInfo.data).Contains(ally.coords))
                                    contains = true;
                            }
                            if(contains)
                                for (int i = 0; i < cardInfo.count; i++)
                                    cardInfos.Add(new CardInfo(cardInfo));
                            break;
                    }
                }
            }
            /*for (int i = 0; i < cardInfo.count; i++)
            {
                cardInfos.Add(new CardInfo(Instantiate(cardInfo.data), cardInfo.count, cardInfo.unit));
            }*/
        }

        List<CardInfo> highCardInfos = new();
        var maxPriority = 0;
        foreach (var cardInfo in cardInfos)
        {
            if (cardInfo.priority >= maxPriority)
            {
                maxPriority = cardInfo.priority;
                highCardInfos.Add(cardInfo);
            }
        }

        cardInfos = highCardInfos.FindAll(x => x.priority == maxPriority);
        int rand = Random.Range(0, cardInfos.Count);
        unit.card.SetUp(cardInfos[rand]);
        if (cardInfos[rand].data.useType == UseType.Should)
        {
            unit.targetCoords = GetNearestUnit2(unit, true).coords;
            unit.SetFlipX(unit.transform.position.x < unit.targetCoords.Pos.x);
        }
    }
    public IEnumerator AutoAction(Unit unit, bool isEnemy = true)
    {
        var useType = unit.card.data.useType;
        if (useType == UseType.Able)
        {
            Unit targetUnit = GetNearestUnit2(unit, isEnemy);
            if (targetUnit)
            {
                unit.SetFlipX(unit.transform.position.x < targetUnit.transform.position.x);
                var targetCoords = FollowRange(unit, targetUnit); //NullTarget == Attack
                if (targetCoords == null)
                {
                    unit.card.UseCard(GridManager.Inst.GetTile(targetUnit));
                }
                else
                {
                    if (StatusManager.CanMove(unit))
                    {
                        unit.move.OnMove((HexCoords)targetCoords);
                    }

                    yield return YieldInstructionCache.WaitForSeconds(0.5f);

                    targetCoords = FollowRange(unit, targetUnit); //NullTarget == Attack
                    if (targetCoords == null)
                    {
                        unit.card.UseCard(GridManager.Inst.GetTile(targetUnit));
                    }
                }
            }
        }
        else if (useType == UseType.Should)
        {
            unit.card.UseCard(GridManager.Inst.GetTile(unit.targetCoords));
        }
    }

    #region UnitAlgorithm
    public void FollowUnit(Unit startUnit, Unit targetUnit)
    {
        var coordses = GetMinCoordses(startUnit, targetUnit.coords);

        startUnit.move.OnMove(coordses[Random.Range(0, coordses.Count)]);
    }
    public HexCoords? FollowRange(Unit startUnit, Unit targetUnit)
    {
        Dictionary<HexCoords, float> coordses = new();
        var minDistance = 10000f;
        foreach (var coords in targetUnit.card.GetArea(startUnit.card.data))
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
        if (startUnit.card.data.shouldClose)
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
