using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager inst;
    private void Awake()
    {
        inst = this;
    }

    [Header("캔버스 트랜스폼")] 
    [SerializeField] private Transform canvasCamera;
    [SerializeField] private Transform canvasWorld;

    [Header("프리팹")]
    [SerializeField] private StatusTMP statusTextPrefab;
    [SerializeField] private DamageTMP damageTextPrefab;
    
    [Header("표시")]
    [SerializeField] private GameObject unitInfoPanel;

    public void ShowStatusTMP(Unit unit, StatusSO statusData)
    {
        var statusTMP = Instantiate(statusTextPrefab, canvasWorld);
        statusTMP.Setup(unit, statusData.displayExplain);
    }
    
    public void ShowDamageTMP(Unit unit, int value)
    {
        var damageTMP = Instantiate(damageTextPrefab, canvasWorld);
        damageTMP.Setup(unit, value);
    }

    public void OpenInfoPanel()
    {
        unitInfoPanel.SetActive(true);
    }
    
    public void CloseInfoPanel()
    {
        unitInfoPanel.SetActive(false);
    }
}