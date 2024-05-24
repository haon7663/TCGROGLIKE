using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public enum Paze { Draw, Card, End, Enemy }
public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("Develop")]
    [SerializeField][Tooltip("시작 턴 모드를 정합니다")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("시작 카드 개수를 정합니다")] int startCardCount;

    [Header("Properties")]
    public Paze paze = Paze.Draw;
    public bool myTurn;

    [Header("Resources")]
    public int maxMoveCost;
    public int maxEnergy;
    public int CommanderCost { get; private set; }
    public int Energy { get; private set; }

    enum ETurnMode { My, Other }
    readonly WaitForSeconds delay05 = YieldInstructionCache.WaitForSeconds(0.05f);
    readonly WaitForSeconds delay7 = YieldInstructionCache.WaitForSeconds(0.7f);

    public static System.Action OnAddCard;
    public static event Action<bool> OnTurnStarted;

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

    IEnumerator StartTurnCo()
    {
        yield return delay05;

        CommanderCost = maxMoveCost;
        Energy = maxEnergy;
        
        OnTurnStarted?.Invoke(true);

        paze = Paze.Draw;
        for (var i = 0; i < startCardCount; i++)
        {
            yield return delay05;
            OnAddCard?.Invoke();
        }
        yield return delay7;

        paze = Paze.Card;
        yield return new WaitUntil(() => Energy <= 0);

        paze = Paze.End;
        CardManager.Inst.RemoveCards();
        yield return delay7;

        GridManager.inst.StatusNode();
        
        paze = Paze.Enemy;

        LightManager.inst.ChangeLight(true);
        yield return delay7;

        var enemies = UnitManager.inst.enemies;
        var ableEnemies = enemies.FindAll(x => x.card.CardData.useType == UseType.Able);
        var shouldEnemies = enemies.FindAll(x => x.card.CardData.useType == UseType.Should);
        for (var i = shouldEnemies.Count - 1; i >= 0; i--)
        {
            var unit = shouldEnemies[i];

            UnitManager.inst.SelectUnit(unit);

            bool canAction = StatusManager.CanAction(unit);
            bool canMove = StatusManager.CanMove(unit);

            print(unit.name + "/ act: " + canAction + "/ move: " + canMove);

            if(canMove)
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

            bool canAction = StatusManager.CanAction(unit);
            bool canMove = StatusManager.CanMove(unit);

            print(unit.name + "/ act: " + canAction + "/ move: " + canMove);

            StatusManager.Inst.StatusActive(unit);

            if (canMove)
            {
                yield return StartCoroutine(UnitManager.inst.EnemyMove(unit, true));
            }
            if (canAction)
            {
                yield return StartCoroutine(UnitManager.inst.EnemyAct(unit, true));
            }
            yield return delay7;
        }
        LightManager.inst.ChangeLight(false);
        yield return delay7;
        StartCoroutine(StartTurnCo());
    }

    public static void UseMoveCost(int value) => Inst.CommanderCost = Inst.CommanderCost >= value ? Inst.CommanderCost - value : 0;
    public static void UseEnergy(int value) => Inst.Energy = Inst.Energy >= value ? Inst.Energy - value : 0;

    public void EndTurn()
    {
        myTurn = !myTurn;
        StartCoroutine(StartTurnCo());
    }
}