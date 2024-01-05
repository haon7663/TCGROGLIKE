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
    public void Init(Unit unit, HexDirection direction, Item item)
    {
        coords = unit.coords;
        transform.position = unit.coords.Pos;
        StartCoroutine(Fire(direction, item));
    }
    IEnumerator Fire(HexDirection direction, Item item)
    {
        for (int i = 1; i <= item.range; i++)
        {
            if (!GridManager.Inst.Tiles.ContainsKey((coords + direction.Coords() * i).Pos))
                continue;
            transform.DOMove((coords + direction.Coords() * i).Pos, 0.05f).SetEase(Ease.Linear);
            yield return YieldInstructionCache.WaitForSeconds(0.05f);
            if(GridManager.Inst.ContainsTile(coords + direction.Coords() * i)?.OnDamage(item.value) == true && !item.isPenetrate)
                break;
        }
        Release();
    }
    public void Release()
    {
        _ManagedPool.Release(this);
    }
}
