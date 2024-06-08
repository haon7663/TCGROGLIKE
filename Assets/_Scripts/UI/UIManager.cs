using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

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
    [SerializeField] private StatusEffectTMP statusEffectTMPPrefab;
    [SerializeField] private DamageTMP damageTMPPrefab;
    [SerializeField] private RecoveryTMP recoveryTMPPrefab;
    
    [Header("표시")]
    [SerializeField] private GameObject unitInfoPanel;
    
    [Header("자원표시")]
    [SerializeField] private TMP_Text energyTMP;
    [SerializeField] private TMP_Text moveCostTMP;

    private void Update()
    {
        energyTMP.text = TurnManager.Inst.Energy + " / " + TurnManager.Inst.maxEnergy;
        moveCostTMP.text = TurnManager.Inst.MoveCost + " / " + TurnManager.Inst.maxMoveCost;
    }

    public void ShowStatusTMP(Unit unit, StatusEffectSO statusEffectSO)
    {
        var statusTMP = Instantiate(statusEffectTMPPrefab, canvasWorld);
        statusTMP.Setup(unit, statusEffectSO.displayExplain);
    }
    public void ShowDamageTMP(Unit unit, int value)
    {
        var damageTMP = Instantiate(damageTMPPrefab, canvasWorld);
        damageTMP.Setup(unit, value);
    }
    public void ShowRecoveryTMP(Unit unit, int value)
    {
        var recoveryTMP = Instantiate(recoveryTMPPrefab, canvasWorld);
        recoveryTMP.Setup(unit, value);
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