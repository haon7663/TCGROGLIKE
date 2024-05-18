using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Serialization;

public class RecoveryTMP : MonoBehaviour
{
    [SerializeField] private TMP_Text recoveryTMP;
    
    public void Setup(Unit unit, int value)
    {
        if(value < 0)
            DestroyImmediate(gameObject);
        
        recoveryTMP.text = value.ToString();
        
        var startPosition = unit.coords.Pos + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.3f, 1f));
        transform.position = startPosition;

        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOMoveY(startPosition.y + 2f, 1f).SetEase(Ease.OutCirc));
        sequence.Insert(0.7f, recoveryTMP.DOFade(0f, 0.3f).SetEase(Ease.Linear));
        Destroy(gameObject, 1.01f);
    }
}
