using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class LightManager : MonoBehaviour
{
    public static LightManager inst;
    void Awake() => inst = this;

    [Header("WorldLight")]
    [SerializeField] Light2D worldLight;
    [SerializeField] float worldIntensity;
    [Header("CardLight")]
    [SerializeField] Light2D selectLight;
    [SerializeField] float selectIntensity;

    void Start()
    {
        ChangeLight(false);
    }

    public void ChangeLight(bool isSeleted)
    {
        DOTween.Kill(this);
        DOTween.To(() => worldLight.intensity, x => worldLight.intensity = x, isSeleted ? 0f : worldIntensity, 0.3f).SetEase(Ease.OutCirc);
        DOTween.To(() => selectLight.intensity, x => selectLight.intensity = x, isSeleted ? selectIntensity : 0f, 0.3f).SetEase(Ease.OutCirc);
    }
}
