using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitArrangeManager : MonoBehaviour
{
    public static UnitArrangeManager inst;

    private void Awake()
    {
        inst = this;
    }
    
    public bool isArrange;
    public int arrangeRange;
    private UnitData _unitData;

    [SerializeField] private SpriteRenderer arrangeSR;

    private void Update()
    {
        if (!_unitData)
            return;
        
        GridManager.inst.AreaDisplay(AreaType.Arrange, true, HexDirectionExtension.Area(UnitManager.inst.commander.coords, arrangeRange), UnitManager.inst.commander);

        if(GridManager.inst.selectedNode)
        {
            arrangeSR.transform.position = GridManager.inst.selectedNode.Coords.Pos;
        }
        else
        {
            arrangeSR.transform.position = Utils.MousePos;
        }
        arrangeSR.sprite = _unitData.sprite;
    }
    public void MouseExit(UnitData unitData)
    {

    }
    public void MouseDown(UnitData unitData)
    {
        LightManager.Inst.ChangeLight(true);
        this._unitData = unitData;
    }
    public void MouseUp(UnitData unitData)
    {
        if (GridManager.inst.selectedNode)
        {
            UnitManager.inst.SpawnUnit(unitData, GridManager.inst.selectedNode);
        }

        LightManager.Inst.ChangeLight(false);
        this._unitData = null;
    }
}
