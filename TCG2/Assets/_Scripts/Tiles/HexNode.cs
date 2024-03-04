using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public enum SelectOutline
{
    Selected, MoveSelect, MoveAble, AttackSelect, DamageAble, BuffSelect, BuffAble, Default
}

public class HexNode : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject selectedObject;
    [SerializeField] GameObject moveSelectObject;
    [SerializeField] GameObject moveAbleObject;
    [SerializeField] GameObject attackSelectObject;
    [SerializeField] GameObject damageAbleObject;
    [SerializeField] GameObject buffSelectObject;
    [SerializeField] GameObject buffAbleObject;

    [SerializeField] TMP_Text coordsText;
    [SerializeField] TMP_Text damageText;

    public HexCoords coords;

    public Unit unit;

    public float GetDistance(HexNode other) => coords.GetDistance(other.coords); // Helper to reduce noise in pathfinding
    public bool onObstacle, onUnit;
    public bool canMove, canAttack, canDamaged;

    public List<StatusInfo> statuses;

    public virtual void Init(bool walkable, HexCoords coords)
    {
        onObstacle = !walkable;

        spriteRenderer.enabled = walkable;
        spriteRenderer.sortingOrder = -coords._r;

        this.coords = coords;
        coordsText.text = "q: " + coords._q + ", r: " + coords._r + "  s: " + coords._s;
        transform.position = this.coords.Pos;
    }

    void OnMouseDown()
    {
        if (!CanWalk()) return;

        if (canMove)
        {
            UnitManager.sUnit_Move.OnMove(coords);
        }
        else if (canAttack)
        {
            UnitManager.sUnit_Card.UseCard(this);
        }
    }
    void OnMouseOver()
    {
        bool canMoveOrAttack = canMove || canAttack;

        GridManager.Inst.SelectNode(this, canMoveOrAttack);

        if (canMoveOrAttack)
            UnitManager.sUnit.Repeat(this);

        if (canMove)
        {
            UnitManager.sUnit_Move.TouchArea(this).OnDisplay(SelectOutline.Selected);
        }
        else if (canAttack)
        {
            var selected = UnitManager.sUnit_Card.GetSelectedArea(this);
            foreach (HexNode hexNode in selected)
                hexNode.OnDisplay(SelectOutline.Selected, selected);
        }
    }

    void OnMouseExit()
    {
        if (!canMove && !canAttack) return;

        GridManager.Inst.RevertAbles();
    }

    public void OnUnit(Unit unit, bool isRemove = false)
    {
        this.unit = isRemove ? null : unit;
        spriteRenderer.color = isRemove ? Color.white : new Color(1, 0.75f, 0.75f);
    }

    public void OnDisplay(SelectOutline selectLine, List<HexNode> nodes)
    {
        switch (selectLine)
        {
            case SelectOutline.Selected:
                DisplayDamaged(UnitManager.sUnit);
                SetOutline(selectedObject, nodes);
                break;
            case SelectOutline.MoveSelect:
                canMove = true;
                SetOutline(moveSelectObject, nodes);
                break;
            case SelectOutline.MoveAble:
                SetOutline(moveAbleObject, nodes);
                break;
            case SelectOutline.AttackSelect:
                canAttack = true;
                SetOutline(attackSelectObject, nodes);
                break;
            case SelectOutline.DamageAble:
                SetOutline(damageAbleObject, nodes);
                break;
            case SelectOutline.BuffSelect:
                canAttack = true;
                SetOutline(buffSelectObject, nodes);
                break;
            case SelectOutline.BuffAble:
                SetOutline(buffAbleObject, nodes);
                break;
        }
    }
    public void SetOutline(GameObject outlineObject, List<HexNode> nodes)
    {
        outlineObject.SetActive(true);
        foreach (HexDirection direction in HexDirectionExtension.Loop(HexDirection.EN))
        {
            outlineObject.transform.GetChild((int)direction).gameObject.SetActive(!nodes.Contains(GridManager.Inst.GetTile(coords + direction.Coords())));
        }
    }

    public void OnDisplay(SelectOutline selectLine)
    {
        switch (selectLine)
        {
            case SelectOutline.Selected:
                DisplayDamaged(UnitManager.sUnit);
                SetOutline(selectedObject);
                break;
            case SelectOutline.MoveSelect:
                canMove = true;
                SetOutline(moveSelectObject);
                break;
            case SelectOutline.MoveAble:
                SetOutline(moveAbleObject);
                break;
            case SelectOutline.AttackSelect:
                canAttack = true;
                SetOutline(attackSelectObject);
                break;
            case SelectOutline.DamageAble:
                SetOutline(damageAbleObject);
                break;
            case SelectOutline.BuffSelect:
                canAttack = true;
                SetOutline(buffSelectObject);
                break;
            case SelectOutline.BuffAble:
                SetOutline(buffAbleObject);
                break;
        }
    }
    void SetOutline(GameObject outlineObject)
    {
        outlineObject.SetActive(true);
        for(int i = 0; i < 6; i++)
            outlineObject.transform.GetChild(i).gameObject.SetActive(true);
    }

    public void DisplayDamaged(Unit unit)
    {
        if(onUnit)
        {
            damageText.gameObject.SetActive(true);
            damageText.text = StatusManager.Calculate(unit, GridManager.Inst.OnTileUnits[coords.Pos], unit.card.data).ToString();
            damageText.GetComponent<Renderer>().sortingLayerName = "UI";
        }
    }

    public void RevertTile()
    {
        moveSelectObject.SetActive(false);
        moveAbleObject.SetActive(false);
        attackSelectObject.SetActive(false);
        damageAbleObject.SetActive(false);
        buffSelectObject.SetActive(false);
        buffAbleObject.SetActive(false);
        damageText.gameObject.SetActive(false);

        canMove = false;
        canAttack = false;
        canDamaged = false;
    }

    public void RevertAble()
    {
        selectedObject.SetActive(false);
        damageText.gameObject.SetActive(false);
    }

    public bool CanWalk() => !onObstacle && !onUnit;
    public bool CanPass() => !onObstacle && !onUnit;


    #region Pathfinding
    public List<HexNode> Neighbors { get; protected set; }
    public HexNode Connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    public void CacheNeighbors()
    {
        Neighbors = GridManager.Inst.Tiles.Where(t => coords.GetDistance(t.Value.coords) == 1).Select(t => t.Value).ToList();
    }

    public void SetConnection(HexNode nodeBase)
    {
        Connection = nodeBase;
    }

    public void SetG(float g)
    {
        G = g;
    }

    public void SetH(float h)
    {
        H = h;
    }

    #endregion
}

public struct HexCoords
{
    public int _q { get; set; }
    public int _r { get; set; }
    public int _s { get; set; }

    public HexCoords(int q, int r)
    {
        _q = q;
        _r = r;
        _s = -q - r;
        Pos = _q * new Vector2(HexSize * 2, 0) + _r * new Vector2(HexSize, HexSize);
    }
    public HexCoords(float q, float r)
    {
        int sq = (int)q;
        int sr = (int)r;
        _q = sq;
        _r = sr;
        _s = -sq - sr;
        Pos = sq * new Vector2(HexSize * 2, 0) + sr * new Vector2(HexSize, HexSize);
    }

    public static HexCoords operator +(HexCoords a, HexCoords b)
        => new HexCoords(a._q + b._q, a._r + b._r);

    public static HexCoords operator +(HexCoords a, HexDirection b)
        => a + b.Coords();

    public static HexCoords operator +(HexDirection a, HexCoords b)
        => a.Coords() + b;

    public static HexCoords operator -(HexCoords a, HexCoords b)
        => new HexCoords(a._q - b._q, a._r - b._r);

    public static HexCoords operator -(HexCoords a, HexDirection b)
        => a - b.Coords();

    public static HexCoords operator -(HexDirection a, HexCoords b)
        => a.Coords() - b;

    public static HexCoords operator *(HexCoords a, int b)
        => new HexCoords(a._q * b, a._r * b);

    public static HexCoords operator *(int a, HexCoords b)
        => b * a;

    public static bool operator ==(HexCoords a, HexCoords b)
        => a._q == b._q && a._r == b._r;

    public static bool operator !=(HexCoords a, HexCoords b)
        => a._q != b._q || a._r != b._r;

    public float GetDistance(HexCoords other) => (this - other).AxialLength();

    public int GetPathDistance(HexCoords other) => Pathfinding.FindPathDistance(GridManager.Inst.GetTile(this), GridManager.Inst.GetTile(other));

    private static readonly float Sqrt3 = Mathf.Sqrt(3);

    private static readonly float HexSize = 15f / 16;

    public Vector3 Pos { get; set; }

    private int AxialLength()
    {
        if (_q == 0 && _r == 0) return 0;
        if (_q > 0 && _r >= 0) return _q + _r;
        if (_q <= 0 && _r > 0) return -_q < _r ? _r : -_q;
        if (_q < 0) return -_q - _r;
        return -_r > _q ? -_r : _q;
    }

    public override bool Equals(object obj)
    {
        return obj is HexCoords coords &&
               _q == coords._q &&
               _r == coords._r &&
               _s == coords._s;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(_q, _r, _s);
    }

    public void DebugQRS()
    {
        Debug.Log("Q: " + _q + " / R: " + _r + " / S: " + _s);
    }
}