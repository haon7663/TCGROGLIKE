using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Random = UnityEngine.Random;

public enum UnitType { Ally, Enemy, Commander }

public class UnitManager : MonoBehaviour
{
    public static UnitManager Inst;
    void Awake() => Inst = this;

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
    [Space]
    [SerializeField] Sprite attackSprite;
    [Space]
    [SerializeField] GameObject infoPanel;

    void Start()
    {
        FindUnits();
        TurnManager.OnTurnStarted += OnTurnStarted;
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
        if(unit.card.selectedTiles.Count > 0)
        {
            unit.card.DisplayObjects(true);
        }
    }
    public void UnitMouseExit(Unit unit)
    {
        if (unit != sUnit)
        {
            unit.SetMaterial(defaultMaterial);
            unit.card.DisplayObjects(false);
        }
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
            other.card.DisplayObjects(false);
        }
        unit.SetMaterial(outlineMaterial);

        if (Enemies.Contains(unit))
        {
            if (unit.card.selectedTiles.Count > 0)
            {
                unit.card.DisplayObjects(true);
            }

            //DrawMoveArea();
            //GridManager.Inst.ShowEntire(tiles);

            LightManager.Inst.ChangeLight(true);
            CinemachineManager.Inst.SetOrthoSize(4);
            CinemachineManager.Inst.SetViewPoint(sUnit.transform.position + new Vector3(0, 1.5f));

            infoPanel.SetActive(true);
        }
        else
        {
            LightManager.Inst.ChangeLight(false);
            CinemachineManager.Inst.SetOrthoSize(9);
            CinemachineManager.Inst.SetViewPoint(sUnit.transform.position);

            GridManager.Inst.RevertTiles(unit);
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

            infoPanel.SetActive(false);
        }
    }
    public void DrawCardArea()
    {
        GridManager.Inst.RevertTiles(sUnit);
        sUnit_Card.DrawArea(null, false);
    }
    public void DrawMoveArea()
    {
        GridManager.Inst.RevertTiles(sUnit);
        sUnit_Move.DrawArea(false);
    }
    public void DeSelectUnit(Unit unit)
    {
        infoPanel.SetActive(false);
        unit.SetMaterial(defaultMaterial);
        unit.card.DisplayObjects(false);
        unit.card.Cancel();
    }

    public IEnumerator AutoSelectCard(Unit unit)
    {
        List<CardInfo> cardInfos = new();
        foreach (CardInfo cardInfo in unit.data._CardInfo)
        {
            if (cardInfo.data.conditions.Count == 0)
            {
                for (int i = 0; i < cardInfo.count; i++)
                    cardInfos.Add(cardInfo);
            }
            else
            {
                bool saticfiedCondition = true;
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
                                    if (leftValue > rightValue)
                                        saticfiedCondition = false;
                                    break;
                                case ConditionType.Greater:
                                    if (leftValue <= rightValue)
                                        saticfiedCondition = false;
                                    break;
                                case ConditionType.Equal:
                                    if (leftValue == rightValue)
                                        saticfiedCondition = false;
                                    break;
                            }
                            break;
                        case ActivatedType.Range:
                            List<HexCoords> targetArea = new();
                            foreach (Unit ally in Allies)
                                targetArea.AddRange(ally.card.GetArea(cardInfo.data));
                            if (!(cardInfo.data.isBeforeMove ? targetArea.Exists(x => unit.move.GetArea(true).Contains(x)) : targetArea.Contains(unit.coords)))
                                saticfiedCondition = false;
                            break;
                        case ActivatedType.Count:
                            if (cardInfo.turnCount-- > 0)
                            {
                                saticfiedCondition = false;
                            }
                            break;
                    }
                }
                if(saticfiedCondition)
                    for (int i = 0; i < cardInfo.count; i++)
                        cardInfos.Add(cardInfo);
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

        CardInfo info = cardInfos[rand];
        if (info.data.conditions.Exists(x => x.activatedType == ActivatedType.Count))
            info.turnCount = info.data.conditions.Find(x => x.activatedType == ActivatedType.Count).turnCount;
        int value = info.data.value + Mathf.CeilToInt(info.data.value * 0.1f) * Random.Range(-1, 2);
        unit.card.SetUp(info, value);
        if (info.data.useType == UseType.Should)
        {
            var targetUnit = GetNearestUnit2(unit);
            unit.targetCoords = targetUnit.coords;
            unit.SetFlipX(unit.transform.position.x < unit.targetCoords.Pos.x);
            if(info.data.isBeforeMove)
                yield return StartCoroutine(MoveUnit(unit, targetUnit));

            unit.card.selectedTiles = unit.card.GetSelectedArea(GridManager.Inst.GetTile(targetUnit));
        }

        Sprite sprite = attackSprite;
        if (info.data.activeType == ActiveType.Attack)
            sprite = attackSprite;

        unit.ShowAction(sprite, value);
    }
    public IEnumerator Action(Unit unit, bool isAble)
    {
        if (isAble)
        {
            if (unit.targetUnit)
            {
                yield return StartCoroutine(MoveUnit(unit, unit.targetUnit));

                if (unit.targetUnit.card.GetArea(unit.card.data).Contains(unit.coords))
                {
                    unit.card.UseCard(GridManager.Inst.GetTile(unit.targetUnit));
                    yield return YieldInstructionCache.WaitForSeconds(0.5f);
                }
            }
        }
        else
        {
            unit.card.UseCard(GridManager.Inst.GetTile(unit.targetCoords));
            foreach(var displayObject in unit.card.selectedTiles)
            {
                Destroy(displayObject);
            }
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
        }
    }

    #region UnitAlgorithm

    IEnumerator MoveUnit(Unit unit, Unit targetUnit)
    {
        if (StatusManager.CanMove(unit))
        {
            unit.SetFlipX(unit.transform.position.x < targetUnit.transform.position.x);

            var targetDistance = 1;
            switch (unit.card.data.recommendedDistanceType)
            {
                case RecommendedDistanceType.Far:
                    targetDistance = unit.card.data.range;
                    break;
                case RecommendedDistanceType.Close:
                    targetDistance = 1;
                    break;
                case RecommendedDistanceType.Custom:
                    targetDistance = unit.card.data.recommendedDistance;
                    break;
            }

            List<HexCoords> targetArea = unit.card.data.rangeType == RangeType.Self ? unit.move.GetArea(true) : targetUnit.card.GetArea(unit.card.data, unit);
            targetArea = targetArea.FindAll(x => GridManager.Inst.GetTile(x).CanWalk() || x == unit.coords);
            List<HexCoords> targetCoordses = targetArea.FindAll(x => x.GetDistance(targetUnit.coords) == targetDistance && unit.move.GetArea(true).Contains(x));
            for (int i = targetDistance - 1; i > 0 && targetCoordses.Count == 0; i--)
            {
                targetCoordses = targetArea.FindAll(x => x.GetDistance(targetUnit.coords) == i && unit.move.GetArea(true).Contains(x));
            }


            HexCoords targetCoords;
            if (targetCoordses.Count == 0)
            {
                targetArea = targetArea.FindAll(x => x.GetDistance(targetUnit.coords) == targetDistance).OrderBy(x => x.GetPathDistance(unit.coords)).ToList(); //수정필요
                if (targetArea.Count == 0)
                    targetCoords = targetUnit.coords;
                else
                    targetCoords = targetArea[0]; //수정필요
            }
            else
            {
                targetCoords = targetCoordses[Random.Range(0, targetCoordses.Count)];
            }

            yield return StartCoroutine(unit.move.OnMoveInRange(targetCoords, unit.data.range));
        }
    }
    public Unit GetNearestUnit2(Unit unit) //가까운 유닛 탐색, 거리가 같으면 원래 유닛 타겟 고정
    {
        if (unit.card.data.rangeType == RangeType.Self)
            return unit.targetUnit;

        Unit targetUnit = null;
        var minDistance = 10000f;
        foreach (Unit target in unit.card.data.cardType == CardType.Attack ? Allies : Enemies)
        {
            var distance = unit.coords.GetPathDistance(target.coords);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetUnit = target;
            }
        }
        targetUnit = unit.targetUnit?.coords.GetPathDistance(unit.coords) == minDistance ? unit.targetUnit : targetUnit;
        unit.targetUnit = targetUnit;
        return targetUnit;
    }

    #region Trash
    public void FollowUnit(Unit startUnit, Unit targetUnit)
    {
        var coordses = GetMinCoordses(startUnit, targetUnit.coords);

        //startUnit.move.OnMove(coordses[Random.Range(0, coordses.Count)]);
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
        /*if (startUnit.card.data.shouldClose)
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
        */
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
        startUnit.targetUnit = targetUnit;
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
    #endregion

    void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
}
