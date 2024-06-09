using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum Phase { Draw, Card, End, Enemy }
public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    private void Awake() => Inst = this;

    [Header("Develop")]
    [SerializeField][Tooltip("시작 턴 모드를 정합니다")] private ETurnMode eTurnMode;
    [SerializeField][Tooltip("시작 카드 개수를 정합니다")] private int startCardCount;

    [Header("Properties")]
    public Phase phase = Phase.Draw;
    public bool myTurn;

    [Header("Resources")]
    public int maxMoveCost;
    public int maxEnergy;
    
    public int MoveCost { get; private set; }
    public int Energy { get; private set; }

    private enum ETurnMode { My, Other }
    private WaitForSeconds _delay05 = YieldInstructionCache.WaitForSeconds(0.05f);
    private WaitForSeconds _delay7 = YieldInstructionCache.WaitForSeconds(0.7f);

    public static System.Action OnAddCard;
    public static event Action<bool> OnTurnStarted;

    private void Start()
    {
        GameSetUp();
    }

    public void GameSetUp()
    {
        switch (eTurnMode)
        {
            case ETurnMode.My:
                myTurn = true;
                break;
            case ETurnMode.Other:
                myTurn = false;
                break;
        }
        StartCoroutine(StartTurnCo());
    }

    public void FastMode(bool enable)
    {
        _delay7 = YieldInstructionCache.WaitForSeconds(enable ? 0.07f : 0.7f);
    }

    private IEnumerator StartTurnCo()
    {
        yield return _delay05;

        MoveCost = maxMoveCost;
        Energy = maxEnergy;
        
        OnTurnStarted?.Invoke(true);

        phase = Phase.Draw;
        for (var i = 0; i < startCardCount; i++)
        {
            yield return _delay05;
            OnAddCard?.Invoke();
        }
        yield return _delay7;

        phase = Phase.Card;
        yield return new WaitUntil(() => Energy <= 0);

        phase = Phase.End;
        CardManager.Inst.RemoveCards();
        yield return _delay7;

        GridManager.inst.StatusNode();

        var allies = UnitManager.inst.allies;
        for (var i = allies.Count - 1; i >= 0; i--)
        {
            StatusEffectManager.inst.UpdateStatusEffects(allies[i]);
        }

        phase = Phase.Enemy;

        LightManager.inst.ChangeLight(true);
        yield return _delay7;

        var enemies = UnitManager.inst.enemies;
        foreach(var enemy in enemies.FindAll(x => !x.targetUnit))
        {
            print("nonTarget");
            enemy.targetUnit = UnitManager.inst.GetNearestUnit(enemy);
        }

        var ableEnemies = enemies.FindAll(x => x.card.CardSO.useType == UseType.Able);
        var shouldEnemies = enemies.FindAll(x => x.card.CardSO.useType == UseType.Should);
        for (var i = shouldEnemies.Count - 1; i >= 0; i--)
        {
            var unit = shouldEnemies[i];

            UnitManager.inst.SelectUnit(unit);

            var canAction = unit.canAction;
            var canMove = unit.canMove;

            print(unit.name + "/ act: " + canAction + "/ move: " + canMove);
            
            StatusEffectManager.inst.UpdateStatusEffects(unit);
            StatusEffectManager.inst.UpdateRenewals(unit);

            if (canMove)
            {
                yield return StartCoroutine(UnitManager.inst.EnemyMove(unit, false));
            }
            if(canAction)
            {
                yield return StartCoroutine(UnitManager.inst.EnemyAct(unit, false));
            }
        }
        for (var i = ableEnemies.Count - 1; i >= 0; i--)
        {
            var unit = ableEnemies[i];

            var canAction = unit.canAction;
            var canMove = unit.canMove;

            print(unit.name + "/ act: " + canAction + "/ move: " + canMove);

            StatusEffectManager.inst.UpdateStatusEffects(unit);
            StatusEffectManager.inst.UpdateRenewals(unit);
            
            yield return _delay7;

            if (canMove)
            {
                yield return StartCoroutine(UnitManager.inst.EnemyMove(unit, true));
            }
            if (canAction)
            {
                yield return StartCoroutine(UnitManager.inst.EnemyAct(unit, true));
            }
            yield return _delay7;
        }
        LightManager.inst.ChangeLight(false);
        yield return _delay7;
        StartCoroutine(StartTurnCo());
    }

    public static void UseMoveCost(int value) => Inst.MoveCost = Inst.MoveCost >= value ? Inst.MoveCost - value : 0;
    public static void UseEnergy(int value) => Inst.Energy = Inst.Energy >= value ? Inst.Energy - value : 0;

    public void EndTurn()
    {
        myTurn = !myTurn;
        StartCoroutine(StartTurnCo());
    }
}