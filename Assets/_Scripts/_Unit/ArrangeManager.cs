using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ArrangeManager : MonoBehaviour
{
    public static ArrangeManager inst;

    private void Awake()
    {
        inst = this;
    }
    
    public bool isArrange;
    public int arrangeRange;

    [SerializeField] private SpriteRenderer spriteRenderer;
    private UnitData _unitData;

    private GridManager _gridManager;
    private LightManager _lightManager;
    private UnitManager _unitManager;
    private void Start()
    {
        _gridManager = GridManager.inst;
        _lightManager = LightManager.inst;
        _unitManager = UnitManager.inst;
    }

    private void Update()
    {
        if (!isArrange)
            return;

        spriteRenderer.transform.position = _gridManager.selectedNode ? _gridManager.selectedNode.Coords.Pos : Utils.MousePos;
    }
    public void MouseExit(UnitData unitData)
    {
        
    }
    public void MouseDown(UnitData unitData)
    {
        _lightManager.ChangeLight(true);
        _unitData = unitData;
        
        _gridManager.AreaDisplay(AreaType.Arrange, true, HexDirectionExtension.Area(_unitManager.commander.coords, arrangeRange), _unitManager.commander);
        spriteRenderer.sprite = _unitData.sprite;

        isArrange = true;
    }
    public void MouseUp(UnitData unitData)
    {
        if (_gridManager.selectedNode)
        {
            _unitManager.SpawnUnit(unitData, GridManager.inst.selectedNode);
        }

        _lightManager.ChangeLight(false);
        _gridManager.RevertTiles(_unitManager.commander);
        spriteRenderer.sprite = null;
        _unitData = null;

        isArrange = false;
    }
}
