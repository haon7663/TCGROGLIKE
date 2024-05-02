using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitArrangeManager : MonoBehaviour
{
    public static UnitArrangeManager Inst;
    void Awake() => Inst = this;

    public UnitSO unitData;
    public bool isArrange;
    public int arrangeRange;
    bool isArranged;

    [SerializeField] SpriteRenderer arrangeSR;

    void Update()
    {
        if(!isArrange)
        {
            if (!isArranged)
            {
                isArranged = true;
                arrangeSR.sprite = null;
                GridManager.Inst.RevertTiles(UnitManager.Inst.Commander);
            }
            return;
        }

        if(unitData)
        {
            if(isArranged)
            {
                isArranged = false;
                GridManager.Inst.AreaDisplay(AreaType.Arrange, true, HexDirectionExtension.Area(UnitManager.Inst.Commander.coords, arrangeRange), UnitManager.Inst.Commander);
            }

            if(GridManager.Inst.selectedNode)
            {
                arrangeSR.transform.position = GridManager.Inst.selectedNode.coords.Pos;
            }
            else
            {
                arrangeSR.transform.position = Utils.MousePos;
            }
            arrangeSR.sprite = unitData.sprite;
        }
    }
    public void MouseExit(UnitSO unitData)
    {

    }
    public void MouseDown(UnitSO unitData)
    {
        LightManager.Inst.ChangeLight(true);
        isArrange = true;
        this.unitData = unitData;
    }
    public void MouseUp(UnitSO unitData)
    {
        if (GridManager.Inst.selectedNode)
        {
            UnitManager.Inst.SpawnUnit(unitData, GridManager.Inst.selectedNode);
        }

        LightManager.Inst.ChangeLight(false);
        isArrange = false;
        this.unitData = null;
    }
}
