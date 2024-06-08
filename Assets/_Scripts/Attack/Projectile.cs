using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class Projectile : Action
{
    public override void Init(Unit unit, HexDirection direction, CardSO cardSO)
    {
        base.Init(unit, direction, cardSO);
        StartCoroutine(Fire(direction, cardSO));
    }

    private IEnumerator Fire(HexDirection direction, CardSO cardSO)
    {
        for (var i = 1; i <= cardSO.realRange; i++)
        {
            TargetCoords += direction.Coords();
            if (GridManager.inst.GetNode(TargetCoords)?.OnObstacle != false)
                break;

            transform.DOMove(TargetCoords.Pos, 0.05f).SetEase(Ease.Linear);

            yield return YieldInstructionCache.WaitForSeconds(0.05f);

            if(ActiveEventValue(TargetCoords, cardSO) && !cardSO.isPenetrate)
                break;
        }
        Destroy(gameObject);
    }
}
