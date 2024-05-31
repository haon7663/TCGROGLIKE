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
    private UnitSO _unitSO;

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

        spriteRenderer.transform.position = _gridManager.selectedNode ? _gridManager.selectedNode.Coords.Pos + new Vector3(0, 1.25f) : Utils.MousePos;
    }
    public void MouseExit(UnitSO unitSO)
    {
        
    }
    public void MouseDown(UnitSO unitSO)
    {
        _lightManager.ChangeLight(true);
        _unitSO = unitSO;
        
        _gridManager.AreaDisplay(AreaType.Arrange, true, HexDirectionExtension.Area(_unitManager.commander.coords, arrangeRange), _unitManager.commander);
        spriteRenderer.sprite = _unitSO.sprite;

        isArrange = true;
    }
    public void MouseUp(UnitSO unitSO)
    {
        if (_gridManager.selectedNode)
        {
            _unitManager.SpawnUnit(unitSO, GridManager.inst.selectedNode);
        }

        _lightManager.ChangeLight(false);
        _gridManager.RevertTiles(_unitManager.commander);
        spriteRenderer.sprite = null;
        _unitSO = null;

        isArrange = false;
    }
}
