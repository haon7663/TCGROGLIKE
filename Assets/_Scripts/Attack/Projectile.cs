using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class Projectile : Action
{
    public override void Init(Unit unit, HexDirection direction, CardSO cardSO, int value = -999)
    {
        base.Init(unit, direction, cardSO, value);
        StartCoroutine(Fire(direction, cardSO));
    }

    IEnumerator Fire(HexDirection direction, CardSO cardSO)
    {
        for (int i = 1; i <= cardSO.realRange; i++)
        {
            coords += direction.Coords();
            if (GridManager.inst.GetNode(coords)?.OnObstacle != false)
                break;

            transform.DOMove(coords.Pos, 0.05f).SetEase(Ease.Linear);

            yield return YieldInstructionCache.WaitForSeconds(0.05f);

            if(ActiveEventValue(coords, cardSO) && !cardSO.isPenetrate)
                break;
        }
        Destroy(gameObject);
    }
}
