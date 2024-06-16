using System;
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

    public List<Unit> units;

    public Unit commander;
    public List<Unit> allies;
    public List<Unit> enemies;

    public static Unit sUnit;
    private UnitMove _sUnitMove;
    private UnitCard _sUnitCard;

    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform allyBundle;
    [SerializeField] private Transform enemyBundle;

    public bool isDrag;

    [Header("메테리얼")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material defaultMaterial;
    
    [Header("스프라이트")]
    [SerializeField] private Sprite attackSprite;
    
    public static event Action<bool> OnUnitMove;

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
            unit.Init(unit.unitSO, GridManager.inst.GetRandomNode().Coords);
            switch (unit.unitSO.type)
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
                    throw new ArgumentOutOfRangeException();
            }
            units.Add(unit);
        }
    }
    public Unit SpawnUnit(UnitSO unitSO, HexNode tile)
    {
        if (TurnManager.Inst.MoveCost < unitSO.cost)
            return null;

        TurnManager.UseMoveCost(unitSO.cost);
        
        var unit = Instantiate(unitPrefab);
        unit.Init(unitSO, tile.Coords);

        switch (unitSO.type)
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
                throw new ArgumentOutOfRangeException();
        }
        units.Add(unit);

        return unit;
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

        switch (unit.unitSO.type)
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
            default:
                throw new ArgumentOutOfRangeException();
        }

        units.Remove(unit);
        HealthManager.inst.DestroyHealthBar(unit);
        GridManager.inst.SetTileUnitRemove(unit);
        GridManager.inst.RevertTiles(unit);
        DestroyImmediate(unit.gameObject);
    }

    private void OnTurnStarted(bool playerTurn)
    {
        if (playerTurn)
        {
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                StartCoroutine(EnemySelectCard(enemy));
            }
            
            enemies = enemies.OrderByDescending(x => x.coords.GetPathDistance(x.targetUnit.coords)).ToList();
        }
        else
        {
        }
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

        if (unit.unitSO.type != UnitType.Ally)
            return;
        
        GridManager.inst.RevertTiles(unit);
        LightManager.inst.ChangeLight(true);
        unit.move.DrawArea();
    }
    public void UnitMouseExit(Unit unit)
    {
        if (unit.unitSO.type == UnitType.Ally)
        {
            if (isDrag)
            {
                
            }
            else
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
        if (unit.unitSO.type == UnitType.Ally)
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
        if (!isDrag)
            return;
        
        GridManager.inst.selectedNode?.Use();
        isDrag = false;
        GridManager.inst.RevertTiles(unit);
        LightManager.inst.ChangeLight(false);
    }

    public void ShowUnitInfo(Unit unit)
    {
        unit.SetMaterial(outlineMaterial);
        LightManager.inst.ChangeLight(true);
        CameraManager.inst.SetOrthographicSize(true);
        CameraManager.inst.SetViewPoint(unit.transform.position + new Vector3(0, 0.5f));
        UIManager.inst.OpenInfoPanel();
    }
    public void SelectUnit(Unit unit, bool isCard = false)
    {
        if (sUnit)
            DeSelectUnit(sUnit);

        sUnit = unit;
        _sUnitMove = sUnit.move;
        _sUnitCard = sUnit.card;

        unit.SetMaterial(outlineMaterial);

        if (isCard || unit.unitSO.type == UnitType.Enemy) return;
            
        switch (TurnManager.Inst.phase)
        {
            case Phase.Card:
                _sUnitMove.DrawArea();
                break;
            default:
                _sUnitMove.DrawArea(true);
                break;
        }
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

    public void InitEnemiesArea()
    {
        foreach (var unit in enemies)
        {
            unit.card.SetTargetCoords();
        }
    }
    
    public void DrawCardArea()
    {
        GridManager.inst.RevertTiles(sUnit);
        _sUnitCard.DrawArea(null, false);
    }
    public void DrawMoveArea()
    {
        GridManager.inst.RevertTiles(sUnit);
        _sUnitMove.DrawArea(true);
    }

    #region UnitAlgorithm

    private IEnumerator EnemySelectCard(Unit unit)
    {
        List<CardSO> cardSOs = new();
        foreach (var cardInfo in unit.unitSO.cardInfo)
        {
            if (cardInfo.cardSO.conditions.Count == 0)
            {
                for (var i = 0; i < cardInfo.count; i++)
                    cardSOs.Add(cardInfo.cardSO);
            }
            else
            {
                var cardSO = cardInfo.cardSO;
                var contentCondition = true;
                foreach (var condition in cardInfo.cardSO.conditions)
                {
                    switch (condition.activatedType)
                    {
                        case ActivatedType.Health:
                            float leftValue = unit.hp;
                            float rightValue = unit.unitSO.hp * condition.value * 0.01f;
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
                                    if (Mathf.Approximately(leftValue, rightValue))
                                        contentCondition = false;
                                    break;
                            }
                            break;
                        case ActivatedType.Range:
                            List<HexCoords> targetArea = new();
                            foreach (var ally in allies)
                                targetArea.AddRange(ally.card.GetArea(cardInfo.cardSO));
                            if (!(cardInfo.cardSO.isBeforeMove ? targetArea.Exists(x => unit.move.GetArea(true).Contains(x)) : targetArea.Contains(unit.coords)))
                                contentCondition = false;
                            break;
                        case ActivatedType.Duration:
                            if (cardSO.UpdateDuration() > 0)
                                contentCondition = false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (!contentCondition) continue;
                for (var i = 0; i < cardInfo.count; i++)
                    cardSOs.Add(cardSO);
            }
        }

        var maxPriority = cardSOs.Max(cardSO => cardSO.priority);
        cardSOs = cardSOs.FindAll(cardSO => cardSO.priority == maxPriority);

        var selectedCardSO = cardSOs[Random.Range(0, cardSOs.Count)];
        //var value = selectedCardSO.value + Mathf.CeilToInt(selectedCardSO.value * Random.Range(-0.15f, 0.15f));
        unit.card.SetUp(selectedCardSO);
        
        var targetUnit = GetNearestUnit(unit);
        unit.targetUnit = targetUnit;
        
        if (selectedCardSO.useType == UseType.Should)
        {
            if(selectedCardSO.isBeforeMove)
                yield return StartCoroutine(MoveUnit(unit, targetUnit));
            
            unit.card.SetOffsetCoords();
            unit.card.SetTargetCoords();
        }

        var sprite = attackSprite;
        unit.Repeat(unit.targetCoords.Pos.x);
        unit.ShowAction(sprite);
    }
    
    public IEnumerator EnemyMove(Unit unit, bool ableAction)
    {
        if (ableAction)
        {
            if (!unit.targetUnit)
                yield break;
            
            yield return StartCoroutine(MoveUnit(unit, unit.targetUnit));

            yield return YieldInstructionCache.WaitForSeconds(1f);
        }
        else
        {

        }
    }
    public IEnumerator EnemyAct(Unit unit, bool ableAction)
    {
        if (ableAction)
        {
            if (!unit.targetUnit)
                yield break;

            if (!unit.targetUnit.card.GetArea(unit.card.CardSO).Contains(unit.coords))
                yield break;

            yield return StartCoroutine(unit.card.UseCard(GridManager.inst.GetNode(unit.targetUnit)));
            yield return YieldInstructionCache.WaitForSeconds(1f);
        }
        else
        {
            print("ShouldAction");
            yield return StartCoroutine(unit.card.UseCard(GridManager.inst.GetNode(unit.targetCoords)));
            yield return YieldInstructionCache.WaitForSeconds(1f);
        }
    }

    private IEnumerator MoveUnit(Unit unit, Unit targetUnit)
    {
        var cardData = unit.card.CardSO;

        unit.Repeat(targetUnit.transform.position.x);

        var targetDistance = 1;
        switch (unit.card.CardSO.recommendedDistanceType)
        {
            case RecommendedDistanceType.Far:
                targetDistance = unit.card.CardSO.range;
                break;
            case RecommendedDistanceType.Close:
                targetDistance = 1;
                break;
            case RecommendedDistanceType.Custom:
                targetDistance = unit.card.CardSO.recommendedDistance;
                break;
        }

        var targetArea = cardData.compareByMove ? unit.move.GetArea(true) : targetUnit.card.GetArea(unit.card.CardSO, unit);
        targetArea = targetArea.FindAll(x => GridManager.inst.GetNode(x).CanWalk() || x == unit.coords);
        var targetCoordses = targetArea.FindAll(x => Mathf.Approximately(x.GetDistance(targetUnit.coords), targetDistance) && unit.move.GetArea(true).Contains(x));
        for (int i = targetDistance - 1; i > 0 && targetCoordses.Count == 0; i--)
        {
            targetCoordses = targetArea.FindAll(x => Mathf.Approximately(x.GetDistance(targetUnit.coords), i) && unit.move.GetArea(true).Contains(x));
        }


        HexCoords targetCoords;
        if (targetCoordses.Count == 0)
        {
            targetArea = targetArea.FindAll(x => Mathf.Approximately(x.GetDistance(targetUnit.coords), targetDistance)).OrderBy(x => x.GetPathDistance(unit.coords)).ToList(); //수정필요
            if (targetArea.Count == 0)
                targetCoords = targetUnit.coords;
            else
                targetCoords = targetArea[0]; //수정필요
        }
        else
        {
            targetCoords = targetCoordses[Random.Range(0, targetCoordses.Count)];
        }

        yield return StartCoroutine(unit.move.OnMoveInRange(targetCoords, unit.unitSO.enemyMoveRange));
    }
    public Unit GetNearestUnit(Unit unit) //가까운 유닛 탐색, 거리가 같으면 원래 유닛 타겟 고정
    {
        if (unit.card.CardSO.rangeType == RangeType.Self)
            return unit;

        var targetUnits = unit.unitSO.onTargetToEnemy ? allies : enemies;
        
        var minDistance = targetUnits.Where(x => x != unit).Min(x => x.coords.GetPathDistance(unit.coords));
        var targetUnit = targetUnits.Where(x => x != unit).OrderBy(x => x.coords.GetPathDistance(unit.coords)).First();
        
        return unit.targetUnit?.coords.GetPathDistance(unit.coords) == minDistance ? unit.targetUnit : targetUnit;
    }
    
    #endregion

    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
}
