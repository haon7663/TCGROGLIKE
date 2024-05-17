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

    [Header("메테리얼")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material defaultMaterial;
    
    [Header("스프라이트")]
    [SerializeField] private Sprite attackSprite;

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
            unit.Init(unit.data, GridManager.inst.GetRandomNode().Coords);
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
    public void SpawnUnit(UnitData unitData, HexNode tile)
    {
        var unit = Instantiate(unitPrefab);
        unit.Init(unitData, tile.Coords);

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
        HealthManager.inst.DestroyHealthBar(unit);
        GridManager.inst.SetTileUnitRemove(unit);
        GridManager.inst.RevertTiles(unit);
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
            GridManager.inst.RevertTiles(unit);
            LightManager.inst.ChangeLight(true);
            unit.move.DrawArea();
        }
    }
    public void UnitMouseExit(Unit unit)
    {
        if (GameManager.Inst.moveAble && unit.data.type == UnitType.Ally)
        {
            if (!isDrag)
            {
                GridManager.inst.RevertTiles(unit);
                LightManager.inst.ChangeLight(false);
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
            if(GridManager.inst.selectedNode)
            {
                
            }

            isDrag = false;
            GridManager.inst.RevertTiles(unit);
            LightManager.inst.ChangeLight(false);
        }
    }

    public void SelectUnit(Unit unit, bool isCard = false)
    {
        if (sUnit)
            DeSelectUnit(sUnit);

        sUnit = unit;
        _sUnitMove = sUnit.move;
        _sUnitCard = sUnit.card;

        unit.SetMaterial(outlineMaterial);

        LightManager.inst.ChangeLight(true);
        if (enemies.Contains(unit))
        {
            CameraManager.inst.SetOrthographicSize(true);
            CameraManager.inst.SetViewPoint(sUnit.transform.position + new Vector3(0, 1.5f));
        }
        else
        {
            CameraManager.inst.SetOrthographicSize(false);
            CameraManager.inst.SetViewPoint(sUnit.transform.position);

            GridManager.inst.RevertTiles(unit);
            if (isCard) return;
            
            switch (TurnManager.Inst.paze)
            {
                case Paze.Card:
                    _sUnitMove.DrawArea();
                    break;
                default:
                    _sUnitMove.DrawArea(false);
                    break;
            }
        }
        
        UIManager.inst.OpenInfoPanel();
    }
    public void DeSelectUnit(Unit unit)
    {
        if (!unit)
            return;
        
        unit.SetMaterial(defaultMaterial);
        unit.card.Cancel();
        GridManager.inst.RevertTiles(unit);
        LightManager.inst.ChangeLight(false);
        UIManager.inst.CloseInfoPanel();

        if (sUnit != unit)
            return;

        sUnit = null;
        _sUnitMove = null;
        _sUnitCard = null;
    }
    
    public void DrawCardArea()
    {
        GridManager.inst.RevertTiles(sUnit);
        _sUnitCard.DrawRange(null, false);
    }
    public void DrawMoveArea()
    {
        GridManager.inst.RevertTiles(sUnit);
        _sUnitMove.DrawArea(false);
    }

    #region UnitAlgorithm

    public IEnumerator AutoSelectCard(Unit unit)
    {
        List<CardInfo> cardInfos = new();
        foreach (CardInfo cardInfo in unit.data.cardInfo)
        {
            if (cardInfo.data.conditions.Count == 0)
            {
                for (int i = 0; i < cardInfo.count; i++)
                    cardInfos.Add(cardInfo);
            }
            else
            {
                bool contentCondition = true;
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
                                        contentCondition = false;
                                    break;
                                case ConditionType.Greater:
                                    if (leftValue <= rightValue)
                                        contentCondition = false;
                                    break;
                                case ConditionType.Equal:
                                    if (leftValue == rightValue)
                                        contentCondition = false;
                                    break;
                            }
                            break;
                        case ActivatedType.Range:
                            List<HexCoords> targetArea = new();
                            foreach (Unit ally in allies)
                                targetArea.AddRange(ally.card.GetArea(cardInfo.data));
                            if (!(cardInfo.data.isBeforeMove ? targetArea.Exists(x => unit.move.GetArea(true).Contains(x)) : targetArea.Contains(unit.coords)))
                                contentCondition = false;
                            break;
                        case ActivatedType.Count:
                            if (cardInfo.turnCount-- > 0)
                            {
                                contentCondition = false;
                            }
                            break;
                    }
                }
                if(contentCondition)
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
            var targetUnit = GetNearestUnit(unit);
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

                if (unit.targetUnit.card.GetArea(unit.card.CardData).Contains(unit.coords))
                {
                    yield return YieldInstructionCache.WaitForSeconds(0.7f);
                    yield return StartCoroutine(unit.card.UseCard(GridManager.inst.GetTile(unit.targetUnit)));
                }
            }
        }
        else
        {
            yield return StartCoroutine(unit.card.UseCard(GridManager.inst.GetTile(unit.targetCoords)));
            /*foreach(var displayObject in unit.card.selectedTiles)
            {
                Destroy(displayObject);
            }*/
        }
    }
    private IEnumerator MoveUnit(Unit unit, Unit targetUnit)
    {
        if (StatusManager.CanMove(unit))
        {
            unit.SetFlipX(unit.transform.position.x < targetUnit.transform.position.x);

            var targetDistance = 1;
            switch (unit.card.CardData.recommendedDistanceType)
            {
                case RecommendedDistanceType.Far:
                    targetDistance = unit.card.CardData.range;
                    break;
                case RecommendedDistanceType.Close:
                    targetDistance = 1;
                    break;
                case RecommendedDistanceType.Custom:
                    targetDistance = unit.card.CardData.recommendedDistance;
                    break;
            }

            List<HexCoords> targetArea = unit.card.CardData.rangeType == RangeType.Self ? unit.move.GetArea(true) : targetUnit.card.GetArea(unit.card.CardData, unit);
            targetArea = targetArea.FindAll(x => GridManager.inst.GetTile(x).CanWalk() || x == unit.coords);
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
    public Unit GetNearestUnit(Unit unit) //가까운 유닛 탐색, 거리가 같으면 원래 유닛 타겟 고정
    {
        if (unit.card.CardData.rangeType == RangeType.Self)
            return unit;

        Unit targetUnit = null;
        var minDistance = 10000f;
        foreach (Unit target in unit.card.CardData.cardType == CardType.Attack ? allies : enemies)
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
    
    #endregion

    void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
}
