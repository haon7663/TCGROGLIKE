using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Unit_Card : MonoBehaviour
{
    Unit unit;
    IObjectPool<Projectile> _ProjectilePool;
    void Awake()
    {
        unit = GetComponent<Unit>();

        _ProjectilePool = new ObjectPool<Projectile>(CreateProjectile, OnGetProjectile, OnReleaseProjectile, OnDestroyProjectile);
    }

    [HideInInspector] public CardSO card;

    HexDirection direction;
    public void DrawArea(CardSO card)
    {
        GridManager.Inst.RevertTiles();
        this.card = card;

        switch (card.rangeType)
        {
            case RangeType.Liner:
                for (int i = 0; i < card.range; i++)
                {
                    foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                    {
                        var floorWide = Mathf.FloorToInt((float)card.lineWidth / 2);
                        for (int j = -floorWide; j <= floorWide; j++)
                        {
                            var pos = (unit.coords + hexDirection.Rotate(j).Coords() + hexDirection.Coords() * i).Pos;
                            GridManager.Inst.OnAttackSelect(pos);
                        }
                    }
                }
                break;
            case RangeType.Area:
                if (card.canSelectAll)
                    foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, card.range))
                        GridManager.Inst.OnAttackSelect(hexNode.Coords);
                else
                {
                    foreach (HexDirection hexDirection in HexDirectionExtension.Loop(HexDirection.E))
                    {
                        var pos = (unit.coords + hexDirection.Coords()).Pos;
                        GridManager.Inst.OnAttackSelect(pos);
                    }
                    foreach (HexNode hexNode in HexDirectionExtension.Area(unit.coords, card.range))
                    {
                        GridManager.Inst.OnAttackRange(hexNode.Coords);
                    }
                }
                break;
            case RangeType.Our:
                foreach (var unit in GridManager.Inst.OnTileUnits.Values.Where(t => t.unitData.type != UnitType.Enemy))
                {
                    GridManager.Inst.OnAttackSelect(GridManager.Inst.ContainsUnit(unit).Coords);
                }
                break;
        }
    }

    public List<HexNode> GetArea(HexNode hexNode)
    {
        GridManager.Inst.RevertAbles();
        List<HexNode> hexNodes = new List<HexNode>();

        direction = (hexNode.Coords - unit.coords).GetSignDirection();
        //Debug.Log(hexDirection.Coords()._q.ToString() + "/" + hexDirection.Coords()._r.ToString());
        switch (card.attackType)
        {
            case AttackType.Single:
                hexNodes.Add(hexNode);
                break;
            case AttackType.Wide:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(unit.coords, direction, card.range));
                break;
            case AttackType.Liner:
                for(int i = -card.multiShot/2; i <= card.multiShot/2; i++)
                    hexNodes.AddRange(HexDirectionExtension.Liner(unit.coords, direction.Rotate(i), card.range, card.lineWidth));
                break;
            case AttackType.Splash:
                hexNodes.AddRange(HexDirectionExtension.Area(hexNode.Coords, card.splashRange, true));
                break;
            case AttackType.Emission:
                hexNodes.AddRange(HexDirectionExtension.Diagonal(unit.coords + direction, direction, card.range - 1, true));
                break;
        }
        return hexNodes;
    }

    public bool OnAttack()
    {
        switch (card.attackType)
        {
            case AttackType.Single:
                break;
            case AttackType.Wide:
                break;
            case AttackType.Liner:
                for (int i = -card.multiShot / 2; i <= card.multiShot / 2; i++)
                {
                    var prefab = _ProjectilePool.Get();
                    prefab.Init(unit, direction.Rotate(i), card);
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
        Projectile prefab = Instantiate(card.prefab).GetComponent<Projectile>();
        prefab.SetManagedPool(_ProjectilePool);
        return prefab;
    } 
    void OnGetProjectile(Projectile prefab) => prefab.gameObject.SetActive(true);
    void OnReleaseProjectile(Projectile prefab) => prefab.gameObject.SetActive(false);
    void OnDestroyProjectile(Projectile prefab) => Destroy(prefab.gameObject);
    #endregion
}