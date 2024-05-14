using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum UnitType { Ally, Enemy, Commander }

public class UnitManager : MonoBehaviour
{
    public static UnitManager inst;
    private void Awake() => inst = this;

    [HideInInspector]
    public List<Unit> units;

    public Unit commander;
    public List<Unit> allies;
    public List<Unit> enemies;

    public static Unit sUnit;
    private Unit_Move _sUnitMove;
    private Unit_Card _sUnitCard;

    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform allyBundle;
    [SerializeField] private Transform enemyBundle;

    public bool isDrag;

    [Header("Graphic")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material defaultMaterial;
    [Space]
    [SerializeField] private Sprite attackSprite;
    [Space]
    [SerializeField] private GameObject infoPanel;

    private void Start()
    {
        FindUnits();
        TurnManager.OnTurnStarted += OnTurnStarted;
        CardManager.Inst.StartSet();
    }
    private void FindUnits()
    {
        foreach (var unit in FindObjectsOfType<Unit>())
        {
            unit.Init(unit.data, GridManager.Inst.GetRandomNode().coords);
            switch (unit.data.type)
            {
                case UnitType.Commander:
                    commander = unit;
                    allies.Add(unit);
                    break;
                case UnitType.Ally:
                    allies.Add(unit);
                    break;
                case UnitType.Enemy:
                    enemies.Add(unit);
                    break;
                default:
                    break;
            }
            units.Add(unit);
        }
    }
    public void SpawnUnit(UnitSO unitData, HexNode tile)
    {
        var unit = Instantiate(unitPrefab);
        unit.Init(unitData, tile.coords);

        switch (unitData.type)
        {
            case UnitType.Commander:
                allies.Add(unit);
                break;
            case UnitType.Ally:
                allies.Add(unit);
                unit.transform.SetParent(allyBundle);
                break;
            case UnitType.Enemy:
                enemies.Add(unit);
                unit.transform.SetParent(enemyBundle);
                break;
            default:
                break;
        }
        units.Add(unit);
    }
    public void Death(Unit unit)
    {
        //사망 이벤트 삽입
        sUnit = unit;
        Retreat();
    }
    public void Retreat()
    {
        var unit = sUnit;
        DeSelectUnit(unit);

        switch (unit.data.type)
        {
            case UnitType.Commander:
                allies.Remove(unit);
                break;
            case UnitType.Ally:
                allies.Remove(unit);
                break;
            case UnitType.Enemy:
                enemies.Remove(unit);
                break;
        }

        units.Remove(unit);
        HealthManager.Inst.DestroyHealthBar(unit);
        GridManager.Inst.SetTileUnitRemove(unit);
        GridManager.Inst.RevertTiles(unit);
        DestroyImmediate(unit.gameObject);
    }

    private void OnTurnStarted(bool myTurn)
    {

    }

    public void SetOrderUnits(bool isFront)
    {
        foreach (var unit in units)
        {
            unit.transform.position = new Vector3(unit.coords.Pos.x, unit.coords.Pos.y, isFront ? -1 : 1);
        }
    }

    public void UnitMouseOver(Unit unit)
    {
        unit.SetMaterial(outlineMaterial);

        if (GameManager.Inst.moveAble && unit.data.type == UnitType.Ally)
        {
            GridManager.Inst.RevertTiles(unit);
            LightManager.Inst.ChangeLight(true);
            unit.move.DrawArea();
        }
    }
    public void UnitMouseExit(Unit unit)
    {
        if (GameManager.Inst.moveAble && unit.data.type == UnitType.Ally)
        {
            if (!isDrag)
            {
                GridManager.Inst.RevertTiles(unit);
                LightManager.Inst.ChangeLight(false);
            }
        }
        else
        {
            if (unit != sUnit)
            {
                unit.SetMaterial(defaultMaterial);
                /*if(!GameManager.Inst.onDisplayActions)
                    unit.card.DisplayObjects(false);*/
            }
        }
    }
    public void UnitMouseDown(Unit unit)
    {
        if (GameManager.Inst.moveAble && unit.data.type == UnitType.Ally)
        {
            isDrag = true;
        }
        else
        {
            SelectUnit(unit);
        }
    }
    public void UnitMouseUp(Unit unit)
    {
        if (GameManager.Inst.moveAble && unit.data.type == UnitType.Ally)
        {
            if(GridManager.Inst.selectedNode)
            {
                
            }

            isDrag = false;
            GridManager.Inst.RevertTiles(unit);
            LightManager.Inst.ChangeLight(false);
        }
    }

    public void SelectUnit(Unit unit, bool isCard = false)
    {
        if (sUnit)
            DeSelectUnit(sUnit);

        sUnit = unit;
        _sUnitMove = sUnit.move;
        _sUnitCard = sUnit.card;

        foreach (Unit other in units)
        {
            other.SetMaterial(defaultMaterial);
            /*if (!GameManager.Inst.onDisplayActions)
                other.card.DisplayObjects(false);*/
        }
        unit.SetMaterial(outlineMaterial);

        LightManager.Inst.ChangeLight(true);
        if (enemies.Contains(unit))
        {
            /*if (unit.card.canDisplay)
            {
                unit.card.DisplayObjects(true);
            }*/

            //DrawMoveArea();
            //GridManager.Inst.SelectNodes(AreaType.Default, false, GridManager.Inst.Tiles.Values.ToList(), unit);

            CinemachineManager.Inst.SetOrthoSize(true);
            CinemachineManager.Inst.SetViewPoint(sUnit.transform.position + new Vector3(0, 1.5f));
        }
        else
        {
            CinemachineManager.Inst.SetOrthoSize(false);
            CinemachineManager.Inst.SetViewPoint(sUnit.transform.position);

            GridManager.Inst.RevertTiles(unit);
            if (isCard) return;

            switch (TurnManager.Inst.paze)
            {
                case Paze.Draw | Paze.End | Paze.Enemy:
                    _sUnitMove.DrawArea(false);
                    break;
                case Paze.Commander:
                    _sUnitMove.DrawArea();
                    break;
                case Paze.Card:
                    _sUnitMove.DrawArea(false);
                    break;
            }
        }

        infoPanel.SetActive(true); //정보 패널 표시 분류 작업 나중에 하기
        infoPanel.transform.GetChild(2).gameObject.SetActive(unit.data.type == UnitType.Ally);
    }
    public void DrawCardArea()
    {
        GridManager.Inst.RevertTiles(sUnit);
        _sUnitCard.DrawArea(null, false);
    }
    public void DrawMoveArea()
    {
        GridManager.Inst.RevertTiles(sUnit);
        _sUnitMove.DrawArea(false);
    }
    public void DeSelectUnit(Unit unit)
    {
        if(unit)
        {
            infoPanel.SetActive(false);
            unit.SetMaterial(defaultMaterial);
            /*if (!GameManager.Inst.onDisplayActions)
                unit.card.DisplayObjects(false);*/
            GridManager.Inst.RevertTiles(unit);
            LightManager.Inst.ChangeLight(false);
            unit.card.Cancel();

            sUnit = null;
            _sUnitMove = null;
            _sUnitCard = null;
        }
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
                            foreach (Unit ally in allies)
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
            unit.card.canDisplay = true;
            unit.targetCoords = targetUnit.coords;
            unit.SetFlipX(unit.transform.position.x < unit.targetCoords.Pos.x);
            if(info.data.isBeforeMove)
                yield return StartCoroutine(MoveUnit(unit, targetUnit));

            unit.card.directionCoords = targetUnit.coords - unit.coords;
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
                    yield return YieldInstructionCache.WaitForSeconds(0.7f);
                    yield return StartCoroutine(unit.card.UseCard(GridManager.Inst.GetTile(unit.targetUnit)));
                }
            }
        }
        else
        {
            yield return StartCoroutine(unit.card.UseCard(GridManager.Inst.GetTile(unit.targetCoords)));
            /*foreach(var displayObject in unit.card.selectedTiles)
            {
                Destroy(displayObject);
            }*/
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
            return unit;

        Unit targetUnit = null;
        var minDistance = 10000f;
        foreach (Unit target in unit.card.data.cardType == CardType.Attack ? allies : enemies)
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
        foreach (Unit unit in isEnemy ? allies : enemies)
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
