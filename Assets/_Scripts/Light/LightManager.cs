using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class LightManager : MonoBehaviour
{
    public static LightManager Inst;
    void Awake() => Inst = this;

    [Header("WorldLight")]
    [SerializeField] Light2D worldLight;
    [SerializeField] float worldIntensity;
    [Header("CardLight")]
    [SerializeField] Light2D cardLight;
    [SerializeField] float cardIntensity;

    void Start()
    {
        ChangeLight(false);
    }

    public void ChangeLight(bool onCard)
    {
        print(onCard);

        DOTween.Kill(this);
        DOTween.To(() => worldLight.intensity, x => worldLight.intensity = x, onCard ? 0f : worldIntensity, 0.3f).SetEase(Ease.OutCirc);
        DOTween.To(() => cardLight.intensity, x => cardLight.intensity = x, onCard ? cardIntensity : 0f, 0.3f).SetEase(Ease.OutCirc);
    }
}
