using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class Projectile : Action
{
    public override void Init(Unit unit, HexDirection direction, CardData data, int value = -999)
    {
        base.Init(unit, direction, data, value);
        StartCoroutine(Fire(direction, data));
    }

    IEnumerator Fire(HexDirection direction, CardData data)
    {
        for (int i = 1; i <= data.realRange; i++)
        {
            coords += direction.Coords();
            if (GridManager.inst.GetNode(coords)?.OnObstacle != false)
                break;

            transform.DOMove(coords.Pos, 0.05f).SetEase(Ease.Linear);

            yield return YieldInstructionCache.WaitForSeconds(0.05f);

            if(ActiveEventValue(coords, data) && !data.isPenetrate)
                break;
        }
        Destroy(gameObject);
    }
}
