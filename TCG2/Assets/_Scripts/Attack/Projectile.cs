using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class Projectile : Attack
{
    public override void Init(Unit unit, HexDirection direction, CardSO card)
    {
        coords = unit.coords;
        transform.position = unit.coords.Pos;
        StartCoroutine(Fire(direction, card));
    }
    public override void Init(Unit unit, HexNode node, CardSO card)
    {
        Destroy(gameObject);
    }

    IEnumerator Fire(HexDirection direction, CardSO card)
    {
        for (int i = 1; i <= card.range; i++)
        {
            coords += direction.Coords();
            if (GridManager.Inst.GetTile(coords)?.onObstacle != false)
                break;

            transform.DOMove(coords.Pos, 0.05f).SetEase(Ease.Linear);

            yield return YieldInstructionCache.WaitForSeconds(0.05f);

            if(ActiveEventValue(coords, card) && !card.isPenetrate)
                break;
        }
        Destroy(gameObject);
    }
}
