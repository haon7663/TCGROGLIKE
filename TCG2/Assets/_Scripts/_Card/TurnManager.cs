using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public enum Paze { Draw, Commander, Card, End, Enemy }
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
    public int MoveCost { get; private set; }
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

        MoveCost = maxMoveCost;
        Energy = maxEnergy;

        for (int i = UnitManager.Inst.Enemies.Count - 1; i >= 0; i--)
        {
            yield return StartCoroutine(UnitManager.Inst.AutoSelectCard(UnitManager.Inst.Enemies[i]));
        }

        paze = Paze.Draw;
        for (int i = 0; i < startCardCount; i++)
        {
            yield return delay05;
            OnAddCard?.Invoke();
        }
        yield return delay7;

        paze = Paze.Commander;
        yield return new WaitUntil(() => MoveCost <= 0);

        paze = Paze.Card;
        yield return new WaitUntil(() => Energy <= 0);

        paze = Paze.End;
        CardManager.Inst.RemoveCards();
        yield return delay7;

        GridManager.Inst.StatusNode();

        for (int i = UnitManager.Inst.Units.Count - 1; i >= 0; i--)
        {
            StatusManager.Inst.StatusActive(UnitManager.Inst.Units[i]);
            yield return delay05;
        }

        paze = Paze.Enemy;

        var enemies = UnitManager.Inst.Enemies.OrderByDescending(x => x.coords.GetPathDistance(UnitManager.Inst.GetNearestUnit2(x).coords)).ToList();
        var ableEnemies = enemies.FindAll(x => x.card.data.useType == UseType.Able);
        var shouldEnemies = enemies.FindAll(x => x.card.data.useType == UseType.Should);
        for (int i = shouldEnemies.Count - 1; i >= 0; i--)
        {
            yield return StartCoroutine(UnitManager.Inst.Action(shouldEnemies[i], false));
        }
        for (int i = ableEnemies.Count - 1; i >= 0; i--)
        {
            yield return StartCoroutine(UnitManager.Inst.Action(ableEnemies[i], true));
            yield return delay7;
        }

        yield return delay7;
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