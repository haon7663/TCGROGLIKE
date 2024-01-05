using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class Attack : MonoBehaviour
{
    IObjectPool<Attack> _ManagedPool;
    public void SetManagedPool(IObjectPool<Attack> pool) => _ManagedPool = pool;

    public HexCoords coords;
    public abstract void Init(Unit unit, HexDirection direction, Item item);
    public abstract void Init(Unit unit, HexNode hexNode, Item item);

    public void Release()
    {
        _ManagedPool.Release(this);
    }
}
