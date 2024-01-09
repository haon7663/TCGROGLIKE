using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    IObjectPool<Projectile> _ManagedPool;
    public void SetManagedPool(IObjectPool<Projectile> pool) => _ManagedPool = pool;

    HexCoords coords;
    public void Init(Unit unit, HexDirection direction, CardSO card)
    {
        coords = unit.coords;
        transform.position = unit.coords.Pos;
        StartCoroutine(Fire(direction, card));
    }
    IEnumerator Fire(HexDirection direction, CardSO card)
    {
        for (int i = 1; i <= card.range; i++)
        {
            if (!GridManager.Inst.Tiles.ContainsKey((coords + direction.Coords() * i).Pos))
                continue;
            transform.DOMove((coords + direction.Coords() * i).Pos, 0.05f).SetEase(Ease.Linear);
            yield return YieldInstructionCache.WaitForSeconds(0.05f);
            if(GridManager.Inst.ContainsOnTileUnits(coords + direction.Coords() * i)?.OnDamage(card.value) == true && !card.isPenetrate)
                break;
        }
        Release();
    }
    public void Release()
    {
        _ManagedPool.Release(this);
    }
}
