using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Unit_Attack : MonoBehaviour
{
    Unit unit;
    IObjectPool<Projectile> _ProjectilePool;
    void Awake()
    {
        unit = GetComponent<Unit>();

        _ProjectilePool = new ObjectPool<Projectile>(CreateProjectile, OnGetProjectile, OnReleaseProjectile, OnDestroyProjectile);
    }

    public Item item;

    HexDirection direction;
    public void OnDrawArea(Item item)
    {
        GridManager.Inst.RevertTiles();
        this.item = item;

        if (item.rangeType == RangeType.Liner)
        {
            for (int i = 0; i < item.range; i++)
            {
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var floorWide = Mathf.FloorToInt((float)item.lineWidth / 2);
                    for (int j = -floorWide; j <= floorWide; j++)
                    {
                        var pos = (unit.coords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i).Pos;
                        GridManager.Inst.OnAttackSelect(pos);
                    }
                }
            }
        }
        else if (item.rangeType == RangeType.Area)
        {
            if (item.canSelectAll)
                foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, item.range))
                    GridManager.Inst.OnAttackSelect(hexNode.Coords);
            else
            {
                foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                {
                    var pos = (unit.coords + hexDirection.Coords()).Pos;
                    GridManager.Inst.OnAttackSelect(pos);
                }
                foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, item.range))
                {
                    GridManager.Inst.OnAttackRange(hexNode.Coords);
                }
            }
        }
    }

    public List<HexNode> GetArea(HexNode hexNode)
    {
        GridManager.Inst.RevertAbles();
        List<HexNode> hexNodes = new List<HexNode>();

        direction = (hexNode.Coords - unit.coords).GetSignDirection();
        //Debug.Log(hexDirection.Coords()._q.ToString() + "/" + hexDirection.Coords()._r.ToString());
        switch (item.attackType)
        {
            case AttackType.Single:
                hexNodes.Add(hexNode);
                break;
            case AttackType.Wide:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(unit.coords, direction, item.range));
                break;
            case AttackType.Liner:
                for(int i = -item.multiShot/2; i <= item.multiShot/2; i++)
                    hexNodes.AddRange(HexDirectionExtension.Liner(unit.coords, direction.Rotate(i), item.range, item.lineWidth));
                break;
            case AttackType.Splash:
                hexNodes.AddRange(HexDirectionExtension.Area(hexNode.Coords, item.splashRange, true));
                break;
            case AttackType.Emission:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(unit.coords + direction, direction, item.range - 1, true));
                break;
        }
        return hexNodes;
    }

    public bool OnAttack()
    {
        switch (item.attackType)
        {
            case AttackType.Single:
                break;
            case AttackType.Wide:
                break;
            case AttackType.Liner:
                for (int i = -item.multiShot / 2; i <= item.multiShot / 2; i++)
                {
                    var prefab = _ProjectilePool.Get();
                    prefab.Init(unit, direction.Rotate(i), item);
                }
                break;
            case AttackType.Splash:
                break;
            case AttackType.Emission:
                break;
        }

        GridManager.Inst.RevertTiles();
        return true;
    }

    #region ProjectilePool
    Projectile CreateProjectile()
    {
        Projectile prefab = Instantiate(item.prefab).GetComponent<Projectile>();
        prefab.SetManagedPool(_ProjectilePool);
        return prefab;
    } 
    void OnGetProjectile(Projectile prefab) => prefab.gameObject.SetActive(true);
    void OnReleaseProjectile(Projectile prefab) => prefab.gameObject.SetActive(false);
    void OnDestroyProjectile(Projectile prefab) => Destroy(prefab.gameObject);
    #endregion
}