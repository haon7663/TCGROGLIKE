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

    [SerializeField] private GameObject unitInfoPanel;

    public void OpenInfoPanel()
    {
        unitInfoPanel.SetActive(true);
    }
    
    public void CloseInfoPanel()
    {
        unitInfoPanel.SetActive(false);
    }
}