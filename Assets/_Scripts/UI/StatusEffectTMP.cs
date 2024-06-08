using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;

public class StatusEffectTMP : MonoBehaviour
{
    [SerializeField] private TMP_Text statusTMP;
    public void Setup(Unit unit, string text)
    {
        statusTMP.text = text;
        
        transform.position = unit.coords.Pos + new Vector3(0, 1f);

        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(unit.coords.Pos + new Vector3(0, 2.5f), 1f).SetEase(Ease.OutCirc));
        sequence.Insert(0.7f, statusTMP.DOFade(0f, 0.3f).SetEase(Ease.Linear));
        Destroy(gameObject, 1.01f);
    }
}